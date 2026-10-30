namespace Vowly.Api.Dtos;

public class ParcelaRespostaDto
{
    public int Id { get; set; }
    public int DespesaId { get; set; }
    public int Numero { get; set; }
    public decimal Valor { get; set; }
    public DateOnly DataVencimento { get; set; }
    public DateOnly? DataPagamento { get; set; }
    public bool Paga { get; set; }
}