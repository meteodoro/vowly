import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { mensagemDeErro } from '../../core/api/erros.util';
import { CATEGORIAS, Despesa, NovaDespesa, StatusDespesa } from '../../core/models/despesa.model';
import { DespesasService } from '../../core/despesas/despesa.service';
import { formatarData } from '../../core/despesas/parcelas.util';
import { NovaDespesaModal } from './nova-despesa.modal';

type FiltroTipo = 'todas' | 'parceladas' | 'avista';
type FiltroStatus = 'todos' | StatusDespesa;
type IconeCategoria = 'buffet' | 'decoracao' | 'fotografia' | 'vestido' | 'musica' | 'outro';

const TAMANHO_PAGINA = 5;

const ROTULOS_STATUS: Record<StatusDespesa, string> = {
  paga: 'Paga',
  a_pagar: 'A pagar',
  em_pagamento: 'Em pagamento',
  atrasada: 'Atrasada',
};

const CLASSES_STATUS: Record<StatusDespesa, string> = {
  paga: 'bg-[#A7B88F]/30 text-[#5F7048]',
  a_pagar: 'bg-[#D98F86]/20 text-[#A8564C]',
  em_pagamento: 'bg-[#E6C579]/35 text-[#8A6A1E]',
  atrasada: 'bg-[#D98F86]/40 text-[#8F3A30]',
};

// Lista de despesas com filtros, paginação e o modal "Nova despesa".
@Component({
  selector: 'app-despesas',
  standalone: true,
  imports: [NovaDespesaModal],
  templateUrl: './despesas.page.html',
})
export class DespesasPage {
  private readonly despesasService = inject(DespesasService);
  private readonly router = inject(Router);
  private readonly rota = inject(ActivatedRoute);

  protected readonly categorias = CATEGORIAS;
  protected readonly statusOpcoes = (Object.keys(ROTULOS_STATUS) as StatusDespesa[]).map((valor) => ({
    valor,
    rotulo: ROTULOS_STATUS[valor],
  }));

  protected readonly despesas = signal<Despesa[]>([]);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);

  protected readonly filtroTipo = signal<FiltroTipo>('todas');
  protected readonly filtroCategoria = signal<string>('todas');
  protected readonly filtroStatus = signal<FiltroStatus>('todos');
  protected readonly pagina = signal(1);

  protected readonly modalAberto = signal(false);
  protected readonly salvando = signal(false);
  protected readonly erroModal = signal<string | null>(null);

  protected readonly filtradas = computed(() => {
    const tipo = this.filtroTipo();
    const categoria = this.filtroCategoria();
    const status = this.filtroStatus();

    return this.despesas()
      .filter((d) => tipo === 'todas' || (tipo === 'parceladas' ? d.parcelas > 1 : d.parcelas === 1))
      .filter((d) => categoria === 'todas' || d.categoria === categoria)
      .filter((d) => status === 'todos' || d.status === status)
      .sort((a, b) => a.vencimento.localeCompare(b.vencimento));
  });

  protected readonly totalPaginas = computed(() =>
    Math.max(1, Math.ceil(this.filtradas().length / TAMANHO_PAGINA)),
  );
  protected readonly paginas = computed(() =>
    Array.from({ length: this.totalPaginas() }, (_, i) => i + 1),
  );
  protected readonly itensPagina = computed(() => {
    const inicio = (this.pagina() - 1) * TAMANHO_PAGINA;
    return this.filtradas().slice(inicio, inicio + TAMANHO_PAGINA);
  });
  protected readonly primeiroItem = computed(() =>
    this.filtradas().length ? (this.pagina() - 1) * TAMANHO_PAGINA + 1 : 0,
  );
  protected readonly ultimoItem = computed(() =>
    Math.min(this.pagina() * TAMANHO_PAGINA, this.filtradas().length),
  );

  constructor() {
    this.carregar();

    // O botão "Nova despesa" do resumo chega aqui com ?nova=1 e já abre o modal.
    if (this.rota.snapshot.queryParamMap.has('nova')) {
      this.abrirModal();
      void this.router.navigate([], {
        relativeTo: this.rota,
        queryParams: { nova: null },
        queryParamsHandling: 'merge',
        replaceUrl: true,
      });
    }
  }

  // ---------- Filtros e paginação ----------
  protected mudarTipo(evento: Event): void {
    this.filtroTipo.set(this.valor(evento) as FiltroTipo);
    this.pagina.set(1);
  }

  protected mudarCategoria(evento: Event): void {
    this.filtroCategoria.set(this.valor(evento));
    this.pagina.set(1);
  }

  protected mudarStatus(evento: Event): void {
    this.filtroStatus.set(this.valor(evento) as FiltroStatus);
    this.pagina.set(1);
  }

  protected irParaPagina(numero: number): void {
    this.pagina.set(Math.min(Math.max(1, numero), this.totalPaginas()));
  }

  // ---------- Modal ----------
  protected abrirModal(): void {
    this.erroModal.set(null);
    this.modalAberto.set(true);
  }

  protected fecharModal(): void {
    if (this.salvando()) return;
    this.modalAberto.set(false);
  }

  protected salvar(dados: NovaDespesa): void {
    this.erroModal.set(null);
    this.salvando.set(true);

    this.despesasService
      .criar(dados)
      .pipe(finalize(() => this.salvando.set(false)))
      .subscribe({
        next: (nova) => {
          this.despesas.update((lista) => [...lista, nova]);
          this.modalAberto.set(false);
          this.pagina.set(1);
        },
        error: (erro: unknown) => this.erroModal.set(mensagemDeErro(erro)),
      });
  }

  // ---------- Apresentação ----------
  protected moeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
  }

  protected data(iso: string): string {
    return formatarData(iso);
  }

  protected rotuloStatus(status: StatusDespesa): string {
    return ROTULOS_STATUS[status];
  }

  protected classeStatus(status: StatusDespesa): string {
    return CLASSES_STATUS[status];
  }

  protected icone(categoria: string): IconeCategoria {
    switch (categoria) {
      case 'Buffet':
        return 'buffet';
      case 'Decoração':
        return 'decoracao';
      case 'Fotografia':
        return 'fotografia';
      case 'Vestido':
        return 'vestido';
      case 'Música':
        return 'musica';
      default:
        return 'outro';
    }
  }

  protected valor(evento: Event): string {
    return (evento.target as HTMLSelectElement).value;
  }

  private carregar(): void {
    this.despesasService
      .listar()
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (lista) => this.despesas.set(lista),
        error: (erro: unknown) => this.erro.set(mensagemDeErro(erro)),
      });
  }
}
