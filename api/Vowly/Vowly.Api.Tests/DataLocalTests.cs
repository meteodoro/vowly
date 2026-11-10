using Vowly.Api.Services;

namespace Vowly.Api.Tests;

public class DataLocalTests
{
    [Fact]
    public void Hoje_DeNoiteNoBrasil_AindaEhODiaAnterior()
    {
        // 01:00 UTC do dia 4 = 22:00 do dia 3 em Brasília
        var agoraUtc = new DateTime(2026, 11, 4, 1, 0, 0, DateTimeKind.Utc);

        Assert.Equal(new DateOnly(2026, 11, 3), DataLocal.Hoje(agoraUtc));
    }

    [Theory]
    [InlineData(2, 59, 2)] // 02:59 UTC = 23:59 do dia 2 em Brasília
    [InlineData(3, 0, 3)]  // 03:00 UTC = 00:00 do dia 3 em Brasília
    public void Hoje_NaVirada_RespeitaOFusoDeBrasilia(int hora, int minuto, int diaEsperado)
    {
        var agoraUtc = new DateTime(2026, 11, 3, hora, minuto, 0, DateTimeKind.Utc);

        Assert.Equal(new DateOnly(2026, 11, diaEsperado), DataLocal.Hoje(agoraUtc));
    }
    
    [Fact]
    public void ParaUtc_SemFusoInformado_AssumeHorarioDeBrasilia()
    {
        var semFuso = new DateTime(2026, 12, 10, 15, 0, 0, DateTimeKind.Unspecified);

        var utc = DataLocal.ParaUtc(semFuso);

        Assert.Equal(new DateTime(2026, 12, 10, 18, 0, 0), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
    }

    [Fact]
    public void ParaUtc_JaEmUtc_NaoAltera()
    {
        var emUtc = new DateTime(2026, 12, 10, 18, 0, 0, DateTimeKind.Utc);

        Assert.Equal(emUtc, DataLocal.ParaUtc(emUtc));
    }
    
    [Theory]
    [InlineData(2, 59, 3)] // 02:59 UTC de 4/11 = 23:59 de 3/11 em Brasília
    [InlineData(3, 0, 4)]  // 03:00 UTC de 4/11 = 00:00 de 4/11 em Brasília
    public void DataLocalDe_RespeitaOFusoDeBrasilia(int hora, int minuto, int diaEsperado)
    {
        var utc = new DateTime(2026, 11, 4, hora, minuto, 0, DateTimeKind.Utc);

        Assert.Equal(new DateOnly(2026, 11, diaEsperado), DataLocal.DataLocalDe(utc));
    }
    
    [Fact]
    public void InicioDoDiaUtc_EhMeiaNoiteDeBrasiliaEmUtc()
    {
        var inicio = DataLocal.InicioDoDiaUtc(new DateOnly(2026, 11, 1));

        Assert.Equal(new DateTime(2026, 11, 1, 3, 0, 0), inicio);
        Assert.Equal(DateTimeKind.Utc, inicio.Kind);
    }
}