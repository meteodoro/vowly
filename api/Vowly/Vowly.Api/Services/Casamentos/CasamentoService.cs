using Vowly.Api.Dtos.Casamentos;
using Vowly.Api.Enums;
using Vowly.Api.Models;
using Vowly.Api.Repositories.Casamentos;
using Vowly.Api.Repositories.Comum;

namespace Vowly.Api.Services.Casamentos;

public interface ICasamentoService
{
    Task<Resultado<CasamentoResponse>> ObterAsync(string usuarioId);
    Task<Resultado<CasamentoResponse>> CriarAsync(string usuarioId, CriarCasamentoRequest request);
    Task<Resultado<CasamentoResponse>> AtualizarAsync(string usuarioId, AtualizarCasamentoRequest request);
}

public class CasamentoService(ICasamentoRepository casamentoRepository, IUnitOfWork unitOfWork)
    : ICasamentoService
{
    private const string SemCasamento = "Você ainda não cadastrou um casamento.";

    private static readonly string[] CategoriasPadrao =
        ["Buffet", "Vestido", "Decoração", "Fotografia", "Música", "Local"];

    public async Task<Resultado<CasamentoResponse>> ObterAsync(string usuarioId)
    {
        var casamento = await casamentoRepository.ObterPorUsuarioAsync(usuarioId, somenteLeitura: true);

        return casamento is null
            ? Resultado<CasamentoResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento)
            : Resultado<CasamentoResponse>.Ok(Mapear(casamento));
    }

    public async Task<Resultado<CasamentoResponse>> CriarAsync(string usuarioId, CriarCasamentoRequest request)
    {
        if (await casamentoRepository.ExistePorUsuarioAsync(usuarioId))
            return Resultado<CasamentoResponse>.Falha(TipoErro.Conflito,
                "Você já tem um casamento cadastrado. Use a edição.");

        var casamento = new Casamento
        {
            UsuarioId = usuarioId,
            NomeNoivo1 = request.NomeNoivo1,
            NomeNoivo2 = request.NomeNoivo2,
            DataPrevista = request.DataPrevista,
            Cidade = request.Cidade,
            OrcamentoTotal = request.OrcamentoTotal
        };

        // O casamento nasce com as categorias padrão, gravadas junto
        foreach (var nome in CategoriasPadrao)
            casamento.Categorias.Add(new Categoria { Nome = nome });

        casamentoRepository.Adicionar(casamento);
        await unitOfWork.SalvarAsync();

        return Resultado<CasamentoResponse>.Ok(Mapear(casamento));
    }

    public async Task<Resultado<CasamentoResponse>> AtualizarAsync(
        string usuarioId, AtualizarCasamentoRequest request)
    {
        var casamento = await casamentoRepository.ObterPorUsuarioAsync(usuarioId);

        if (casamento is null)
            return Resultado<CasamentoResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        casamento.NomeNoivo1 = request.NomeNoivo1;
        casamento.NomeNoivo2 = request.NomeNoivo2;
        casamento.DataPrevista = request.DataPrevista;
        casamento.Cidade = request.Cidade;
        casamento.OrcamentoTotal = request.OrcamentoTotal;

        await unitOfWork.SalvarAsync();

        return Resultado<CasamentoResponse>.Ok(Mapear(casamento));
    }

    private static CasamentoResponse Mapear(Casamento c) => new()
    {
        Id = c.Id,
        NomeNoivo1 = c.NomeNoivo1,
        NomeNoivo2 = c.NomeNoivo2,
        DataPrevista = c.DataPrevista,
        Cidade = c.Cidade,
        OrcamentoTotal = c.OrcamentoTotal
    };
}