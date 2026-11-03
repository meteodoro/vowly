using Vowly.Api.Enums;

namespace Vowly.Api.Dtos;

public class ParcelaPendenteDto
{
    public int ParcelaId { get; set; }
    public int DespesaId { get; set; }
    public string DespesaDescricao { get; set; } = string.Empty;
    public string CategoriaNome { get; set; } = string.Empty;
    public int Numero { get; set; }
    public int TotalParcelas { get; set; }
    public decimal Valor { get; set; }
    public DateOnly DataVencimento { get; set; }
    public SituacaoParcela Situacao { get; set; }
    public int DiasParaVencer { get; set; }
}