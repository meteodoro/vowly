using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos;
using Vowly.Api.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api")]
public class ParcelasController(IParcelaService parcelaService) : CasalControllerBase
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
    public async Task<IActionResult> Gerar(int despesaId, GerarParcelasDto dto)
    {
        var r = await parcelaService.GerarAsync(UsuarioId, despesaId, dto);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r.Erro, r.Mensagem);
    }
    
    [HttpPost("parcelas/{id:int}/pagar")]
    public async Task<IActionResult> Pagar(
        int id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PagarParcelaDto? dto)
    {
        var r = await parcelaService.PagarAsync(UsuarioId, id, dto?.DataPagamento);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }
    
    
}