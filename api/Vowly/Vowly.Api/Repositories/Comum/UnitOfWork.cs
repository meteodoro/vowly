using Vowly.Api.Data;

namespace Vowly.Api.Repositories.Comum;

public interface IUnitOfWork
{
    Task<int> SalvarAsync();
}

// Grava de uma vez tudo o que os repositórios adicionaram, alteraram ou removeram
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> SalvarAsync() => context.SaveChangesAsync();
}