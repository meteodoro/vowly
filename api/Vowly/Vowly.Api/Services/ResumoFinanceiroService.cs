using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;

namespace Vowly.Api.Services;

public interface IResumoFinanceiroService
{
    Task<Resultado<ResumoFinanceiroRespostaDto>> ObterAsync(string UsuarioId);
}

public class ResumoFinanceiroService(AppDbContext context) : IResumoFinanceiroService
{
    public async Task<Resultado<ResumoFinanceiroRespostaDto>> ObterAsync(string usuarioId)
    {
        var casamento = await context.Casamentos
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => new { c.Id, c.OrcamentoTotal })
            .FirstOrDefaultAsync();
        
        if (casamento is null)
            return Resultado<ResumoFinanceiroRespostaDto>.Falha(TipoErro.NaoEncontrado, "Cadastre o casamento para ver o resumo financeiro.");

        var despesas = await context.Despesas
            .AsNoTracking()
            .Where(d => d.CasamentoId == casamento.Id)
            .Select(d => new DespesaResumo(
                d.ValorTotal,
                d.Status == StatusDespesa.Paga,
                d.Parcelas.Any(),
                d.Parcelas.Where(p => p.Paga).Sum(p => p.Valor)))
            .ToListAsync();

        var resumo = ResumoFinanceiroCalculadora.Calcular(casamento.OrcamentoTotal, despesas);

        return Resultado<ResumoFinanceiroRespostaDto>.Ok(new ResumoFinanceiroRespostaDto
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
