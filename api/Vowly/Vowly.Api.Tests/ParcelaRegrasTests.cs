using Vowly.Api.Enums;
using Vowly.Api.Services;

namespace Vowly.Api.Tests;

public class ParcelaRegrasTests
{
    private static readonly DateOnly Hoje = new(2026, 11, 3);

    [Theory]
    [InlineData(-30, SituacaoParcela.Atrasada)]
    [InlineData(-1, SituacaoParcela.Atrasada)]
    [InlineData(0, SituacaoParcela.AVencer)]  // vence hoje: ainda não está atrasada
    [InlineData(1, SituacaoParcela.AVencer)]
    [InlineData(30, SituacaoParcela.AVencer)]
    public void Classificar_RespeitaOLimiteDeHoje(int diasEmRelacaoAHoje, SituacaoParcela esperado)
    {
        var vencimento = Hoje.AddDays(diasEmRelacaoAHoje);

        Assert.Equal(esperado, ParcelaRegras.Classificar(vencimento, Hoje));
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(12)]
    public void DiasParaVencer_NegativoParaAtrasoEZeroParaHoje(int dias)
    {
        Assert.Equal(dias, ParcelaRegras.DiasParaVencer(Hoje.AddDays(dias), Hoje));
    }

    [Fact]
    public void DiasParaVencer_AtravessaMudancaDeMes()
    {
        Assert.Equal(2, ParcelaRegras.DiasParaVencer(new DateOnly(2026, 12, 1), new DateOnly(2026, 11, 29)));
    }
}