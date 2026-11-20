export interface Tarefa {
  id: number;
  titulo: string;
  /** Prazo no formato ISO (aaaa-mm-dd), ou null quando não há prazo. */
  prazo: string | null;
  /** Quem cuida da tarefa (ex.: "Maria", "Pedro", "Mãe da noiva"). */
  responsavel: string | null;
  concluida: boolean;
}

/** Campos enviados ao criar ou editar uma tarefa. */
export type DadosTarefa = Omit<Tarefa, 'id' | 'concluida'>;
