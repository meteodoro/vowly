using Microsoft.AspNetCore.Identity;

namespace Vowly.Api.Models;

public class Usuario : IdentityUser
{
    public string Nome { get; set; } = string.Empty;
    public Casamento? Casamento { get; set; }
    
    
}