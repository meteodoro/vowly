using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Enums;
using Vowly.Api.Models;

namespace Vowly.Api.Repositories.Compromissos;

public interface ICompromissoRepository
{
    Task<List<Compromisso>> ListarPorCasamentoAsync(int casamentoId, StatusCompromisso? status);
    Task<List<Compromisso>> ListarPendentesAteAsync(int casamentoId, DateTime dataHoraLimiteUtc);
    Task<Compromisso?> ObterPorCasamentoAsync(int casamentoId, int id, bool somenteLeitura = false);
    void Adicionar(Compromisso compromisso);
    void Remover(Compromisso compromisso);
}

public class CompromissoRepository(AppDbContext context) : ICompromissoRepository
{
    public Task<List<Compromisso>> ListarPorCasamentoAsync(int casamentoId, StatusCompromisso? status)
    {
        IQueryable<Compromisso> consulta = context.Compromissos
            .AsNoTracking()
            .Where(c => c.CasamentoId == casamentoId);

        if (status is not null)
            consulta = consulta.Where(c => c.Status == status);

        return consulta.OrderBy(c => c.DataHora).ToListAsync();
    }

    // Compromissos pendentes, os atrasados e os que acontecem até o limite
    public Task<List<Compromisso>> ListarPendentesAteAsync(int casamentoId, DateTime dataHoraLimiteUtc) =>
        context.Compromissos
            .AsNoTracking()
            .Where(c => c.CasamentoId == casamentoId
                        && c.Status == StatusCompromisso.Pendente
                        && c.DataHora <= dataHoraLimiteUtc)
            .OrderBy(c => c.DataHora)
            .ToListAsync();

    // Filtra pelo casamento, então nunca devolve um compromisso de outro usuário
    public Task<Compromisso?> ObterPorCasamentoAsync(int casamentoId, int id, bool somenteLeitura = false)
    {
        IQueryable<Compromisso> consulta = context.Compromissos;

        if (somenteLeitura)
            consulta = consulta.AsNoTracking();

        return consulta.FirstOrDefaultAsync(c => c.Id == id && c.CasamentoId == casamentoId);
    }

    public void Adicionar(Compromisso compromisso) => context.Compromissos.Add(compromisso);

    public void Remover(Compromisso compromisso) => context.Compromissos.Remove(compromisso);
}