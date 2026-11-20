using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos.Auth;
using Vowly.Api.Services.Auth;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : VowlyControllerBase
{
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(RegistrarRequest request)
    {
        var r = await authService.RegistrarAsync(request);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var r = await authService.LoginAsync(request);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }
}