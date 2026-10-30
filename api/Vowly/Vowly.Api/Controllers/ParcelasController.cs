using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ParcelasController(IParcelaService parcelaService) : VowlyControllerBase
{
    [HttpGet("despesas/{despesaId:int}/parcelas")]
    public async Task<IActionResult> Listar(int despesaId)
    {
        var r = await parcelaService.ListarAsync(UsuarioId, despesaId);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPost("despesas/{despesaId:int}/parcelas")]
    public async Task<IActionResult> Gerar(int despesaId, GerarParcelasDto dto)
    {
        var r = await parcelaService.GerarAsync(UsuarioId, despesaId, dto);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r.Erro, r.Mensagem);
    }
}