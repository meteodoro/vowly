import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

// Estrutura das telas do casal: menu lateral e área de conteúdo
@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.layout.html',
})
export class ShellLayout {
  protected readonly auth = inject(AuthService);

  protected readonly itens = [
    { rota: '/resumo', rotulo: 'Resumo' },
    { rota: '/despesas', rotulo: 'Despesas' },
    { rota: '/parcelas', rotulo: 'Parcelas' },
    { rota: '/agenda', rotulo: 'Agenda' },
    { rota: '/plano', rotulo: 'Plano' },
  ];
}
