import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import {Despesa, NovaDespesa, StatusDespesa } from '../models/despesa.model';


// Dados de exemplo. TODO: trocar pelas chamadas HttpClient quando a API estiver pronta
// (GET /despesas e POST /despesas). Os métodos já devolvem Observable, como o CasamentoService.
const EXEMPLOS: Despesa[] = [
  { id: 1, descricao: 'Buffet', categoria: 'Buffet', valor: 18000, vencimento: '2026-11-05', status: 'em_pagamento', parcelas: 6 },
  { id: 2, descricao: 'Decoração', categoria: 'Decoração', valor: 7500, vencimento: '2026-11-10', status: 'a_pagar', parcelas: 5 },
  { id: 3, descricao: 'Fotografia e vídeo', categoria: 'Fotografia', valor: 6000, vencimento: '2026-11-15', status: 'a_pagar', parcelas: 3 },
  { id: 4, descricao: 'Vestido da noiva', categoria: 'Vestido', valor: 4200, vencimento: '2026-11-20', status: 'paga', parcelas: 1 },
  { id: 5, descricao: 'Convites', categoria: 'Itens gerais', valor: 850, vencimento: '2026-11-22', status: 'paga', parcelas: 1 },
  { id: 6, descricao: 'Lembrancinhas', categoria: 'Itens gerais', valor: 1200, vencimento: '2026-11-28', status: 'a_pagar', parcelas: 2 },
  { id: 7, descricao: 'Música e DJ', categoria: 'Música', valor: 3500, vencimento: '2026-12-02', status: 'a_pagar', parcelas: 2 },
  { id: 8, descricao: 'Bolo e doces', categoria: 'Buffet', valor: 2400, vencimento: '2026-10-25', status: 'atrasada', parcelas: 1 },
  { id: 9, descricao: 'Terno do noivo', categoria: 'Vestido', valor: 1800, vencimento: '2026-10-18', status: 'paga', parcelas: 1 },
  { id: 10, descricao: 'Alianças', categoria: 'Itens gerais', valor: 3900, vencimento: '2026-10-10', status: 'paga', parcelas: 1 },
  { id: 11, descricao: 'Flores da cerimônia', categoria: 'Decoração', valor: 2100, vencimento: '2026-12-10', status: 'a_pagar', parcelas: 3 },
  { id: 12, descricao: 'Assessoria do casamento', categoria: 'Assessoria', valor: 5000, vencimento: '2026-12-15', status: 'em_pagamento', parcelas: 5 },
];

@Injectable({ providedIn: 'root' })
export class DespesasService {
  private readonly banco: Despesa[] = [...EXEMPLOS];
  private proximoId = 100;

  listar(): Observable<Despesa[]> {
    return of([...this.banco]);
  }

  criar(dados: NovaDespesa): Observable<Despesa> {
    const nova: Despesa = {
      id: this.proximoId++,
      descricao: dados.descricao.trim(),
      categoria: dados.categoria,
      valor: dados.valorTotal,
      vencimento: dados.primeiroVencimento,
      status: this.statusFinal(dados.parcelas === 1 ? (dados.status ?? 'a_pagar') : 'a_pagar', dados.primeiroVencimento),
      parcelas: dados.parcelas,
    };
    this.banco.push(nova);
    return of(nova);
  }

  atualizar(id: number, dados: NovaDespesa): Observable<Despesa> {
    const indice = this.banco.findIndex((d) => d.id === id);
    if (indice < 0) return throwError(() => new Error('Despesa não encontrada.'));

    // O número de parcelas não muda na edição; só os dados principais.
    const atualizada: Despesa = {
      ...this.banco[indice],
      descricao: dados.descricao.trim(),
      categoria: dados.categoria,
      valor: dados.valorTotal,
      vencimento: dados.primeiroVencimento,
      // À vista: o status vem do modal. Parcelada: segue as parcelas, então não muda aqui.
      status:
        this.banco[indice].parcelas === 1 && dados.status
          ? this.statusFinal(dados.status, dados.primeiroVencimento)
          : this.banco[indice].status,
    };
    this.banco[indice] = atualizada;
    return of(atualizada);
  }

  /** "Atrasada" é automático: não está paga e o vencimento já passou. A API deve fazer o mesmo. */
  private statusFinal(status: StatusDespesa, vencimento: string): StatusDespesa {
    const hoje = new Date();
    const iso = `${hoje.getFullYear()}-${String(hoje.getMonth() + 1).padStart(2, '0')}-${String(hoje.getDate()).padStart(2, '0')}`;
    return status === 'a_pagar' && vencimento < iso ? 'atrasada' : status;
  }
}
