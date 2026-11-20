using Vowly.Api.Dtos.Dashboard;
using Vowly.Api.Enums;
using Vowly.Api.Repositories.Casamentos;
using Vowly.Api.Repositories.Despesas;
using Vowly.Api.Services.Comum;

namespace Vowly.Api.Services.Dashboard;

public interface IResumoFinanceiroService
{
    Task<Resultado<ResumoFinanceiroResponse>> ObterAsync(string usuarioId);
}

public class ResumoFinanceiroService(
    ICasamentoRepository casamentoRepository,
    IDespesaRepository despesaRepository) : IResumoFinanceiroService
{
    public async Task<Resultado<ResumoFinanceiroResponse>> ObterAsync(string usuarioId)
    {
        var casamento = await casamentoRepository.ObterPorUsuarioAsync(usuarioId, somenteLeitura: true);

        if (casamento is null)
            return Resultado<ResumoFinanceiroResponse>.Falha(TipoErro.NaoEncontrado,
                "Cadastre o casamento para ver o resumo financeiro.");

        var despesas = await despesaRepository.ListarResumosPorCasamentoAsync(casamento.Id);

        var resumo = ResumoFinanceiroCalculadora.Calcular(casamento.OrcamentoTotal, despesas);

        return Resultado<ResumoFinanceiroResponse>.Ok(new ResumoFinanceiroResponse
        {
            OrcamentoTotal = resumo.OrcamentoTotal,
            Comprometido = resumo.Comprometido,
            Pago = resumo.Pago,
            APagar = resumo.APagar,
            Saldo = resumo.Saldo,
            Percentual = resumo.Percentual,
            Alerta = resumo.Alerta
        });
    }
}