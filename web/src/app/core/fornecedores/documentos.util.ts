/** Limite por arquivo. A API deve aplicar o mesmo limite (e conferir o conteúdo do arquivo). */
export const TAMANHO_MAXIMO_BYTES = 10 * 1024 * 1024;

const EXTENSOES_PERMITIDAS = ['pdf', 'jpg', 'jpeg', 'png', 'webp'];

/** Devolve a mensagem de erro do arquivo, ou null se ele pode ser enviado. */
export function validarArquivo(arquivo: { name: string; size: number }): string | null {
  const extensao = arquivo.name.includes('.') ? (arquivo.name.split('.').pop() ?? '').toLowerCase() : '';

  if (!EXTENSOES_PERMITIDAS.includes(extensao)) {
    return `"${arquivo.name}": envie apenas PDF ou imagem (JPG, PNG ou WebP).`;
  }
  if (arquivo.size === 0) return `"${arquivo.name}" está vazio.`;
  if (arquivo.size > TAMANHO_MAXIMO_BYTES) return `"${arquivo.name}" passa do limite de 10 MB.`;
  return null;
}

export function formatarTamanho(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1).replace('.', ',')} MB`;
}

export function ehPdf(nome: string): boolean {
  return nome.toLowerCase().endsWith('.pdf');
}
