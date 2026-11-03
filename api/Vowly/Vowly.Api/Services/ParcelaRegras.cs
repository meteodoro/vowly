using Vowly.Api.Enums;

namespace Vowly.Api.Services;

public static class ParcelaRegras
{
    public static SituacaoParcela Classificar(DateOnly vencimento, DateOnly hoje) =>
        vencimento < hoje ? SituacaoParcela.Atrasada : SituacaoParcela.AVencer;

    // Número negativo = x dias de atraso, 0 = vence hoje
    public static int DiasParaVencer(DateOnly vencimento, DateOnly hoje) =>
        vencimento.DayNumber - hoje.DayNumber;
}