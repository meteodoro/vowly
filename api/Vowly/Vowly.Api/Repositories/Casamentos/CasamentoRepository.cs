using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Models;

namespace Vowly.Api.Repositories.Casamentos;

public interface ICasamentoRepository
{
    Task<Casamento?> ObterPorUsuarioAsync(string usuarioId, bool somenteLeitura = false);
    Task<int?> ObterIdPorUsuarioAsync(string usuarioId);
    Task<bool> ExistePorUsuarioAsync(string usuarioId);
    void Adicionar(Casamento casamento);
}

public class CasamentoRepository(AppDbContext context) : ICasamentoRepository
{
    public Task<Casamento?> ObterPorUsuarioAsync(string usuarioId, bool somenteLeitura = false)
    {
        IQueryable<Casamento> consulta = context.Casamentos;

        if (somenteLeitura)
            consulta = consulta.AsNoTracking();

        return consulta.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
    }

    // Os outros services vão usar este método, em vez de cada um ter o seu
    public Task<int?> ObterIdPorUsuarioAsync(string usuarioId) =>
        context.Casamentos
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();

    public Task<bool> ExistePorUsuarioAsync(string usuarioId) =>
        context.Casamentos.AnyAsync(c => c.UsuarioId == usuarioId);

    public void Adicionar(Casamento casamento) => context.Casamentos.Add(casamento);
}