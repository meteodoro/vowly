using Microsoft.Extensions.Options;
using Vowly.Api.Dtos.Relatorios;
using Vowly.Api.Repositories.Assinaturas;
using Vowly.Api.Repositories.Cobrancas;
using Vowly.Api.Services.Comum;

namespace Vowly.Api.Services.Relatorios;

public interface IRelatorioEmpresaService
{
    Task<RelatorioEmpresaResponse> ObterAsync(int meses);
}

public class RelatorioEmpresaService(
    ICobrancaRepository cobrancaRepository,
    IAssinaturaRepository assinaturaRepository,
    IOptions<OpcoesPlano> opcoesPlano,
    IOptions<OpcoesEmpresa> opcoesEmpresa) : IRelatorioEmpresaService
{
    public async Task<RelatorioEmpresaResponse> ObterAsync(int meses)
    {
        var agora = DateTime.UtcNow;
        var hoje = DataLocal.Hoje(agora);
        var preco = opcoesPlano.Value.PrecoPremium;
        var custosMensais = opcoesEmpresa.Value.CustosFixosMensais;

        var primeiroMes = new DateOnly(hoje.Year, hoje.Month, 1).AddMonths(-(meses - 1));
        var inicioUtc = DataLocal.InicioDoDiaUtc(primeiroMes);

        var cobrancas = await cobrancaRepository.ListarDesdeAsync(inicioUtc);

        // Agrupa pelo mês de Brasília, não pelo mês em UTC
        var porMes = cobrancas
            .GroupBy(c =>
            {
                var data = DataLocal.DataLocalDe(c.DataCobranca);
                return (data.Year, data.Month);
            })
            .ToDictionary(g => g.Key, g => (Assinantes: g.Count(), Receita: g.Sum(c => c.Valor)));

        var listaMeses = new List<RelatorioMesResponse>();
        for (var i = 0; i < meses; i++)
        {
            var mes = primeiroMes.AddMonths(i);
            porMes.TryGetValue((mes.Year, mes.Month), out var dados);

            listaMeses.Add(new RelatorioMesResponse
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
        var vigentes = await assinaturaRepository.ContarPremiumVigentesAsync(agora);

        return new RelatorioEmpresaResponse
        {
            PrecoPremium = preco,
            CustosFixosMensais = custosMensais,
            Custos = opcoesEmpresa.Value.CustosFixos
                .Select(c => new ItemCustoResponse { Nome = c.Nome, Valor = c.Valor })
                .ToList(),
            PontoEquilibrio = pontoEquilibrio,
            AssinantesPremiumVigentes = vigentes,
            FaltamParaEquilibrio = RelatorioEmpresaRegras.AssinantesFaltantes(pontoEquilibrio, vigentes),
            Meses = listaMeses
        };
    }
}