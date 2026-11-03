using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vowly.Api.Data;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;
using Vowly.Api.Models;

namespace Vowly.Api.Services;

public interface IAssinaturaService
{
    Task<AssinaturaRespostaDto> ObterAsync(string usuarioId);
    Task<Resultado<AssinaturaRespostaDto>> AssinarAsync(string usuarioId);
    Task<Resultado<AssinaturaRespostaDto>> CancelarAsync(string usuarioId);
}

public class AssinaturaService(AppDbContext context, IOptions<OpcoesPlano> opcoesPlano) : IAssinaturaService
{
    public async Task<AssinaturaRespostaDto> ObterAsync(string usuarioId)
    {
        var assinatura = await BuscarAssinaturaAsync(usuarioId);
        return await MontarRespostaAsync(usuarioId, assinatura);
    }

    public async Task<Resultado<AssinaturaRespostaDto>> AssinarAsync(string usuarioId)
    {
        var agora = DateTime.UtcNow;
        var assinatura = await BuscarAssinaturaAsync(usuarioId);

        if (assinatura is not null && PlanoRegras.PremiumAtivo(assinatura, agora))
        {
            if (assinatura.Status == StatusAssinatura.Ativa)
                return Resultado<AssinaturaRespostaDto>.Falha(TipoErro.Conflito, "Você já tem o Premium ativo.");

            // Cancelada, mas ainda dentro da vigência já paga: reativa sem nova cobrança
            assinatura.Status = StatusAssinatura.Ativa;
        }
        else
        {
            if (assinatura is null)
            {
                assinatura = new Assinatura { UsuarioId = usuarioId };
                context.Assinaturas.Add(assinatura);
            }

            // Cobrança simulada: nenhum pagamento real é processado
            assinatura.Plano = TipoPlano.Premium;
            assinatura.Status = StatusAssinatura.Ativa;
            assinatura.DataInicio = agora;
            assinatura.DataFim = agora.AddMonths(1);
            assinatura.ValorMensal = opcoesPlano.Value.PrecoPremium;
        }

        await context.SaveChangesAsync();

        return Resultado<AssinaturaRespostaDto>.Ok(await MontarRespostaAsync(usuarioId, assinatura));
    }

    public async Task<Resultado<AssinaturaRespostaDto>> CancelarAsync(string usuarioId)
    {
        var assinatura = await BuscarAssinaturaAsync(usuarioId);

        if (assinatura is null
            || !PlanoRegras.PremiumAtivo(assinatura, DateTime.UtcNow)
            || assinatura.Status != StatusAssinatura.Ativa)
        {
            return Resultado<AssinaturaRespostaDto>.Falha(TipoErro.Conflito,
                "Você não tem uma assinatura Premium ativa para cancelar.");
        }

        // A DataFim não muda: o Premium vale até o fim do período já contratado
        assinatura.Status = StatusAssinatura.Cancelada;
        await context.SaveChangesAsync();

        return Resultado<AssinaturaRespostaDto>.Ok(await MontarRespostaAsync(usuarioId, assinatura));
    }

    private Task<Assinatura?> BuscarAssinaturaAsync(string usuarioId) =>
        context.Assinaturas.FirstOrDefaultAsync(a => a.UsuarioId == usuarioId);

    private async Task<AssinaturaRespostaDto> MontarRespostaAsync(string usuarioId, Assinatura? assinatura)
    {
        var premiumVigente = PlanoRegras.PremiumAtivo(assinatura, DateTime.UtcNow);

        var despesas = await context.Despesas
            .CountAsync(d => d.Casamento.UsuarioId == usuarioId);

        SituacaoPlano situacao;
        if (assinatura is null || assinatura.Plano != TipoPlano.Premium)
            situacao = SituacaoPlano.Gratuito;
        else if (!premiumVigente)
            situacao = SituacaoPlano.PremiumVencido;
        else
            situacao = assinatura.Status == StatusAssinatura.Cancelada
                ? SituacaoPlano.PremiumCancelado
                : SituacaoPlano.PremiumAtivo;

        return new AssinaturaRespostaDto
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