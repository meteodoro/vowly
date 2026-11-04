using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;
using Vowly.Api.Models;

namespace Vowly.Api.Services;

public interface ICompromissoService
{
    Task<Resultado<List<CompromissoRespostaDto>>> ListarAsync(string usuarioId, StatusCompromisso? status);
    Task<Resultado<CompromissoRespostaDto>> ObterAsync(string usuarioId, int id);
    Task<Resultado<CompromissoRespostaDto>> CriarAsync(string usuarioId, SalvarCompromissoDto dto);
    Task<Resultado<CompromissoRespostaDto>> AtualizarAsync(string usuarioId, int id, SalvarCompromissoDto dto);
    Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id);
    Task<Resultado<LembretesRespostaDto>> ListarLembretesAsync(string usuarioId);
}

public class CompromissoService(AppDbContext context) : ICompromissoService
{
    private const string SemCasamento = "Cadastre o casamento antes de usar a agenda.";
    private const string NaoEncontrado = "Compromisso não encontrado.";

    public async Task<Resultado<List<CompromissoRespostaDto>>> ListarAsync(
        string usuarioId, StatusCompromisso? status)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<CompromissoRespostaDto>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var consulta = context.Compromissos
            .AsNoTracking()
            .Where(c => c.CasamentoId == casamentoId);

        if (status is not null)
            consulta = consulta.Where(c => c.Status == status);

        var compromissos = await consulta.OrderBy(c => c.DataHora).ToListAsync();

        var agora = DateTime.UtcNow;
        return Resultado<List<CompromissoRespostaDto>>.Ok(
            compromissos.Select(c => Mapear(c, agora)).ToList());
    }

    public async Task<Resultado<CompromissoRespostaDto>> ObterAsync(string usuarioId, int id)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<CompromissoRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var compromisso = await context.Compromissos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.CasamentoId == casamentoId);

        return compromisso is null
            ? Resultado<CompromissoRespostaDto>.Falha(TipoErro.NaoEncontrado, NaoEncontrado)
            : Resultado<CompromissoRespostaDto>.Ok(Mapear(compromisso, DateTime.UtcNow));
    }

    public async Task<Resultado<CompromissoRespostaDto>> CriarAsync(string usuarioId, SalvarCompromissoDto dto)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<CompromissoRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var compromisso = new Compromisso { CasamentoId = casamentoId.Value };
        Preencher(compromisso, dto);

        context.Compromissos.Add(compromisso);
        await context.SaveChangesAsync();

        return Resultado<CompromissoRespostaDto>.Ok(Mapear(compromisso, DateTime.UtcNow));
    }

    public async Task<Resultado<CompromissoRespostaDto>> AtualizarAsync(
        string usuarioId, int id, SalvarCompromissoDto dto)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<CompromissoRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var compromisso = await context.Compromissos
            .FirstOrDefaultAsync(c => c.Id == id && c.CasamentoId == casamentoId);
        if (compromisso is null)
            return Resultado<CompromissoRespostaDto>.Falha(TipoErro.NaoEncontrado, NaoEncontrado);

        Preencher(compromisso, dto);
        await context.SaveChangesAsync();

        return Resultado<CompromissoRespostaDto>.Ok(Mapear(compromisso, DateTime.UtcNow));
    }

    public async Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var compromisso = await context.Compromissos
            .FirstOrDefaultAsync(c => c.Id == id && c.CasamentoId == casamentoId);
        if (compromisso is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, NaoEncontrado);

        context.Compromissos.Remove(compromisso);
        await context.SaveChangesAsync();

        return Resultado<bool>.Ok(true);
    }
    
    public async Task<Resultado<LembretesRespostaDto>> ListarLembretesAsync(string usuarioId)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<LembretesRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var agora = DateTime.UtcNow;

        // A antecedência máxima é 90 dias
        var limiteConsulta = agora.AddDays(91);

        var pendentes = await context.Compromissos
            .AsNoTracking()
            .Where(c => c.CasamentoId == casamentoId
                        && c.Status == StatusCompromisso.Pendente
                        && c.DataHora <= limiteConsulta)
            .OrderBy(c => c.DataHora)
            .ToListAsync();

        var atrasados = pendentes
            .Where(c => CompromissoRegras.Atrasado(c.Status, c.DataHora, agora))
            .Select(c => Mapear(c, agora))
            .ToList();

        var proximos = pendentes
            .Where(c => CompromissoRegras.DeveLembrar(c.Status, c.DataHora, c.AntecedenciaLembreteDias, agora))
            .Select(c => Mapear(c, agora))
            .ToList();

        return Resultado<LembretesRespostaDto>.Ok(new LembretesRespostaDto
        {
            Atrasados = atrasados,
            Proximos = proximos
        });
    }

    private Task<int?> ObterCasamentoIdAsync(string usuarioId) =>
        context.Casamentos
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();

    private static void Preencher(Compromisso compromisso, SalvarCompromissoDto dto)
    {
        compromisso.Titulo = dto.Titulo.Trim();
        compromisso.Tipo = dto.Tipo.Trim();
        compromisso.DataHora = DataLocal.ParaUtc(dto.DataHora);
        compromisso.Local = dto.Local;
        compromisso.Observacoes = dto.Observacoes;
        compromisso.Status = dto.Status;
        compromisso.AntecedenciaLembreteDias = dto.AntecedenciaLembreteDias;
    }

    private static CompromissoRespostaDto Mapear(Compromisso c, DateTime agoraUtc) => new()
    {
        Id = c.Id,
        Titulo = c.Titulo,
        Tipo = c.Tipo,
        DataHora = c.DataHora,
        Local = c.Local,
        Observacoes = c.Observacoes,
        Status = c.Status,
        AntecedenciaLembreteDias = c.AntecedenciaLembreteDias,
        Atrasado = CompromissoRegras.Atrasado(c.Status, c.DataHora, agoraUtc)
    };
}