using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos.Compromissos;
using Vowly.Api.Enums;
using Vowly.Api.Services.Compromissos;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/compromissos")]
public class CompromissoController(ICompromissoService compromissoService) : CasalControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] StatusCompromisso? status)
    {
        var r = await compromissoService.ListarAsync(UsuarioId, status);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id)
    {
        var r = await compromissoService.ObterAsync(UsuarioId, id);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpGet("lembretes")]
    public async Task<IActionResult> Lembretes()
    {
        var r = await compromissoService.ListarLembretesAsync(UsuarioId);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarCompromissoRequest request)
    {
        var r = await compromissoService.CriarAsync(UsuarioId, request);
        return r.Sucesso
            ? CreatedAtAction(nameof(Obter), new { id = r.Valor!.Id }, r.Valor)
            : Erro(r.Erro, r.Mensagem);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarCompromissoRequest request)
    {
        var r = await compromissoService.AtualizarAsync(UsuarioId, id, request);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var r = await compromissoService.ExcluirAsync(UsuarioId, id);
        return r.Sucesso ? NoContent() : Erro(r.Erro, r.Mensagem);
    }
}