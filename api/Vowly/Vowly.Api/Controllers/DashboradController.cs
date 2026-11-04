using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController(IResumoFinanceiroService resumoService) : CasalControllerBase
{
    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo()
    {
        var r = await resumoService.ObterAsync(UsuarioId);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }
}