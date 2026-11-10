import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { mensagemDeErro } from '../../core/api/erros.util';
import { AuthService } from '../../core/auth/auth.service';

// Versão mínima, para provar o fluxo de autenticação.
// A tela completa (layout dividido, cadastro) vem na tarefa 20.
@Component({
  selector: 'app-entrar',
  templateUrl: './entrar.page.html',
})
export class EntrarPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly email = signal('');
  protected readonly senha = signal('');
  protected readonly erro = signal<string | null>(null);
  protected readonly carregando = signal(false);

  protected entrar(evento: Event): void {
    evento.preventDefault();
    this.erro.set(null);
    this.carregando.set(true);

    this.auth.login({ email: this.email(), senha: this.senha() }).subscribe({
      next: () => void this.router.navigateByUrl(this.auth.paginaInicial()),
      error: (erro: unknown) => {
        this.erro.set(mensagemDeErro(erro));
        this.carregando.set(false);
      },
    });
  }

  protected valor(evento: Event): string {
    return (evento.target as HTMLInputElement).value;
  }
}
