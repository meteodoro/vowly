using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Enums;
using Vowly.Api.Models;

namespace Vowly.Api.Repositories.Despesas;

// Dados mínimos de cada despesa para o resumo financeiro (consulta só de leitura)
public record DespesaResumo(decimal ValorTotal, bool Paga, bool TemParcelas, decimal ValorParcelasPagas);

public interface IDespesaRepository
{
    Task<List<Despesa>> ListarPorCasamentoAsync(int casamentoId, int? categoriaId, StatusDespesa? status);
    Task<Despesa?> ObterDetalhadaAsync(int casamentoId, int id);
    Task<bool> ExistePorCasamentoAsync(int casamentoId, int id);
    Task<List<DespesaResumo>> ListarResumosPorCasamentoAsync(int casamentoId);
    Task<Despesa?> ObterPorCasamentoAsync(int casamentoId, int id);
    Task<int> ContarPorCasamentoAsync(int casamentoId);
    Task<int> ContarPorUsuarioAsync(string usuarioId);
    Task<bool> PossuiParcelasAsync(int id);
    void Adicionar(Despesa despesa);
    void Remover(Despesa despesa);
}

public class DespesaRepository(AppDbContext context) : IDespesaRepository
{
    public Task<List<Despesa>> ListarPorCasamentoAsync(
        int casamentoId, int? categoriaId, StatusDespesa? status)
    {
        IQueryable<Despesa> consulta = context.Despesas
            .AsNoTracking()
            .Include(d => d.Categoria)
            .Include(d => d.Parcelas)
            .Where(d => d.CasamentoId == casamentoId);

        if (categoriaId is not null)
            consulta = consulta.Where(d => d.CategoriaId == categoriaId);

        if (status is not null)
            consulta = consulta.Where(d => d.Status == status);

        return consulta.OrderByDescending(d => d.Id).ToListAsync();
    }

    public Task<Despesa?> ObterDetalhadaAsync(int casamentoId, int id) =>
        context.Despesas
            .AsNoTracking()
            .Include(d => d.Categoria)
            .Include(d => d.Parcelas)
            .FirstOrDefaultAsync(d => d.Id == id && d.CasamentoId == casamentoId);

    public Task<bool> ExistePorCasamentoAsync(int casamentoId, int id) =>
        context.Despesas.AnyAsync(d => d.Id == id && d.CasamentoId == casamentoId);
    
    public Task<List<DespesaResumo>> ListarResumosPorCasamentoAsync(int casamentoId) =>
        context.Despesas
            .AsNoTracking()
            .Where(d => d.CasamentoId == casamentoId)
            .Select(d => new DespesaResumo(
                d.ValorTotal,
                d.Status == StatusDespesa.Paga,
                d.Parcelas.Any(),
                d.Parcelas.Where(p => p.Paga).Sum(p => p.Valor)))
            .ToListAsync();
    
    public Task<Despesa?> ObterPorCasamentoAsync(int casamentoId, int id) =>
        context.Despesas.FirstOrDefaultAsync(d => d.Id == id && d.CasamentoId == casamentoId);

    public Task<int> ContarPorCasamentoAsync(int casamentoId) =>
        context.Despesas.CountAsync(d => d.CasamentoId == casamentoId);
    
    public Task<int> ContarPorUsuarioAsync(string usuarioId) =>
        context.Despesas.CountAsync(d => d.Casamento.UsuarioId == usuarioId);

    public Task<bool> PossuiParcelasAsync(int id) =>
        context.Despesas.AnyAsync(d => d.Id == id && d.Parcelas.Any());

    public void Adicionar(Despesa despesa) => context.Despesas.Add(despesa);

    public void Remover(Despesa despesa) => context.Despesas.Remove(despesa);
}