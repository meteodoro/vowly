import {AfterViewInit, Component, ElementRef, EventEmitter, HostListener, Input, Output, ViewChild, computed, signal,} from '@angular/core';
import { CATEGORIAS, NovaDespesa } from '../../core/models/despesa.model';
import { formatarData, gerarParcelas } from '../../core/despesas/parcelas.util';

const MAX_PARCELAS = 60;
const VALOR_MAXIMO = 999_999_999.99;

// Modal de cadastro de despesa, com pré-visualização das parcelas geradas.
@Component({
  selector: 'app-nova-despesa',
  standalone: true,
  templateUrl: './nova-despesa.modal.html',
})
export class NovaDespesaModal implements AfterViewInit {
  @Input() salvando = false;
  @Input() erro: string | null = null;
  @Output() cancelar = new EventEmitter<void>();
  @Output() salvar = new EventEmitter<NovaDespesa>();

  @ViewChild('campoDescricao') private campoDescricao?: ElementRef<HTMLInputElement>;

  protected readonly categorias = CATEGORIAS;

  protected readonly descricao = signal('');
  protected readonly categoria = signal('');
  protected readonly valorTotal = signal('');
  protected readonly parcelar = signal(true);
  protected readonly numParcelas = signal('3');
  protected readonly primeiroVencimento = signal('');
  protected readonly erroLocal = signal<string | null>(null);

  /** Parcelas que serão criadas, atualizadas enquanto a pessoa digita. */
  protected readonly parcelasGeradas = computed(() => {
    if (!this.parcelar()) return [];

    const total = Number(this.valorTotal());
    const quantidade = Number(this.numParcelas());
    const data = this.primeiroVencimento();

    if (!(total > 0) || !Number.isInteger(quantidade) || quantidade < 2 || quantidade > MAX_PARCELAS || !data) {
      return [];
    }
    return gerarParcelas(total, quantidade, data);
  });

  ngAfterViewInit(): void {
    this.campoDescricao?.nativeElement.focus();
  }

  @HostListener('document:keydown.escape')
  protected aoApertarEsc(): void {
    this.cancelar.emit();
  }

  protected enviar(evento: Event): void {
    evento.preventDefault();
    this.erroLocal.set(null);

    const valor = Number(this.valorTotal());
    const quantidade = this.parcelar() ? Number(this.numParcelas()) : 1;

    if (!this.descricao().trim()) return this.erroLocal.set('Informe a descrição.');
    if (!this.categoria()) return this.erroLocal.set('Escolha a categoria.');
    if (!(valor > 0)) return this.erroLocal.set('Informe um valor maior que zero.');
    if (valor > VALOR_MAXIMO) return this.erroLocal.set('O valor informado é maior que o limite permitido.');
    if (this.parcelar() && (!Number.isInteger(quantidade) || quantidade < 2 || quantidade > MAX_PARCELAS)) {
      return this.erroLocal.set(`Informe de 2 a ${MAX_PARCELAS} parcelas.`);
    }
    if (!this.primeiroVencimento()) return this.erroLocal.set('Informe a data de vencimento.');

    this.salvar.emit({
      descricao: this.descricao().trim(),
      categoria: this.categoria(),
      valorTotal: valor,
      parcelas: quantidade,
      primeiroVencimento: this.primeiroVencimento(),
    });
  }

  protected moeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
  }

  protected data(iso: string): string {
    return formatarData(iso);
  }

  protected valor(evento: Event): string {
    return (evento.target as HTMLInputElement | HTMLSelectElement).value;
  }
}
