using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IResumoFinanceiroService resumoService) : VowlyControllerBase
{
    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo()
    {
        var r = await resumoService.ObterAsync(UsuarioId);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }
}