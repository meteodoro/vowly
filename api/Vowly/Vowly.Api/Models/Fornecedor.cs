namespace Vowly.Api.Models;

public class Fornecedor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Servico { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Observacoes { get; set; }
    
    public int CasamentoId { get; set; }
    public Casamento Casamento { get; set; } = null!;
    
    public List<Despesa> Despesas { get; set; } = new();
}