using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Vowly.Api.Dtos.Parcelas;
using Vowly.Api.Services.Parcelas;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api")]
public class ParcelaController(IParcelaService parcelaService) : CasalControllerBase
{
    [HttpGet("despesas/{despesaId:int}/parcelas")]
    public async Task<IActionResult> Listar(int despesaId)
    {
        var r = await parcelaService.ListarAsync(UsuarioId, despesaId);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpGet("parcelas/pendentes")]
    public async Task<IActionResult> Pendentes([FromQuery, Range(1, 365)] int dias = 30)
    {
        var r = await parcelaService.ListarPendentesAsync(UsuarioId, dias);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPost("despesas/{despesaId:int}/parcelas")]
    public async Task<IActionResult> Gerar(int despesaId, GerarParcelasRequest request)
    {
        var r = await parcelaService.GerarAsync(UsuarioId, despesaId, request);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPost("parcelas/{id:int}/pagar")]
    public async Task<IActionResult> Pagar(
        int id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PagarParcelaRequest? request)
    {
        var r = await parcelaService.PagarAsync(UsuarioId, id, request?.DataPagamento);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }
}