using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/assinatura")]
public class AssinaturaController(IAssinaturaService assinaturaService) : CasalControllerBase
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