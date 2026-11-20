import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { mensagemDeErro } from '../../core/api/erros.util';
import { AssinaturaService } from '../../core/assinatura/assinatura.service';
import { FornecedoresService } from '../../core/fornecedores/fornecedores.service';
import { linkWhatsapp } from '../../core/fornecedores/fornecedores.util';
import { NovaDespesa } from '../../core/models/despesa.model';
import { DadosFornecedor, Fornecedor, STATUS_FORNECEDOR, StatusFornecedor } from '../../core/models/fornecedor.model';
import { ComparacaoFornecedores } from './comparacao-fornecedores';
import { ContratarFornecedorModal } from './contratar-fornecedor.modal';
import { FornecedorModal } from './fornecedor.modal';

type Filtro = 'todos' | StatusFornecedor;

const ROTULO_STATUS: Record<StatusFornecedor, string> = {
  cotando: 'Cotando',
  contratado: 'Contratado',
  descartado: 'Descartado',
};

const CLASSE_STATUS: Record<StatusFornecedor, string> = {
  cotando: 'bg-[#E6C579]/40 text-[#8A6A1E]',
  contratado: 'bg-[#A7B88F]/30 text-[#4F6038]',
  descartado: 'bg-bege/50 text-carvao/70',
};

// Cadastro de fornecedores. Recurso exclusivo do plano Premium.
@Component({
  selector: 'app-fornecedores',
  standalone: true,
  imports: [RouterLink, FornecedorModal, ComparacaoFornecedores, ContratarFornecedorModal],
  templateUrl: './fornecedores.page.html',
})
export class FornecedoresPage {
  private readonly assinaturaService = inject(AssinaturaService);
  private readonly fornecedoresService = inject(FornecedoresService);

  protected readonly filtros: { valor: Filtro; rotulo: string }[] = [
    { valor: 'todos', rotulo: 'Todos' },
    ...STATUS_FORNECEDOR,
  ];

  protected readonly premium = signal(false);
  protected readonly fornecedores = signal<Fornecedor[]>([]);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);

  protected readonly busca = signal('');
  protected readonly filtro = signal<Filtro>('todos');

  protected readonly modalAberto = signal(false);
  protected readonly emEdicao = signal<Fornecedor | null>(null);
  protected readonly salvando = signal(false);
  protected readonly erroModal = signal<string | null>(null);

  protected readonly visiveis = computed(() => {
    const texto = this.busca().trim().toLowerCase();
    const filtro = this.filtro();

    return this.fornecedores()
      .filter((f) => filtro === 'todos' || f.status === filtro)
      .filter(
        (f) =>
          !texto ||
          [f.nome, f.categoria, f.contato ?? ''].some((campo) => campo.toLowerCase().includes(texto)),
      )
      .sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR'));
  });

  // ---------- Comparação ----------
  private readonly idsSelecionados = signal<number[]>([]);
  protected readonly comparando = signal(false);
  /** Fornecedor que a pessoa clicou em "Escolher esse", aguardando a confirmação da despesa. */
  protected readonly escolhido = signal<Fornecedor | null>(null);
  protected readonly escolhendo = signal(false);
  protected readonly erroEscolha = signal<string | null>(null);
  protected readonly mensagemSucesso = signal<string | null>(null);

  /** Fornecedores marcados para comparar (no máximo 2, sempre da mesma categoria). */
  protected readonly selecionados = computed(() =>
    this.idsSelecionados()
      .map((id) => this.fornecedores().find((f) => f.id === id))
      .filter((f): f is Fornecedor => f !== undefined),
  );

  /** O outro fornecedor da comparação, que será descartado se o escolhido for contratado. */
  protected readonly descartado = computed(
    () => this.selecionados().find((f) => f.id !== this.escolhido()?.id) ?? null,
  );

  protected readonly contratados = computed(
    () => this.fornecedores().filter((f) => f.status === 'contratado').length,
  );

  constructor() {
    this.assinaturaService.obter().subscribe({
      next: (assinatura) => {
        const premium = assinatura.plano === 'premium';
        this.premium.set(premium);
        if (premium) this.carregar();
        else this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erro.set(mensagemDeErro(erro));
        this.carregando.set(false);
      },
    });
  }

  private carregar(): void {
    this.fornecedoresService
      .listar()
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (lista) => this.fornecedores.set(lista),
        error: (erro: unknown) => this.erro.set(mensagemDeErro(erro)),
      });
  }

  protected abrirNovo(): void {
    this.emEdicao.set(null);
    this.erroModal.set(null);
    this.modalAberto.set(true);
  }

  protected abrirEdicao(fornecedor: Fornecedor): void {
    this.emEdicao.set(fornecedor);
    this.erroModal.set(null);
    this.modalAberto.set(true);
  }

  protected fecharModal(): void {
    if (this.salvando()) return;
    this.modalAberto.set(false);
  }

  protected salvar(dados: DadosFornecedor): void {
    if (this.salvando()) return;
    this.salvando.set(true);
    this.erroModal.set(null);

    const atual = this.emEdicao();
    const chamada = atual
      ? this.fornecedoresService.atualizar(atual.id, dados)
      : this.fornecedoresService.criar(dados);

    chamada.pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: (salvo) => {
        this.fornecedores.update((lista) =>
          atual ? lista.map((f) => (f.id === salvo.id ? salvo : f)) : [...lista, salvo],
        );
        this.modalAberto.set(false);
      },
      error: (erro: unknown) => this.erroModal.set(mensagemDeErro(erro)),
    });
  }

  protected excluir(): void {
    const atual = this.emEdicao();
    if (!atual || this.salvando()) return;
    this.salvando.set(true);
    this.erroModal.set(null);

    this.fornecedoresService
      .excluir(atual.id)
      .pipe(finalize(() => this.salvando.set(false)))
      .subscribe({
        next: () => {
          this.fornecedores.update((lista) => lista.filter((f) => f.id !== atual.id));
          this.idsSelecionados.update((ids) => ids.filter((id) => id !== atual.id));
          this.modalAberto.set(false);
        },
        error: (erro: unknown) => this.erroModal.set(mensagemDeErro(erro)),
      });
  }

  /** Só dá para marcar outro fornecedor da mesma categoria do primeiro marcado, até o limite de 2. */
  protected podeSelecionar(fornecedor: Fornecedor): boolean {
    const marcados = this.selecionados();
    if (marcados.some((f) => f.id === fornecedor.id)) return true;
    return marcados.length < 2 && (marcados.length === 0 || marcados[0].categoria === fornecedor.categoria);
  }

  protected estaSelecionado(fornecedor: Fornecedor): boolean {
    return this.idsSelecionados().includes(fornecedor.id);
  }

  protected alternarSelecao(fornecedor: Fornecedor): void {
    if (!this.podeSelecionar(fornecedor)) return;
    this.idsSelecionados.update((ids) =>
      ids.includes(fornecedor.id) ? ids.filter((id) => id !== fornecedor.id) : [...ids, fornecedor.id],
    );
  }

  protected limparSelecao(): void {
    this.idsSelecionados.set([]);
  }

  protected abrirComparacao(): void {
    if (this.selecionados().length !== 2) return;
    this.comparando.set(true);
  }

  /** Fecha a comparação sem escolher ninguém; a seleção continua marcada na lista. */
  protected fecharComparacao(): void {
    if (this.escolhendo()) return;
    this.comparando.set(false);
  }

  /** "Escolher esse": abre a confirmação dos dados da despesa. */
  protected iniciarEscolha(fornecedor: Fornecedor): void {
    this.erroEscolha.set(null);
    this.escolhido.set(fornecedor);
  }

  protected cancelarEscolha(): void {
    if (this.escolhendo()) return;
    this.escolhido.set(null);
  }

  /** Contrata o escolhido, descarta o outro e cria a despesa. */
  protected confirmarEscolha(despesa: NovaDespesa): void {
    const escolhido = this.escolhido();
    const descartado = this.descartado();
    if (!escolhido || !descartado || this.escolhendo()) return;

    this.escolhendo.set(true);
    this.erroEscolha.set(null);

    this.fornecedoresService
      .contratar(escolhido.id, descartado.id, despesa)
      .pipe(finalize(() => this.escolhendo.set(false)))
      .subscribe({
        next: ({ escolhido: contratado, descartado: recusado, despesa: criada }) => {
          const atualizados = [contratado, recusado];
          this.fornecedores.update((lista) => lista.map((f) => atualizados.find((a) => a.id === f.id) ?? f));
          this.escolhido.set(null);
          this.comparando.set(false);
          this.limparSelecao();
          this.mensagemSucesso.set(`${contratado.nome} foi contratado e a despesa "${criada.descricao}" foi criada.`);
        },
        error: (erro: unknown) => this.erroEscolha.set(mensagemDeErro(erro)),
      });
  }

  protected rotuloStatus(status: StatusFornecedor): string {
    return ROTULO_STATUS[status];
  }

  protected classeStatus(status: StatusFornecedor): string {
    return CLASSE_STATUS[status];
  }

  protected whatsapp(telefone: string | null): string | null {
    return linkWhatsapp(telefone);
  }

  protected valor(evento: Event): string {
    return (evento.target as HTMLInputElement).value;
  }
}
