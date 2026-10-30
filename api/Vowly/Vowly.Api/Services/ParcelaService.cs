using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;
using Vowly.Api.Models;

namespace Vowly.Api.Services;

public interface IParcelaService
{
    Task<Resultado<List<ParcelaRespostaDto>>> ListarAsync(string usuarioId, int despesaId);
    Task<Resultado<List<ParcelaRespostaDto>>> GerarAsync(string usuarioId, int despesaId, GerarParcelasDto dto);
}

public class ParcelaService(AppDbContext context) : IParcelaService
{
    private const string SemCasamento = "Cadastre o casamento antes de usar as despesas.";
    private const string DespesaNaoEncontrada = "Despesa não encontrada.";

    public async Task<Resultado<List<ParcelaRespostaDto>>> ListarAsync(string usuarioId, int despesaId)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesaExiste = await context.Despesas
            .AnyAsync(d => d.Id == despesaId && d.CasamentoId == casamentoId);
        if (!despesaExiste)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.NaoEncontrado, DespesaNaoEncontrada);

        var parcelas = await context.Parcelas
            .AsNoTracking()
            .Where(p => p.DespesaId == despesaId)
            .OrderBy(p => p.Numero)
            .Select(p => new ParcelaRespostaDto
            {
                Id = p.Id,
                DespesaId = p.DespesaId,
                Numero = p.Numero,
                Valor = p.Valor,
                DataVencimento = p.DataVencimento,
                DataPagamento = p.DataPagamento,
                Paga = p.Paga
            })
            .ToListAsync();

        return Resultado<List<ParcelaRespostaDto>>.Ok(parcelas);
    }

    public async Task<Resultado<List<ParcelaRespostaDto>>> GerarAsync(
        string usuarioId, int despesaId, GerarParcelasDto dto)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesa = await context.Despesas
            .FirstOrDefaultAsync(d => d.Id == despesaId && d.CasamentoId == casamentoId);
        if (despesa is null)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.NaoEncontrado, DespesaNaoEncontrada);

        var jaParcelada = await context.Parcelas.AnyAsync(p => p.DespesaId == despesaId);
        if (jaParcelada)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.Conflito, "Esta despesa já foi parcelada.");

        if (despesa.Status != StatusDespesa.APagar)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.Conflito,
                "Só é possível parcelar uma despesa com status 'a pagar'.");

        var calculadas = ParcelasCalculadora.Gerar(despesa.ValorTotal, dto.Quantidade, dto.PrimeiroVencimento);

        var parcelas = calculadas
            .Select(p => new Parcela
            {
                DespesaId = despesa.Id,
                Numero = p.Numero,
                Valor = p.Valor,
                DataVencimento = p.DataVencimento,
                Paga = false
            })
            .ToList();

        context.Parcelas.AddRange(parcelas);
        despesa.Status = StatusDespesa.EmPagamento;

        // Parcelas e status da despesa são gravados juntos, na mesma transação
        await context.SaveChangesAsync();

        return Resultado<List<ParcelaRespostaDto>>.Ok(parcelas.Select(Mapear).ToList());
    }

    private Task<int?> ObterCasamentoIdAsync(string usuarioId) =>
        context.Casamentos
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();

    private static ParcelaRespostaDto Mapear(Parcela p) => new()
    {
        Id = p.Id,
        DespesaId = p.DespesaId,
        Numero = p.Numero,
        Valor = p.Valor,
        DataVencimento = p.DataVencimento,
        DataPagamento = p.DataPagamento,
        Paga = p.Paga
    };
}