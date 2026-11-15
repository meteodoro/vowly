using Vowly.Api.Enums;

namespace Vowly.Api.Dtos.Despesas;

public class DespesaResponse
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public string? Observacoes { get; set; }
    public StatusDespesa Status { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public int? FornecedorId { get; set; }
    public int QuantidadeParcelas { get; set; }
}