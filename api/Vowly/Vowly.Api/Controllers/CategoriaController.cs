using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Dtos.Categorias;
using Vowly.Api.Services.Categorias;

namespace Vowly.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriaController(ICategoriaService categoriaService) : CasalControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var r = await categoriaService.ListarAsync(UsuarioId);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarCategoriaRequest request)
    {
        var r = await categoriaService.CriarAsync(UsuarioId, request);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarCategoriaRequest request)
    {
        var r = await categoriaService.AtualizarAsync(UsuarioId, id, request);
        return r.Sucesso ? Ok(r.Valor) : Erro(r.Erro, r.Mensagem);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var r = await categoriaService.ExcluirAsync(UsuarioId, id);
        return r.Sucesso ? NoContent() : Erro(r.Erro, r.Mensagem);
    }
}