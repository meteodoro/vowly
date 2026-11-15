using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Models;

namespace Vowly.Api.Repositories.Assinaturas;

public interface IAssinaturaRepository
{
    Task<Assinatura?> ObterPorUsuarioAsync(string usuarioId, bool somenteLeitura = false);
    void Adicionar(Assinatura assinatura);
}

public class AssinaturaRepository(AppDbContext context) : IAssinaturaRepository
{
    public Task<Assinatura?> ObterPorUsuarioAsync(string usuarioId, bool somenteLeitura = false)
    {
        IQueryable<Assinatura> consulta = context.Assinaturas;

        if (somenteLeitura)
            consulta = consulta.AsNoTracking();

        return consulta.FirstOrDefaultAsync(a => a.UsuarioId == usuarioId);
    }
    
    public void Adicionar(Assinatura assinatura) => context.Assinaturas.Add(assinatura);
}