using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/assinatura")]
[Authorize]
public class AssinaturaController(IAssinaturaService assinaturaService) : VowlyControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obter() =>
        Ok(await assinaturaService.ObterAsync(UsuarioId));

    [HttpPost("assinar")]
    public async Task<IActionResult> Assinar()
    {
        var r = await assinaturaService.AssinarAsync(UsuarioId);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPost("cancelar")]
    public async Task<IActionResult> Cancelar()
    {
        var r = await assinaturaService.CancelarAsync(UsuarioId);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }
}