using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/compromissos")]
[Authorize]
public class CompromissosController(ICompromissoService compromissoService) : VowlyControllerBase
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

    [HttpPost]
    public async Task<IActionResult> Criar(SalvarCompromissoDto dto)
    {
        var r = await compromissoService.CriarAsync(UsuarioId, dto);
        return r.Sucesso
            ? CreatedAtAction(nameof(Obter), new { id = r.Valor!.Id }, r.Valor)
            : Erro(r.Erro, r.Mensagem);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, SalvarCompromissoDto dto)
    {
        var r = await compromissoService.AtualizarAsync(UsuarioId, id, dto);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var r = await compromissoService.ExcluirAsync(UsuarioId, id);
        return r.Sucesso ? NoContent() : Erro(r.Erro, r.Mensagem);
    }
}