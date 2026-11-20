using Vowly.Api.Dtos.Compromissos;
using Vowly.Api.Enums;
using Vowly.Api.Models;
using Vowly.Api.Repositories.Casamentos;
using Vowly.Api.Repositories.Comum;
using Vowly.Api.Repositories.Compromissos;
using Vowly.Api.Services.Comum;

namespace Vowly.Api.Services.Compromissos;

public interface ICompromissoService
{
    Task<Resultado<List<CompromissoResponse>>> ListarAsync(string usuarioId, StatusCompromisso? status);
    Task<Resultado<CompromissoResponse>> ObterAsync(string usuarioId, int id);
    Task<Resultado<CompromissoResponse>> CriarAsync(string usuarioId, CriarCompromissoRequest request);
    Task<Resultado<CompromissoResponse>> AtualizarAsync(string usuarioId, int id, AtualizarCompromissoRequest request);
    Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id);
    Task<Resultado<LembretesResponse>> ListarLembretesAsync(string usuarioId);
}

public class CompromissoService(
    ICasamentoRepository casamentoRepository,
    ICompromissoRepository compromissoRepository,
    IUnitOfWork unitOfWork) : ICompromissoService
{
    private const string SemCasamento = "Cadastre o casamento antes de usar a agenda.";
    private const string NaoEncontrado = "Compromisso não encontrado.";

    public async Task<Resultado<List<CompromissoResponse>>> ListarAsync(
        string usuarioId, StatusCompromisso? status)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<CompromissoResponse>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var compromissos = await compromissoRepository.ListarPorCasamentoAsync(casamentoId.Value, status);

        var agora = DateTime.UtcNow;
        return Resultado<List<CompromissoResponse>>.Ok(compromissos.Select(c => Mapear(c, agora)).ToList());
    }

    public async Task<Resultado<CompromissoResponse>> ObterAsync(string usuarioId, int id)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<CompromissoResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var compromisso = await compromissoRepository.ObterPorCasamentoAsync(
            casamentoId.Value, id, somenteLeitura: true);

        return compromisso is null
            ? Resultado<CompromissoResponse>.Falha(TipoErro.NaoEncontrado, NaoEncontrado)
            : Resultado<CompromissoResponse>.Ok(Mapear(compromisso, DateTime.UtcNow));
    }

    public async Task<Resultado<CompromissoResponse>> CriarAsync(
        string usuarioId, CriarCompromissoRequest request)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<CompromissoResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var compromisso = new Compromisso
        {
            CasamentoId = casamentoId.Value,
            Titulo = request.Titulo.Trim(),
            Tipo = request.Tipo.Trim(),
            DataHora = DataLocal.ParaUtc(request.DataHora),
            Local = request.Local,
            Observacoes = request.Observacoes,
            Status = request.Status,
            AntecedenciaLembreteDias = request.AntecedenciaLembreteDias
        };

        compromissoRepository.Adicionar(compromisso);
        await unitOfWork.SalvarAsync();

        return Resultado<CompromissoResponse>.Ok(Mapear(compromisso, DateTime.UtcNow));
    }

    public async Task<Resultado<CompromissoResponse>> AtualizarAsync(
        string usuarioId, int id, AtualizarCompromissoRequest request)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<CompromissoResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var compromisso = await compromissoRepository.ObterPorCasamentoAsync(casamentoId.Value, id);
        if (compromisso is null)
            return Resultado<CompromissoResponse>.Falha(TipoErro.NaoEncontrado, NaoEncontrado);

        compromisso.Titulo = request.Titulo.Trim();
        compromisso.Tipo = request.Tipo.Trim();
        compromisso.DataHora = DataLocal.ParaUtc(request.DataHora);
        compromisso.Local = request.Local;
        compromisso.Observacoes = request.Observacoes;
        compromisso.Status = request.Status;
        compromisso.AntecedenciaLembreteDias = request.AntecedenciaLembreteDias;

        await unitOfWork.SalvarAsync();

        return Resultado<CompromissoResponse>.Ok(Mapear(compromisso, DateTime.UtcNow));
    }

    public async Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var compromisso = await compromissoRepository.ObterPorCasamentoAsync(casamentoId.Value, id);
        if (compromisso is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, NaoEncontrado);

        compromissoRepository.Remover(compromisso);
        await unitOfWork.SalvarAsync();

        return Resultado<bool>.Ok(true);
    }

    public async Task<Resultado<LembretesResponse>> ListarLembretesAsync(string usuarioId)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<LembretesResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var agora = DateTime.UtcNow;

        // A antecedência máxima é 90 dias, então nada além disso pode virar lembrete
        var pendentes = await compromissoRepository.ListarPendentesAteAsync(casamentoId.Value, agora.AddDays(91));

        var atrasados = pendentes
            .Where(c => CompromissoRegras.Atrasado(c.Status, c.DataHora, agora))
            .Select(c => Mapear(c, agora))
            .ToList();

        var proximos = pendentes
            .Where(c => CompromissoRegras.DeveLembrar(c.Status, c.DataHora, c.AntecedenciaLembreteDias, agora))
            .Select(c => Mapear(c, agora))
            .ToList();

        return Resultado<LembretesResponse>.Ok(new LembretesResponse
        {
            Atrasados = atrasados,
            Proximos = proximos
        });
    }

    private static CompromissoResponse Mapear(Compromisso c, DateTime agoraUtc) => new()
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