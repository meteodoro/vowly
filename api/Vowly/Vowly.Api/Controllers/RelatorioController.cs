using Microsoft.AspNetCore.Mvc;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/admin/relatorio")]
public class RelatorioController : AdminControllerBase
{
    [HttpGet("teste")]
    public IActionResult Teste() => Ok(new { mensagem = "Área do Admin." });
}