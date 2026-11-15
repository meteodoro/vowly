import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { Compromisso, DadosCompromisso } from '../models/compromisso.model';
import { hojeIso, somarDias } from '../parcelas/vencimentos.util';

// Dados de exemplo, com datas relativas a hoje. TODO: trocar pelas chamadas HttpClient
// quando a API estiver pronta (GET/POST /compromissos, PUT/DELETE /compromissos/{id}
// e PATCH /compromissos/{id}/conclusao).
const hoje = hojeIso();
const em = (dias: number): string => somarDias(hoje, dias);

const EXEMPLOS: Compromisso[] = [
  { id: 1, titulo: 'Degustação do buffet', data: em(-12), hora: '19:00', local: 'Buffet Bella Ve', concluido: true },
  { id: 2, titulo: 'Reunião com a decoradora', data: em(-6), hora: '10:30', local: 'Ateliê Flor & Cia', concluido: true },
  { id: 3, titulo: 'Prova do vestido', data: em(1), hora: '14:00', local: 'Atelier Noivas', concluido: false },
  { id: 4, titulo: 'Visita ao espaço da cerimônia', data: em(3), hora: '16:00', local: 'Igreja Matriz', concluido: false },
  { id: 5, titulo: 'Reunião com o fotógrafo', data: em(5), hora: '18:30', local: 'Videochamada', concluido: false },
  { id: 6, titulo: 'Degustação do bolo', data: em(5), hora: '15:00', local: 'Doceria Maria', concluido: false },
  { id: 7, titulo: 'Teste de maquiagem e cabelo', data: em(9), hora: '09:00', local: 'Studio Bela', concluido: false },
  { id: 8, titulo: 'Fechar a lista de convidados', data: em(16), hora: null, local: null, concluido: false },
  { id: 9, titulo: 'Ensaio da cerimônia', data: em(21), hora: '19:30', local: 'Igreja Matriz', concluido: false },
  { id: 10, titulo: 'Entrega dos convites', data: em(30), hora: null, local: null, concluido: false },
];

@Injectable({ providedIn: 'root' })
export class CompromissosService {
  private readonly banco: Compromisso[] = EXEMPLOS.map((c) => ({ ...c }));
  private proximoId = 100;

  listar(): Observable<Compromisso[]> {
    return of(this.banco.map((c) => ({ ...c })));
  }

  criar(dados: DadosCompromisso): Observable<Compromisso> {
    const novo: Compromisso = { id: this.proximoId++, ...dados, titulo: dados.titulo.trim(), concluido: false };
    this.banco.push(novo);
    return of({ ...novo });
  }

  atualizar(id: number, dados: DadosCompromisso): Observable<Compromisso> {
    const atual = this.banco.find((c) => c.id === id);
    if (!atual) return throwError(() => new Error('Compromisso não encontrado.'));

    Object.assign(atual, { ...dados, titulo: dados.titulo.trim() });
    return of({ ...atual });
  }

  excluir(id: number): Observable<void> {
    const indice = this.banco.findIndex((c) => c.id === id);
    if (indice < 0) return throwError(() => new Error('Compromisso não encontrado.'));

    this.banco.splice(indice, 1);
    return of(undefined);
  }

  definirConclusao(id: number, concluido: boolean): Observable<Compromisso> {
    const atual = this.banco.find((c) => c.id === id);
    if (!atual) return throwError(() => new Error('Compromisso não encontrado.'));

    atual.concluido = concluido;
    return of({ ...atual });
  }
}
