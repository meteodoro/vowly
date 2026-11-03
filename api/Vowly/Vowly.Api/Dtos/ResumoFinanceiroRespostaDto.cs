using Vowly.Api.Enums;

namespace Vowly.Api.Dtos;

public class ResumoFinanceiroRespostaDto
{
    public decimal OrcamentoTotal { get; set; }
    public decimal Comprometido { get; set; }
    public decimal Pago { get; set; }
    public decimal APagar { get; set; }
    public decimal Saldo { get; set; }
    public decimal Percentual { get; set; }
    public NivelAlerta Alerta { get; set; }
}