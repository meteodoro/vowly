namespace Vowly.Api.Models;

public class Cobranca
{
    public int Id { get; set; }
    public DateTime DataCobranca { get; set; }
    public decimal Valor { get; set; }

    public string UsuarioId { get; set; } = string.Empty;
    public Usuario Usuario { get; set; } = null!;
}