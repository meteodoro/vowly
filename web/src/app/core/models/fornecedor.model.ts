export type StatusFornecedor = 'cotando' | 'contratado' | 'descartado';

export const STATUS_FORNECEDOR: { valor: StatusFornecedor; rotulo: string }[] = [
  { valor: 'cotando', rotulo: 'Cotando' },
  { valor: 'contratado', rotulo: 'Contratado' },
  { valor: 'descartado', rotulo: 'Descartado' },
];

export interface Fornecedor {
  id: number;
  nome: string;
  categoria: string;
  status: StatusFornecedor;
  /** Nome da pessoa de contato. */
  contato: string | null;
  telefone: string | null;
  email: string | null;
  observacoes: string | null;
  /** Valor total do orçamento, em reais. */
  valorOrcado: number | null;
  /** Condições de pagamento em texto livre (ex.: "30% de entrada + 5x"). */
  condicoesPagamento: string | null;
  pros: string[];
  contras: string[];
}

/** Campos enviados ao criar ou editar um fornecedor. */
export type DadosFornecedor = Omit<Fornecedor, 'id'>;
