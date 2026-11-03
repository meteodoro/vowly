using Vowly.Api.Enums;

namespace Vowly.Api.Services;

public static class CompromissoRegras
{
    public static bool Atrasado(StatusCompromisso status, DateTime dataHoraUtc, DateTime agoraUtc) =>
        status == StatusCompromisso.Pendente && dataHoraUtc < agoraUtc;
}