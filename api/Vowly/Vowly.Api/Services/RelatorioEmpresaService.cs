using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vowly.Api.Data;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;

namespace Vowly.Api.Services;

public interface IRelatorioEmpresaService
{
    Task<RelatorioEmpresaRespostaDto> ObterAsync(int meses);
}

public class RelatorioEmpresaService(
    AppDbContext context,
    IOptions<OpcoesPlano> opcoesPlano,
    IOptions<OpcoesEmpresa> opcoesEmpresa) : IRelatorioEmpresaService
{
    public async Task<RelatorioEmpresaRespostaDto> ObterAsync(int meses)
    {
        var agora = DateTime.UtcNow;
        var hoje = DataLocal.Hoje(agora);
        var preco = opcoesPlano.Value.PrecoPremium;
        var custosMensais = opcoesEmpresa.Value.CustosFixosMensais;

        var primeiroMes = new DateOnly(hoje.Year, hoje.Month, 1).AddMonths(-(meses - 1));
        var inicioUtc = DataLocal.InicioDoDiaUtc(primeiroMes);

        var cobrancas = await context.Cobrancas
            .AsNoTracking()
            .Where(c => c.DataCobranca >= inicioUtc)
            .Select(c => new { c.DataCobranca, c.Valor })
            .ToListAsync();

        // Agrupa pelo mês de Brasília, não pelo mês em UTC
        var porMes = cobrancas
            .GroupBy(c =>
            {
                var data = DataLocal.DataLocalDe(c.DataCobranca);
                return (data.Year, data.Month);
            })
            .ToDictionary(g => g.Key, g => (Assinantes: g.Count(), Receita: g.Sum(c => c.Valor)));

        var listaMeses = new List<RelatorioMesDto>();
        for (var i = 0; i < meses; i++)
        {
            var mes = primeiroMes.AddMonths(i);
            porMes.TryGetValue((mes.Year, mes.Month), out var dados);

            listaMeses.Add(new RelatorioMesDto
            {
                Ano = mes.Year,
                Mes = mes.Month,
                Assinantes = dados.Assinantes,
                Receita = dados.Receita,
                Custos = custosMensais,
                Lucro = RelatorioEmpresaRegras.Lucro(dados.Receita, custosMensais)
            });
        }

        var pontoEquilibrio = RelatorioEmpresaRegras.PontoEquilibrio(custosMensais, preco);

        var vigentes = await context.Assinaturas
            .CountAsync(a => a.Plano == TipoPlano.Premium && a.DataFim > agora);

        return new RelatorioEmpresaRespostaDto
        {
            PrecoPremium = preco,
            CustosFixosMensais = custosMensais,
            Custos = opcoesEmpresa.Value.CustosFixos,
            PontoEquilibrio = pontoEquilibrio,
            AssinantesPremiumVigentes = vigentes,
            FaltamParaEquilibrio = RelatorioEmpresaRegras.AssinantesFaltantes(pontoEquilibrio, vigentes),
            Meses = listaMeses
        };
    }
}