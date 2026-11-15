using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;

namespace Vowly.Api.Repositories.Fornecedores;

public interface IFornecedorRepository
{
    Task<bool> ExistePorCasamentoAsync(int casamentoId, int id);
}

public class FornecedorRepository(AppDbContext context) : IFornecedorRepository
{
    public Task<bool> ExistePorCasamentoAsync(int casamentoId, int id) =>
        context.Fornecedores.AnyAsync(f => f.Id == id && f.CasamentoId == casamentoId);
}