using System.Globalization;
using Vowly.Api.Enums;
using Vowly.Api.Services.Compromissos;

namespace Vowly.Api.Tests;

public class CompromissoRegrasTests
{
    private static readonly DateTime Agora = new(2026, 11, 3, 15, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(-1, StatusCompromisso.Pendente, true)]    // passou há 1 minuto
    [InlineData(0, StatusCompromisso.Pendente, false)]    // no horário exato ainda não é atraso
    [InlineData(1, StatusCompromisso.Pendente, false)]    // ainda vai acontecer
    [InlineData(-1, StatusCompromisso.Concluido, false)]  // concluído nunca é atrasado
    [InlineData(-10000, StatusCompromisso.Concluido, false)]
    public void Atrasado_SoQuandoPendenteEJaPassou(
        int minutosEmRelacaoAAgora, StatusCompromisso status, bool esperado)
    {
        var dataHora = Agora.AddMinutes(minutosEmRelacaoAAgora);

        Assert.Equal(esperado, CompromissoRegras.Atrasado(status, dataHora, Agora));
    }
    
    [Theory]
    [InlineData("2026-11-03T15:01:00Z", 7, true)]  // daqui a 1 minuto
    [InlineData("2026-11-11T02:59:00Z", 7, true)]  // 23:59 de 10/11 em Brasília: ainda dentro dos 7 dias
    [InlineData("2026-11-11T03:00:00Z", 7, false)] // 00:00 de 11/11 em Brasília: já fora
    [InlineData("2026-11-03T23:00:00Z", 0, true)]  // hoje à noite, aviso no próprio dia
    [InlineData("2026-11-04T03:00:00Z", 0, false)] // 00:00 de amanhã
    [InlineData("2026-11-03T14:59:00Z", 7, false)] // já passou: é atrasado, não lembrete
    public void DeveLembrar_UsaADataDeBrasiliaEAAntecedenciaDoCompromisso(
        string dataHoraTexto, int antecedenciaDias, bool esperado)
    {
        var dataHora = DateTime.Parse(dataHoraTexto, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        Assert.Equal(esperado,
            CompromissoRegras.DeveLembrar(StatusCompromisso.Pendente, dataHora, antecedenciaDias, Agora));
    }

    [Fact]
    public void DeveLembrar_CompromissoConcluido_NuncaLembra()
    {
        var amanha = Agora.AddDays(1);

        Assert.False(CompromissoRegras.DeveLembrar(StatusCompromisso.Concluido, amanha, 7, Agora));
    }
}