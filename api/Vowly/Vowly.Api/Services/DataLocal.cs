namespace Vowly.Api.Services;

public static class DataLocal
{
    private const int DeslocamentoHoras = -3;

    public static DateOnly Hoje(DateTime? agoraUtc = null) =>
        DateOnly.FromDateTime((agoraUtc ?? DateTime.UtcNow).AddHours(DeslocamentoHoras));
}