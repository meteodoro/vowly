using Vowly.Api.Enums;

namespace Vowly.Api.Services;

public static class CompromissoRegras
{
    public static bool Atrasado(StatusCompromisso status, DateTime dataHoraUtc, DateTime agoraUtc) =>
        status == StatusCompromisso.Pendente && dataHoraUtc < agoraUtc;
    
    public static bool DeveLembrar(StatusCompromisso status, DateTime dataHoraUtc, int antecedenciaDias, DateTime agoraUtc)
    {
        if (status != StatusCompromisso.Pendente || dataHoraUtc < agoraUtc)
            return false;

        var limite = DataLocal.Hoje(agoraUtc).AddDays(antecedenciaDias);
        return DataLocal.DataLocalDe(dataHoraUtc) <= limite;
    }
}