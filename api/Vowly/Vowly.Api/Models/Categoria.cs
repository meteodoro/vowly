namespace Vowly.Api.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int CasamentoId { get; set; }
    public Casamento Casamento { get; set; } = null!;

    public List<Despesa> Despesas { get; set; } = new();
}