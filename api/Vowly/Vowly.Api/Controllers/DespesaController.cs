using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos.Despesas;
using Vowly.Api.Enums;
using Vowly.Api.Services.Despesas;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/despesas")]
public class DespesaController(IDespesaService despesaService) : CasalControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int? categoriaId, [FromQuery] StatusDespesa? status)
    {
        var r = await despesaService.ListarAsync(UsuarioId, categoriaId, status);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var r = await despesaService.ObterAsync(UsuarioId, id);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarDespesaRequest request)
    {
        var r = await despesaService.CriarAsync(UsuarioId, request);
        return r.Sucesso
            ? CreatedAtAction(nameof(Obter), new { id = r.Valor!.Id }, r.Valor)
            : Erro(r.Erro, r.Mensagem);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarDespesaRequest request)
    {
        var r = await despesaService.AtualizarAsync(UsuarioId, id, request);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var r = await despesaService.ExcluirAsync(UsuarioId, id);
        return r.Sucesso ? NoContent() : Erro(r.Erro, r.Mensagem);
    }
}