using Vowly.Api.Enums;
using Vowly.Api.Repositories.Despesas;
using Vowly.Api.Services.Dashboard;

namespace Vowly.Api.Tests;

public class ResumoFinanceiroCalculadoraTests
{
    private static DespesaResumo AVista(decimal valor, bool paga) =>
        new(valor, paga, TemParcelas: false, ValorParcelasPagas: 0m);

    private static DespesaResumo Parcelada(decimal valor, decimal pagoEmParcelas, bool quitada = false) =>
        new(valor, quitada, TemParcelas: true, ValorParcelasPagas: pagoEmParcelas);

    [Fact]
    public void SemDespesas_TudoZeradoESaldoIgualAoOrcamento()
    {
        var r = ResumoFinanceiroCalculadora.Calcular(10000m, []);

        Assert.Equal(0m, r.Comprometido);
        Assert.Equal(0m, r.Pago);
        Assert.Equal(0m, r.APagar);
        Assert.Equal(10000m, r.Saldo);
        Assert.Equal(0m, r.Percentual);
        Assert.Equal(NivelAlerta.Normal, r.Alerta);
    }

    [Fact]
    public void DespesaAVistaPaga_ContaComoPagaPeloValorTotal()
    {
        var r = ResumoFinanceiroCalculadora.Calcular(10000m, [AVista(2000m, paga: true)]);

        Assert.Equal(2000m, r.Comprometido);
        Assert.Equal(2000m, r.Pago);
        Assert.Equal(0m, r.APagar);
    }

    [Fact]
    public void DespesaAVistaAPagar_ComprometeMasNaoContaComoPaga()
    {
        var r = ResumoFinanceiroCalculadora.Calcular(10000m, [AVista(2000m, paga: false)]);

        Assert.Equal(2000m, r.Comprometido);
        Assert.Equal(0m, r.Pago);
        Assert.Equal(2000m, r.APagar);
    }

    [Fact]
    public void SoParcelas_PagoEhASomaDasParcelasPagas()
    {
        // R$ 12.000 em 3x, com uma parcela de R$ 4.000 paga
        var r = ResumoFinanceiroCalculadora.Calcular(60000m, [Parcelada(12000m, pagoEmParcelas: 4000m)]);

        Assert.Equal(12000m, r.Comprometido);
        Assert.Equal(4000m, r.Pago);
        Assert.Equal(8000m, r.APagar);
    }

    [Fact]
    public void DespesaParceladaQuitada_NaoContaEmDobro()
    {
        var r = ResumoFinanceiroCalculadora.Calcular(
            60000m, [Parcelada(12000m, pagoEmParcelas: 12000m, quitada: true)]);

        Assert.Equal(12000m, r.Comprometido);
        Assert.Equal(12000m, r.Pago);
        Assert.Equal(0m, r.APagar);
    }

    [Fact]
    public void MisturaDeDespesas_SegueAsFormulasDaHU05()
    {
        var despesas = new[]
        {
            AVista(8000m, paga: true),
            AVista(2000m, paga: false),
            Parcelada(12000m, pagoEmParcelas: 4000m)
        };

        var r = ResumoFinanceiroCalculadora.Calcular(60000m, despesas);

        Assert.Equal(22000m, r.Comprometido);
        Assert.Equal(12000m, r.Pago);
        Assert.Equal(10000m, r.APagar);
        Assert.Equal(38000m, r.Saldo);
        Assert.Equal(36.7m, r.Percentual);
        Assert.Equal(NivelAlerta.Normal, r.Alerta);
    }

    [Theory]
    [InlineData(1000, 33.3)]
    [InlineData(2000, 66.7)]
    public void Percentual_TemUmaCasaDecimal(int comprometido, double esperado)
    {
        var r = ResumoFinanceiroCalculadora.Calcular(3000m, [AVista(comprometido, paga: false)]);

        Assert.Equal((decimal)esperado, r.Percentual);
    }

    [Theory]
    [InlineData(89900, NivelAlerta.Normal)]    // 89,9%
    [InlineData(90000, NivelAlerta.Atencao)]   // 90,0%
    [InlineData(100000, NivelAlerta.Atencao)]  // 100,0%: ainda não passou do orçamento
    [InlineData(100001, NivelAlerta.Estourado)] // acima de 100%
    public void Alerta_RespeitaOsLimitesDe90ENoEstouro(int comprometidoEmCentavos, NivelAlerta esperado)
    {
        // Orçamento de R$ 1.000,00 = 100000 centavos
        var comprometido = comprometidoEmCentavos / 100m;

        var r = ResumoFinanceiroCalculadora.Calcular(1000m, [AVista(comprometido, paga: false)]);

        Assert.Equal(esperado, r.Alerta);
    }

    [Fact]
    public void OrcamentoEstourado_SaldoFicaNegativo()
    {
        var r = ResumoFinanceiroCalculadora.Calcular(10000m, [AVista(12000m, paga: false)]);

        Assert.Equal(-2000m, r.Saldo);
        Assert.Equal(120.0m, r.Percentual);
        Assert.Equal(NivelAlerta.Estourado, r.Alerta);
    }

    [Fact]
    public void OrcamentoZerado_SemDespesas_NaoDividePorZero()
    {
        var r = ResumoFinanceiroCalculadora.Calcular(0m, []);

        Assert.Equal(0m, r.Percentual);
        Assert.Equal(NivelAlerta.Normal, r.Alerta);
    }

    [Fact]
    public void OrcamentoZerado_ComDespesa_EhEstourado()
    {
        var r = ResumoFinanceiroCalculadora.Calcular(0m, [AVista(100m, paga: false)]);

        Assert.Equal(0m, r.Percentual);
        Assert.Equal(-100m, r.Saldo);
        Assert.Equal(NivelAlerta.Estourado, r.Alerta);
    }
}