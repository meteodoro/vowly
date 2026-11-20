using Microsoft.Extensions.Options;
using Vowly.Api.Dtos.Assinaturas;
using Vowly.Api.Enums;
using Vowly.Api.Models;
using Vowly.Api.Repositories.Assinaturas;
using Vowly.Api.Repositories.Cobrancas;
using Vowly.Api.Repositories.Comum;
using Vowly.Api.Repositories.Despesas;
using Vowly.Api.Services.Comum;

namespace Vowly.Api.Services.Assinaturas;

public interface IAssinaturaService
{
    Task<AssinaturaResponse> ObterAsync(string usuarioId);
    Task<Resultado<AssinaturaResponse>> AssinarAsync(string usuarioId);
    Task<Resultado<AssinaturaResponse>> CancelarAsync(string usuarioId);
}

public class AssinaturaService(
    IAssinaturaRepository assinaturaRepository,
    ICobrancaRepository cobrancaRepository,
    IDespesaRepository despesaRepository,
    IUnitOfWork unitOfWork,
    IOptions<OpcoesPlano> opcoesPlano) : IAssinaturaService
{
    public async Task<AssinaturaResponse> ObterAsync(string usuarioId)
    {
        var assinatura = await assinaturaRepository.ObterPorUsuarioAsync(usuarioId, somenteLeitura: true);
        return await MontarRespostaAsync(usuarioId, assinatura);
    }

    public async Task<Resultado<AssinaturaResponse>> AssinarAsync(string usuarioId)
    {
        var agora = DateTime.UtcNow;
        var assinatura = await assinaturaRepository.ObterPorUsuarioAsync(usuarioId);

        if (assinatura is not null && PlanoRegras.PremiumAtivo(assinatura, agora))
        {
            if (assinatura.Status == StatusAssinatura.Ativa)
                return Resultado<AssinaturaResponse>.Falha(TipoErro.Conflito, "Você já tem o Premium ativo.");

            // Cancelada, mas ainda dentro da vigência já paga: reativa sem nova cobrança
            assinatura.Status = StatusAssinatura.Ativa;
        }
        else
        {
            if (assinatura is null)
            {
                assinatura = new Assinatura { UsuarioId = usuarioId };
                assinaturaRepository.Adicionar(assinatura);
            }

            // Cobrança simulada: nenhum pagamento real é processado
            assinatura.Plano = TipoPlano.Premium;
            assinatura.Status = StatusAssinatura.Ativa;
            assinatura.DataInicio = agora;
            assinatura.DataFim = agora.AddMonths(1);
            assinatura.ValorMensal = opcoesPlano.Value.PrecoPremium;

            cobrancaRepository.Adicionar(new Cobranca
            {
                UsuarioId = usuarioId,
                DataCobranca = agora,
                Valor = opcoesPlano.Value.PrecoPremium
            });
        }

        // Assinatura e cobrança são gravadas juntas, na mesma transação
        await unitOfWork.SalvarAsync();

        return Resultado<AssinaturaResponse>.Ok(await MontarRespostaAsync(usuarioId, assinatura));
    }

    public async Task<Resultado<AssinaturaResponse>> CancelarAsync(string usuarioId)
    {
        var assinatura = await assinaturaRepository.ObterPorUsuarioAsync(usuarioId);

        if (assinatura is null
            || !PlanoRegras.PremiumAtivo(assinatura, DateTime.UtcNow)
            || assinatura.Status != StatusAssinatura.Ativa)
        {
            return Resultado<AssinaturaResponse>.Falha(TipoErro.Conflito,
                "Você não tem uma assinatura Premium ativa para cancelar.");
        }

        // A DataFim não muda: o Premium vale até o fim do período já contratado
        assinatura.Status = StatusAssinatura.Cancelada;
        await unitOfWork.SalvarAsync();

        return Resultado<AssinaturaResponse>.Ok(await MontarRespostaAsync(usuarioId, assinatura));
    }

    private async Task<AssinaturaResponse> MontarRespostaAsync(string usuarioId, Assinatura? assinatura)
    {
        var premiumVigente = PlanoRegras.PremiumAtivo(assinatura, DateTime.UtcNow);
        var despesas = await despesaRepository.ContarPorUsuarioAsync(usuarioId);

        SituacaoPlano situacao;
        if (assinatura is null || assinatura.Plano != TipoPlano.Premium)
            situacao = SituacaoPlano.Gratuito;
        else if (!premiumVigente)
            situacao = SituacaoPlano.PremiumVencido;
        else
            situacao = assinatura.Status == StatusAssinatura.Cancelada
                ? SituacaoPlano.PremiumCancelado
                : SituacaoPlano.PremiumAtivo;

        return new AssinaturaResponse
        {
            Situacao = situacao,
            PremiumVigente = premiumVigente,
            DataInicio = assinatura?.DataInicio,
            DataFim = assinatura?.DataFim,
            ValorMensal = assinatura?.ValorMensal,
            DespesasCadastradas = despesas,
            LimiteDespesas = premiumVigente ? null : opcoesPlano.Value.LimiteDespesasGratuito,
            PrecoPremium = opcoesPlano.Value.PrecoPremium
        };
    }
}