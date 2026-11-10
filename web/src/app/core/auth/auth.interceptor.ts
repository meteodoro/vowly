import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { API } from '../api/api.config';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (requisicao, proximo) => {
  const auth = inject(AuthService);

  // Só chamadas à nossa API levam o token
  const token = requisicao.url.startsWith(API) ? auth.token() : null;

  const comToken = token
    ? requisicao.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : requisicao;

  return proximo(comToken).pipe(
    catchError((erro: unknown) => {
      const ehLoginOuRegistro = requisicao.url.startsWith(`${API}/auth/`);

      // Token vencido ou inválido: encerra a sessão e volta para o login
      if (erro instanceof HttpErrorResponse && erro.status === 401 && !ehLoginOuRegistro) {
        auth.sair();
      }

      return throwError(() => erro);
    }),
  );
};
