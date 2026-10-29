using Vowly.Api.Enums;

namespace Vowly.Api.Models;

public class Despesa
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public StatusDespesa Status { get; set; }
    public string? Observacoes { get; set; }
    
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
    
    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }
    
    public int CasamentoId { get; set; }
    public Casamento Casamento { get; set; } = null!;

    public List<Parcela> Parcelas { get; set; } = new();

}