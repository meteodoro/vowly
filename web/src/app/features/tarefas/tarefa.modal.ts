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
import { DadosTarefa, Tarefa } from '../../core/models/tarefa.model';

// Modal para criar, editar e excluir uma tarefa.
@Component({
  selector: 'app-tarefa-modal',
  standalone: true,
  templateUrl: './tarefa.modal.html',
})
export class TarefaModal implements OnInit, AfterViewInit {
  /** Quando informada, o modal edita essa tarefa em vez de criar uma nova. */
  @Input() tarefa: Tarefa | null = null;
  @Input() salvando = false;
  @Input() erro: string | null = null;
  @Output() cancelar = new EventEmitter<void>();
  @Output() salvar = new EventEmitter<DadosTarefa>();
  @Output() excluir = new EventEmitter<void>();

  @ViewChild('campoTitulo') private campoTitulo?: ElementRef<HTMLInputElement>;

  protected readonly titulo = signal('');
  protected readonly prazo = signal('');
  protected readonly responsavel = signal('');
  protected readonly erroLocal = signal<string | null>(null);
  protected readonly confirmandoExclusao = signal(false);

  protected get editando(): boolean {
    return this.tarefa !== null;
  }

  ngOnInit(): void {
    const tarefa = this.tarefa;
    if (!tarefa) return;

    this.titulo.set(tarefa.titulo);
    this.prazo.set(tarefa.prazo ?? '');
    this.responsavel.set(tarefa.responsavel ?? '');
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

    if (!this.titulo().trim()) return this.erroLocal.set('Informe o nome da tarefa.');

    this.salvar.emit({
      titulo: this.titulo().trim(),
      prazo: this.prazo() || null,
      responsavel: this.responsavel().trim() || null,
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
