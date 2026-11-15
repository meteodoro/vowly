import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnInit,
  Output,
  ViewChild,
  signal,
} from '@angular/core';
import { Compromisso, DadosCompromisso } from '../../core/models/compromisso.model';

// Modal para criar, editar e excluir um compromisso.
@Component({
  selector: 'app-compromisso-modal',
  standalone: true,
  templateUrl: './compromisso.modal.html',
})
export class CompromissoModal implements OnInit, AfterViewInit {
  /** Quando informado, o modal edita esse compromisso em vez de criar um novo. */
  @Input() compromisso: Compromisso | null = null;
  /** Data (aaaa-mm-dd) já preenchida ao criar um compromisso novo. */
  @Input() dataInicial = '';
  @Input() salvando = false;
  @Input() erro: string | null = null;
  @Output() cancelar = new EventEmitter<void>();
  @Output() salvar = new EventEmitter<DadosCompromisso>();
  @Output() excluir = new EventEmitter<void>();

  @ViewChild('campoTitulo') private campoTitulo?: ElementRef<HTMLInputElement>;

  protected readonly titulo = signal('');
  protected readonly data = signal('');
  protected readonly hora = signal('');
  protected readonly local = signal('');
  protected readonly erroLocal = signal<string | null>(null);
  protected readonly confirmandoExclusao = signal(false);

  protected get editando(): boolean {
    return this.compromisso !== null;
  }

  ngOnInit(): void {
    const compromisso = this.compromisso;

    if (compromisso) {
      this.titulo.set(compromisso.titulo);
      this.data.set(compromisso.data);
      this.hora.set(compromisso.hora ?? '');
      this.local.set(compromisso.local ?? '');
    } else {
      this.data.set(this.dataInicial);
    }
  }

  ngAfterViewInit(): void {
    this.campoTitulo?.nativeElement.focus();
  }

  @HostListener('document:keydown.escape')
  protected aoApertarEsc(): void {
    this.cancelar.emit();
  }

  protected enviar(evento: Event): void {
    evento.preventDefault();
    this.erroLocal.set(null);

    if (!this.titulo().trim()) return this.erroLocal.set('Informe o título do compromisso.');
    if (!this.data()) return this.erroLocal.set('Informe a data do compromisso.');

    this.salvar.emit({
      titulo: this.titulo().trim(),
      data: this.data(),
      hora: this.hora() || null,
      local: this.local().trim() || null,
    });
  }

  /** A exclusão pede um segundo clique para evitar apagar sem querer. */
  protected pedirExclusao(): void {
    if (!this.confirmandoExclusao()) {
      this.confirmandoExclusao.set(true);
      return;
    }
    this.excluir.emit();
  }

  protected valor(evento: Event): string {
    return (evento.target as HTMLInputElement).value;
  }
}
