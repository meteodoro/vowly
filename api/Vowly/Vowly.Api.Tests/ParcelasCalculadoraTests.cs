using Vowly.Api.Services;

namespace Vowly.Api.Tests;

public class ParcelasCalculadoraTests
{
    private static readonly DateOnly PrimeiroVencimento = new(2026, 12, 10);

    [Theory]
    [InlineData(100000, 3)] // R$ 1.000,00 em 3x
    [InlineData(10000, 3)]  // R$ 100,00 em 3x
    [InlineData(10001, 7)]  // R$ 100,01 em 7x
    [InlineData(1999, 6)]   // R$ 19,99 em 6x
    [InlineData(99999, 12)] // R$ 999,99 em 12x
    [InlineData(50000, 10)] // R$ 500,00 em 10x
    [InlineData(1, 1)]      // R$ 0,01 em 1x
    public void Gerar_SomaDasParcelas_EhIgualAoValorTotal(int valorEmCentavos, int quantidade)
    {
        var valorTotal = valorEmCentavos / 100m;

        var parcelas = ParcelasCalculadora.Gerar(valorTotal, quantidade, PrimeiroVencimento);

        Assert.Equal(quantidade, parcelas.Count);
        Assert.Equal(valorTotal, parcelas.Sum(p => p.Valor));
    }

    [Fact]
    public void Gerar_DivisaoExata_TodasAsParcelasSaoIguais()
    {
        var parcelas = ParcelasCalculadora.Gerar(900m, 3, PrimeiroVencimento);

        Assert.All(parcelas, p => Assert.Equal(300m, p.Valor));
    }

    [Fact]
    public void Gerar_ComCentavosSobrando_DiferencaVaiNaUltimaParcela()
    {
        var parcelas = ParcelasCalculadora.Gerar(1000m, 3, PrimeiroVencimento);

        Assert.Equal(333.33m, parcelas[0].Valor);
        Assert.Equal(333.33m, parcelas[1].Valor);
        Assert.Equal(333.34m, parcelas[2].Valor);
    }

    [Theory]
    [InlineData(10000, 3)]
    [InlineData(10001, 7)]
    [InlineData(1999, 6)]
    public void Gerar_UltimaParcela_NuncaEhMenorQueAsDemais(int valorEmCentavos, int quantidade)
    {
        var parcelas = ParcelasCalculadora.Gerar(valorEmCentavos / 100m, quantidade, PrimeiroVencimento);

        var ultima = parcelas[^1].Valor;
        Assert.All(parcelas, p => Assert.True(ultima >= p.Valor));
    }

    [Fact]
    public void Gerar_UmaParcela_RetornaOValorTotalNoPrimeiroVencimento()
    {
        var parcelas = ParcelasCalculadora.Gerar(250.50m, 1, PrimeiroVencimento);

        var unica = Assert.Single(parcelas);
        Assert.Equal(1, unica.Numero);
        Assert.Equal(250.50m, unica.Valor);
        Assert.Equal(PrimeiroVencimento, unica.DataVencimento);
    }

    [Fact]
    public void Gerar_Numeros_SaoSequenciaisComecandoEmUm()
    {
        var parcelas = ParcelasCalculadora.Gerar(600m, 6, PrimeiroVencimento);

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, parcelas.Select(p => p.Numero));
    }

    [Fact]
    public void Gerar_Vencimentos_SaoMensaisEEmOrdem()
    {
        var parcelas = ParcelasCalculadora.Gerar(400m, 4, PrimeiroVencimento);

        var esperado = new[]
        {
            new DateOnly(2026, 12, 10),
            new DateOnly(2027, 1, 10),
            new DateOnly(2027, 2, 10),
            new DateOnly(2027, 3, 10)
        };
        Assert.Equal(esperado, parcelas.Select(p => p.DataVencimento));
    }

    [Fact]
    public void Gerar_PrimeiroVencimentoNoFimDoMes_NaoArrastaODia()
    {
        var parcelas = ParcelasCalculadora.Gerar(400m, 4, new DateOnly(2027, 1, 31));

        var esperado = new[]
        {
            new DateOnly(2027, 1, 31),
            new DateOnly(2027, 2, 28),
            new DateOnly(2027, 3, 31),
            new DateOnly(2027, 4, 30)
        };
        Assert.Equal(esperado, parcelas.Select(p => p.DataVencimento));
    }

    [Fact]
    public void Gerar_CadaParcelaComUmCentavo_EhAceito()
    {
        var parcelas = ParcelasCalculadora.Gerar(0.03m, 3, PrimeiroVencimento);

        Assert.All(parcelas, p => Assert.Equal(0.01m, p.Valor));
    }

    [Theory]
    [InlineData(1, 3)] // R$ 0,01 em 3x
    [InlineData(2, 3)] // R$ 0,02 em 3x
    public void Gerar_ValorPequenoDemaisParaAQuantidade_LancaExcecao(int valorEmCentavos, int quantidade)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ParcelasCalculadora.Gerar(valorEmCentavos / 100m, quantidade, PrimeiroVencimento));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Gerar_ValorTotalZeroOuNegativo_LancaExcecao(int valorTotal)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ParcelasCalculadora.Gerar(valorTotal, 3, PrimeiroVencimento));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Gerar_QuantidadeMenorQueUm_LancaExcecao(int quantidade)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ParcelasCalculadora.Gerar(100m, quantidade, PrimeiroVencimento));
    }
}