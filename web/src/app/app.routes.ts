import { Routes } from '@angular/router';
import { perfilGuard, visitanteGuard } from './core/auth/auth.guard';

const placeholder = () =>
  import('./features/placeholder/placeholder.page').then((m) => m.PlaceholderPage);

export const routes: Routes = [
  {
    path: 'entrar',
    canActivate: [visitanteGuard],
    loadComponent: () => import('./features/auth/entrar.page').then((m) => m.EntrarPage),
  },

  // Telas do casal
  {
    path: '',
    canActivate: [perfilGuard('Casal')],
    loadComponent: () => import('./features/shell/shell.layout').then((m) => m.ShellLayout),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'resumo' },
      { path: 'resumo', data: { titulo: 'Resumo' }, loadComponent: placeholder },
      { path: 'despesas', data: { titulo: 'Despesas' }, loadComponent: placeholder },
      { path: 'parcelas', data: { titulo: 'Parcelas e vencimentos' }, loadComponent: placeholder },
      { path: 'agenda', data: { titulo: 'Agenda' }, loadComponent: placeholder },
      { path: 'plano', data: { titulo: 'Plano e assinatura' }, loadComponent: placeholder },
    ],
  },

  // Painel da empresa
  {
    path: 'admin',
    canActivate: [perfilGuard('Admin')],
    data: { titulo: 'Painel Admin' },
    loadComponent: placeholder,
  },

  { path: '**', redirectTo: '' },
];
