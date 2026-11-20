export type TipoDocumento = 'contrato' | 'orcamento' | 'outro';

export const TIPOS_DOCUMENTO: { valor: TipoDocumento; rotulo: string }[] = [
  { valor: 'contrato', rotulo: 'Contrato' },
  { valor: 'orcamento', rotulo: 'Orçamento' },
  { valor: 'outro', rotulo: 'Outro' },
];

export interface DocumentoFornecedor {
  id: number;
  fornecedorId: number;
  /** Nome original do arquivo, como o usuário enviou. */
  nome: string;
  tipo: TipoDocumento;
  /** Tamanho em bytes. */
  tamanho: number;
  /** Data e hora do envio (ISO 8601). */
  criadoEm: string;
}

/** Eventos de um envio em andamento: o progresso e, no fim, o documento já salvo. */
export type EventoUpload =
  | { tipo: 'progresso'; percentual: number }
  | { tipo: 'concluido'; documento: DocumentoFornecedor };
