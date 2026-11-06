using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/admin/relatorio")]
public class RelatorioController(IRelatorioEmpresaService relatorioService) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obter([FromQuery, Range(1, 24)] int meses = 6) =>
        Ok(await relatorioService.ObterAsync(meses));
}