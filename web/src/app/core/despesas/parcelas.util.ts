export interface ParcelaGerada {
  numero: number;
  valor: number;
  /** Vencimento no formato ISO (aaaa-mm-dd). */
  vencimento: string;
}

/**
 * Divide o valor total em parcelas mensais.
 * Trabalha em centavos para não perder nada: a diferença da divisão vai para a 1ª parcela.
 */
export function gerarParcelas(
  valorTotal: number,
  quantidade: number,
  primeiroVencimento: string,
): ParcelaGerada[] {
  const [ano, mes, dia] = primeiroVencimento.split('-').map(Number);
  const totalCentavos = Math.round(valorTotal * 100);
  const base = Math.floor(totalCentavos / quantidade);
  const resto = totalCentavos - base * quantidade;

  return Array.from({ length: quantidade }, (_, i) => ({
    numero: i + 1,
    valor: (i === 0 ? base + resto : base) / 100,
    vencimento: somarMeses(ano, mes, dia, i),
  }));
}

/** Soma meses mantendo o dia; em meses mais curtos usa o último dia (31/01 + 1 mês = 28/02). */
export function somarMeses(ano: number, mes: number, dia: number, quantidade: number): string {
  const meses = mes - 1 + quantidade;
  const anoFinal = ano + Math.floor(meses / 12);
  const mesFinal = ((meses % 12) + 12) % 12; // 0 a 11
  const ultimoDia = new Date(anoFinal, mesFinal + 1, 0).getDate();
  const diaFinal = Math.min(dia, ultimoDia);

  return `${anoFinal}-${String(mesFinal + 1).padStart(2, '0')}-${String(diaFinal).padStart(2, '0')}`;
}

/** Converte aaaa-mm-dd em dd/mm/aaaa sem passar por Date (evita problemas de fuso). */
export function formatarData(iso: string): string {
  const [ano, mes, dia] = iso.split('-');
  return `${dia}/${mes}/${ano}`;
}
