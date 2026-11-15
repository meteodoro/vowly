import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import {Despesa, NovaDespesa} from '../models/despesa.model';


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
      status: 'a_pagar',
      parcelas: dados.parcelas,
    };
    this.banco.push(nova);
    return of(nova);
  }
}
