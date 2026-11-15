using Vowly.Api.Data;
using Vowly.Api.Models;

namespace Vowly.Api.Repositories.Cobrancas;

public interface ICobrancaRepository
{
    void Adicionar(Cobranca cobranca);
}

public class CobrancaRepository(AppDbContext context) : ICobrancaRepository
{
    public void Adicionar(Cobranca cobranca) => context.Cobrancas.Add(cobranca);
}