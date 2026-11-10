import { HttpClient } from '@angular/common/http';
import { Service, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { API } from '../api/api.config';
import { AuthResposta, LoginRequest, Perfil, RegistrarRequest } from '../models/api.model';

const CHAVE_SESSAO = 'vowly.sessao';

@Service()
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _sessao = signal<AuthResposta | null>(this.lerSessao());

  // Dados da sessão (nome, perfil...), para exibir na tela
  readonly sessao = this._sessao.asReadonly();

  login(dados: LoginRequest) {
    return this.http
      .post<AuthResposta>(`${API}/auth/login`, dados)
      .pipe(tap((resposta) => this.guardar(resposta)));
  }

  registrar(dados: RegistrarRequest) {
    return this.http
      .post<AuthResposta>(`${API}/auth/registrar`, dados)
      .pipe(tap((resposta) => this.guardar(resposta)));
  }

  sair(irParaLogin = true): void {
    this.limpar();
    if (irParaLogin) {
      void this.router.navigate(['/entrar']);
    }
  }

  // Token válido, ou null se não há sessão ou ela expirou
  token(): string | null {
    const sessao = this._sessao();
    if (!sessao) {
      return null;
    }

    if (new Date(sessao.expiraEm).getTime() <= Date.now()) {
      this.limpar();
      return null;
    }

    return sessao.token;
  }

  estaAutenticado(): boolean {
    return this.token() !== null;
  }

  perfil(): Perfil | null {
    return this.estaAutenticado() ? this._sessao()!.perfil : null;
  }

  // Página inicial de cada perfil
  paginaInicial(): string {
    return this.perfil() === 'Admin' ? '/admin' : '/resumo';
  }

  private guardar(sessao: AuthResposta): void {
    this._sessao.set(sessao);
    try {
      localStorage.setItem(CHAVE_SESSAO, JSON.stringify(sessao));
    } catch {
      // Sem armazenamento disponível: a sessão vale só até recarregar a página
    }
  }

  private limpar(): void {
    this._sessao.set(null);
    try {
      localStorage.removeItem(CHAVE_SESSAO);
    } catch {
      // Ignora
    }
  }

  private lerSessao(): AuthResposta | null {
    try {
      const texto = localStorage.getItem(CHAVE_SESSAO);
      return texto ? (JSON.parse(texto) as AuthResposta) : null;
    } catch {
      return null;
    }
  }
}
