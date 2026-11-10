import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Perfil } from '../models/api.model';
import { AuthService } from './auth.service';

// Só quem está logado e tem o perfil indicado
export const perfilGuard = (perfil: Perfil): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.estaAutenticado()) {
    return router.createUrlTree(['/entrar']);
  }

  // Logado com o outro perfil: manda para a página inicial dele
  return auth.perfil() === perfil ? true : router.createUrlTree([auth.paginaInicial()]);
};

// Só quem ainda não está logado (tela de login)
export const visitanteGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.estaAutenticado() ? router.createUrlTree([auth.paginaInicial()]) : true;
};
