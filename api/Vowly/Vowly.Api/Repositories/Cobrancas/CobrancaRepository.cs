using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Models;

namespace Vowly.Api.Repositories.Cobrancas;

    public record CobrancaResumo(DateTime DataCobranca, decimal Valor);

public interface ICobrancaRepository
{
    Task<List<CobrancaResumo>> ListarDesdeAsync(DateTime dataCobrancaInicioUtc);
    void Adicionar(Cobranca cobranca);
}

public class CobrancaRepository(AppDbContext context) : ICobrancaRepository
{
    public Task<List<CobrancaResumo>> ListarDesdeAsync(DateTime dataCobrancaInicioUtc) =>
        context.Cobrancas
            .AsNoTracking()
            .Where(c => c.DataCobranca >= dataCobrancaInicioUtc)
            .Select(c => new CobrancaResumo(c.DataCobranca, c.Valor))
            .ToListAsync();

    public void Adicionar(Cobranca cobranca) => context.Cobrancas.Add(cobranca);
}