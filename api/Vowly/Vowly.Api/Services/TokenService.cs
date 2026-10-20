using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Vowly.Api.Dtos;
using Vowly.Api.Models;

namespace Vowly.Api.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthRespostaDto GerarToken(Usuario usuario)
    {
        var chave = _configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("Jwt:Key não configurada. Use dotnet user-secrets.");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var minutos = _configuration.GetValue<int>("Jwt:ExpiraEmMinutos");
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id),
            new(ClaimTypes.Email, usuario.Email ?? string.Empty),
            new(ClaimTypes.Name, usuario.Nome)
        };
        
        var chaveSecreta = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave));
        var credenciais = new SigningCredentials(chaveSecreta, SecurityAlgorithms.HmacSha256);
        
        var expiracao = DateTime.UtcNow.AddMinutes(minutos);
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: credenciais);

        var tokenTexto = new JwtSecurityTokenHandler().WriteToken(token);
        
        return new AuthRespostaDto
        {
            Token = tokenTexto,
            ExpiraEm = expiracao,
            Nome = usuario.Nome
        };
    }
    
}