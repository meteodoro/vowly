using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Models;

namespace Vowly.Api.Repositories.Categorias;

public interface ICategoriaRepository
{
    Task<List<Categoria>> ListarPorCasamentoAsync(int casamentoId);
    Task<Categoria?> ObterPorCasamentoAsync(int casamentoId, int id);
    Task<bool> ExistePorNomeAsync(int casamentoId, string nome, int? ignorarId = null);
    Task<bool> PossuiDespesasAsync(int id);
    void Adicionar(Categoria categoria);
    void Remover(Categoria categoria);
    Task<bool> ExistePorCasamentoAsync(int casamentoId, int id);
}

public class CategoriaRepository(AppDbContext context) : ICategoriaRepository
{
    public Task<List<Categoria>> ListarPorCasamentoAsync(int casamentoId) =>
        context.Categorias
            .AsNoTracking()
            .Where(c => c.CasamentoId == casamentoId)
            .OrderBy(c => c.Nome)
            .ToListAsync();

    // Filtra pelo casamento, então nunca devolve uma categoria de outro usuário
    public Task<Categoria?> ObterPorCasamentoAsync(int casamentoId, int id) =>
        context.Categorias.FirstOrDefaultAsync(c => c.Id == id && c.CasamentoId == casamentoId);

    public Task<bool> ExistePorNomeAsync(int casamentoId, string nome, int? ignorarId = null) =>
        context.Categorias.AnyAsync(c =>
            c.CasamentoId == casamentoId &&
            c.Id != ignorarId &&
            c.Nome.ToLower() == nome.ToLower());

    public Task<bool> PossuiDespesasAsync(int id) =>
        context.Categorias.AnyAsync(c => c.Id == id && c.Despesas.Any());

    public void Adicionar(Categoria categoria) => context.Categorias.Add(categoria);

    public void Remover(Categoria categoria) => context.Categorias.Remove(categoria);
    
    public Task<bool> ExistePorCasamentoAsync(int casamentoId, int id) =>
        context.Categorias.AnyAsync(c => c.Id == id && c.CasamentoId == casamentoId);
}