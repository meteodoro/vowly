using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;
using Vowly.Api.Models;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<Usuario> userManager, ITokenService tokenService) : ControllerBase
{
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(RegistrarDto dto)
    {
        var usuario = new Usuario
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nome = dto.Nome
        };

        var resultado = await userManager.CreateAsync(usuario, dto.Senha);

        if (!resultado.Succeeded)
        {
            var emailEmUso = resultado.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail");

            var mensagem = emailEmUso
                ? "Este e-mail já está cadastrado."
                : string.Join(" ", resultado.Errors.Select(e => e.Description));

            return BadRequest(new ErroRespostaDto { Mensagem = mensagem });
        }

        var perfilResultado = await userManager.AddToRoleAsync(usuario, Perfis.Casal);
        if (!perfilResultado.Succeeded)
        {
            // Não deixa um usuário sem perfil no banco
            await userManager.DeleteAsync(usuario);
            return StatusCode(500, new ErroRespostaDto
            {
                Mensagem = "Não foi possível concluir o cadastro. Tente novamente."
            });
        }

        return Ok(tokenService.GerarToken(usuario, [Perfis.Casal]));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var usuario = await userManager.FindByEmailAsync(dto.Email);

        if (usuario is null || !await userManager.CheckPasswordAsync(usuario, dto.Senha))
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });

        var perfis = await userManager.GetRolesAsync(usuario);
        return Ok(tokenService.GerarToken(usuario, perfis));
        
    }
}