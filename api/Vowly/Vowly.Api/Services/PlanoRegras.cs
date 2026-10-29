using Vowly.Api.Enums;
using Vowly.Api.Models;

namespace Vowly.Api.Services;

public static class PlanoRegras
{
    public const int LimiteDespesasGratuito = 20;

    public static bool PremiumAtivo(Assinatura? assinatura, DateTime agoraUtc)
    {
        return assinatura is not null && assinatura.Plano == TipoPlano.Premium && assinatura.DataFim > agoraUtc;
    }

    public static bool PodeCadastrarDespesa(Assinatura? assinatura, int quantidadeAtual, DateTime agoraUtc)
    {
        return PremiumAtivo(assinatura, agoraUtc) || quantidadeAtual < LimiteDespesasGratuito;
    }
}

