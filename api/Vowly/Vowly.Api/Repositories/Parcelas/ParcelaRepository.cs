using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Models;

namespace Vowly.Api.Repositories.Parcelas;

// Parcela em aberto, já com os dados da despesa (consulta só de leitura)
public record ParcelaPendenteConsulta(
    int ParcelaId,
    int DespesaId,
    string DespesaDescricao,
    string CategoriaNome,
    int Numero,
    int TotalParcelas,
    decimal Valor,
    DateOnly DataVencimento);

public interface IParcelaRepository
{
    Task<List<Parcela>> ListarPorDespesaAsync(int despesaId);

    // Rastreada e com a despesa carregada: serve para registrar o pagamento
    Task<Parcela?> ObterPorCasamentoAsync(int casamentoId, int id);

    Task<int> ContarEmAbertoAsync(int despesaId, int ignorarParcelaId);
    Task<List<ParcelaPendenteConsulta>> ListarPendentesAsync(int casamentoId, DateOnly vencimentoAte);
    void AdicionarVarias(IEnumerable<Parcela> parcelas);
}

public class ParcelaRepository(AppDbContext context) : IParcelaRepository
{
    public Task<List<Parcela>> ListarPorDespesaAsync(int despesaId) =>
        context.Parcelas
            .AsNoTracking()
            .Where(p => p.DespesaId == despesaId)
            .OrderBy(p => p.Numero)
            .ToListAsync();

    // Filtra pelo casamento da despesa, então nunca devolve uma parcela de outro usuário
    public Task<Parcela?> ObterPorCasamentoAsync(int casamentoId, int id) =>
        context.Parcelas
            .Include(p => p.Despesa)
            .FirstOrDefaultAsync(p => p.Id == id && p.Despesa.CasamentoId == casamentoId);

    public Task<int> ContarEmAbertoAsync(int despesaId, int ignorarParcelaId) =>
        context.Parcelas.CountAsync(p =>
            p.DespesaId == despesaId && !p.Paga && p.Id != ignorarParcelaId);

    // Todas as atrasadas (vencimento no passado) e as que vencem até a data limite
    public Task<List<ParcelaPendenteConsulta>> ListarPendentesAsync(int casamentoId, DateOnly vencimentoAte) =>
        context.Parcelas
            .AsNoTracking()
            .Where(p => p.Despesa.CasamentoId == casamentoId && !p.Paga && p.DataVencimento <= vencimentoAte)
            .OrderBy(p => p.DataVencimento).ThenBy(p => p.DespesaId).ThenBy(p => p.Numero)
            .Select(p => new ParcelaPendenteConsulta(
                p.Id,
                p.DespesaId,
                p.Despesa.Descricao,
                p.Despesa.Categoria.Nome,
                p.Numero,
                p.Despesa.Parcelas.Count,
                p.Valor,
                p.DataVencimento))
            .ToListAsync();

    public void AdicionarVarias(IEnumerable<Parcela> parcelas) => context.Parcelas.AddRange(parcelas);
}