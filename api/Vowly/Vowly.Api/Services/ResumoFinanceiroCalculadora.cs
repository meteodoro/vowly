using Vowly.Api.Enums;

namespace Vowly.Api.Services;

public record DespesaResumo(decimal ValorTotal, bool Paga, bool TemParcelas, decimal ValorParcelasPagas);

public record ResumoFinanceiro(
    decimal OrcamentoTotal,
    decimal Comprometido,
    decimal Pago,
    decimal APagar,
    decimal Saldo,
    decimal Percentual,
    NivelAlerta Alerta);

public class ResumoFinanceiroCalculadora
{
    public const decimal PercentualAlerta = 90m;

    public static ResumoFinanceiro Calcular(decimal orcamentoTotal, IEnumerable<DespesaResumo> despesas)
    {
        var lista = despesas.ToList();
        var comprometido = lista.Sum(d => d.ValorTotal);

        var pago = lista.Sum(d => d.TemParcelas 
            ? d.ValorParcelasPagas 
            : d.Paga 
                ? d.ValorTotal 
                : 0m);
        
        var aPagar = comprometido - pago;
        var saldo = orcamentoTotal - comprometido;
        decimal percentual;
        NivelAlerta alerta;

        if (orcamentoTotal <= 0)
        {
            percentual = 0m;
            alerta = comprometido > 0
                ? NivelAlerta.Estourado
                : NivelAlerta.Normal;
        }
        else
        {
            percentual = Math.Round(comprometido / orcamentoTotal * 100m, 1, MidpointRounding.AwayFromZero);
            alerta = comprometido > orcamentoTotal
                ? NivelAlerta.Estourado
                : percentual >= PercentualAlerta ? NivelAlerta.Atencao : NivelAlerta.Normal;
        }
        
        return new ResumoFinanceiro(orcamentoTotal, comprometido, pago, aPagar, saldo, percentual, alerta);
    }

}