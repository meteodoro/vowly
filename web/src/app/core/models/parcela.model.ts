export interface Parcela {
  id: number;
  despesaId: number;
  /** Nome da despesa a que a parcela pertence (ex.: "Buffet"). */
  descricao: string;
  categoria: string;
  numero: number;
  totalParcelas: number;
  valor: number;
  /** Vencimento no formato ISO (aaaa-mm-dd). */
  vencimento: string;
  paga: boolean;
}
