using Vowly.Api.Enums;

namespace Vowly.Api.Models;

public class Assinatura
{
    public int Id { get; set; }
    public TipoPlano Plano { get; set; }
    public StatusAssinatura Status { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public decimal ValorMensal { get; set; }
    
    public string UsuarioId { get; set; } = string.Empty;
    public Usuario Usuario { get; set; } = null!;
}