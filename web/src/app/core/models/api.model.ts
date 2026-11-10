// Espelha os DTOs da API.

export type Perfil = 'Casal' | 'Admin';

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface RegistrarRequest extends LoginRequest {
  nome: string;
}

export interface AuthResposta {
  token: string;
  expiraEm: string;
  nome: string;
  perfil: Perfil;
}

// Formato padrão dos erros da API
export interface ErroApi {
  mensagem: string;
  erros?: Record<string, string[]>;
  detalhe?: string;
}
