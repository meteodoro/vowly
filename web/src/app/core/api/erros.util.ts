import { HttpErrorResponse } from '@angular/common/http';
import { ErroApi } from '../models/api.model';

function corpoDoErro(erro: unknown): Partial<ErroApi> | null {
  if (erro instanceof HttpErrorResponse && erro.error && typeof erro.error === 'object') {
    return erro.error as Partial<ErroApi>;
  }
  return null;
}

// Mensagem pronta para mostrar ao usuário
export function mensagemDeErro(erro: unknown): string {
  const corpo = corpoDoErro(erro);
  if (corpo?.mensagem) {
    return corpo.mensagem;
  }

  if (erro instanceof HttpErrorResponse && (erro.status === 0 || erro.status >= 502)) {
    return 'Não foi possível conectar ao servidor. Tente novamente em instantes.';
  }

  return 'Ocorreu um erro inesperado. Tente novamente.';
}

// Erros de validação por campo (status 400), com as chaves em camelCase
export function errosPorCampo(erro: unknown): Record<string, string[]> {
  return corpoDoErro(erro)?.erros ?? {};
}
