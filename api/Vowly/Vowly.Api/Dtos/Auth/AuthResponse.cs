namespace Vowly.Api.Dtos.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
}