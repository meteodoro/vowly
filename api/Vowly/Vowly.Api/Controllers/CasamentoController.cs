using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/casamento")]
[Authorize]
public class CasamentoController(ICasamentoService casamentoService) : VowlyControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var casamento = await casamentoService.ObterAsync(UsuarioId);

        if (casamento is null)
            return NotFound(new { mensagem = "Você ainda não cadastrou um casamento." });

        return Ok(casamento);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(SalvarCasamentoDto dto)
    {
        var casamento = await casamentoService.CriarAsync(UsuarioId, dto);

        if (casamento is null)
            return Conflict(new { mensagem = "Você já tem um casamento cadastrado. Use a edição." });

        return CreatedAtAction(nameof(Obter), casamento);
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar(SalvarCasamentoDto dto)
    {
        var casamento = await casamentoService.AtualizarAsync(UsuarioId, dto);

        if (casamento is null)
            return NotFound(new { mensagem = "Você ainda não cadastrou um casamento." });

        return Ok(casamento);
    }
}