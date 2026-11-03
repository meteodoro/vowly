namespace Vowly.Api.Services;

public static class DataLocal
{
    private const int DeslocamentoHoras = -3;

    public static DateOnly Hoje(DateTime? agoraUtc = null) =>
        DateOnly.FromDateTime((agoraUtc ?? DateTime.UtcNow).AddHours(DeslocamentoHoras));
    
    public static DateTime ParaUtc(DateTime dataHora) => dataHora.Kind switch
    {
        DateTimeKind.Utc => dataHora,
        DateTimeKind.Local => dataHora.ToUniversalTime(),
        // sem fuso informado: considera horário de Brasília
        _ => DateTime.SpecifyKind(dataHora.AddHours(-DeslocamentoHoras), DateTimeKind.Utc)
    };
}