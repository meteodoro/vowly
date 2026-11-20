using Microsoft.AspNetCore.Identity;
using Vowly.Api.Dtos.Auth;
using Vowly.Api.Enums;
using Vowly.Api.Models;
using Vowly.Api.Services.Comum;

namespace Vowly.Api.Services.Auth;

public interface IAuthService
{
    Task<Resultado<AuthResponse>> RegistrarAsync(RegistrarRequest request);
    Task<Resultado<AuthResponse>> LoginAsync(LoginRequest request);
}

public class AuthService(UserManager<Usuario> userManager, ITokenService tokenService) : IAuthService
{
    public async Task<Resultado<AuthResponse>> RegistrarAsync(RegistrarRequest request)
    {
        var usuario = new Usuario
        {
            UserName = request.Email,
            Email = request.Email,
            Nome = request.Nome
        };

        var resultado = await userManager.CreateAsync(usuario, request.Senha);

        if (!resultado.Succeeded)
        {
            var emailEmUso = resultado.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail");

            var mensagem = emailEmUso
                ? "Este e-mail já está cadastrado."
                : string.Join(" ", resultado.Errors.Select(e => e.Description));

            return Resultado<AuthResponse>.Falha(TipoErro.Invalido, mensagem);
        }

        var perfilResultado = await userManager.AddToRoleAsync(usuario, Perfis.Casal);
        if (!perfilResultado.Succeeded)
        {
            // Não deixa um usuário sem perfil no banco
            await userManager.DeleteAsync(usuario);

            return Resultado<AuthResponse>.Falha(TipoErro.ErroInterno,
                "Não foi possível concluir o cadastro. Tente novamente.");
        }

        return Resultado<AuthResponse>.Ok(tokenService.GerarToken(usuario, [Perfis.Casal]));
    }

    public async Task<Resultado<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var usuario = await userManager.FindByEmailAsync(request.Email);

        if (usuario is null || !await userManager.CheckPasswordAsync(usuario, request.Senha))
            return Resultado<AuthResponse>.Falha(TipoErro.NaoAutorizado, "E-mail ou senha inválidos.");

        var perfis = await userManager.GetRolesAsync(usuario);

        return Resultado<AuthResponse>.Ok(tokenService.GerarToken(usuario, perfis));
    }
}