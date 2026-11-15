using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos.Casamentos;
using Vowly.Api.Services.Casamentos;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/casamento")]
public class CasamentoController(ICasamentoService casamentoService) : CasalControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var r = await casamentoService.ObterAsync(UsuarioId);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarCasamentoRequest request)
    {
        var r = await casamentoService.CriarAsync(UsuarioId, request);
        return r.Sucesso ? CreatedAtAction(nameof(Obter), r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar(AtualizarCasamentoRequest request)
    {
        var r = await casamentoService.AtualizarAsync(UsuarioId, request);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }
}