using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Vowly.Api.Enums;
using Vowly.Api.Services;

namespace Vowly.Api.Controllers;

public abstract class VowlyControllerBase : ControllerBase
{
    protected string UsuarioId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    protected IActionResult Erro(TipoErro? erro, string? mensagem) => erro switch
    {
        TipoErro.NaoEncontrado => NotFound(new { mensagem }),
        TipoErro.Conflito => Conflict(new { mensagem }),
        _ => BadRequest(new { mensagem })
    };
}