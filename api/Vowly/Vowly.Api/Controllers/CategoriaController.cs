using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController(ICategoriaService categoriaService) : ControllerBase
{
    private string UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var r = await categoriaService.ListarAsync(UsuarioId);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(SalvarCategoriaDto dto)
    {
        var r = await categoriaService.CriarAsync(UsuarioId, dto);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, SalvarCategoriaDto dto)
    {
        var r = await categoriaService.AtualizarAsync(UsuarioId, id, dto);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var r = await categoriaService.ExcluirAsync(UsuarioId, id);
        return r.Sucesso ? NoContent() : Erro(r.Erro, r.Mensagem);
    }

    private IActionResult Erro(TipoErro? erro, string? mensagem) => erro switch
    {
        TipoErro.NaoEncontrado => NotFound(new { mensagem }),
        TipoErro.Conflito => Conflict(new { mensagem }),
        _ => BadRequest(new { mensagem })
    };
}