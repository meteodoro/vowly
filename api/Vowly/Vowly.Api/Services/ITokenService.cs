using Vowly.Api.Dtos;
using Vowly.Api.Models;

namespace Vowly.Api.Services;

public interface ITokenService
{
    public AuthRespostaDto GerarToken(Usuario usuario);
}
