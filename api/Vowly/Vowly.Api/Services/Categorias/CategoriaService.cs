using Vowly.Api.Dtos.Categorias;
using Vowly.Api.Enums;
using Vowly.Api.Models;
using Vowly.Api.Repositories.Casamentos;
using Vowly.Api.Repositories.Categorias;
using Vowly.Api.Repositories.Comum;

namespace Vowly.Api.Services.Categorias;

public interface ICategoriaService
{
    Task<Resultado<List<CategoriaResponse>>> ListarAsync(string usuarioId);
    Task<Resultado<CategoriaResponse>> CriarAsync(string usuarioId, CriarCategoriaRequest request);
    Task<Resultado<CategoriaResponse>> AtualizarAsync(string usuarioId, int id, AtualizarCategoriaRequest request);
    Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id);
}

public class CategoriaService(
    ICasamentoRepository casamentoRepository,
    ICategoriaRepository categoriaRepository,
    IUnitOfWork unitOfWork) : ICategoriaService
{
    private const string SemCasamento = "Cadastre o casamento antes de usar as categorias.";
    private const string NomeRepetido = "Já existe uma categoria com esse nome.";
    private const string NaoEncontrada = "Categoria não encontrada.";

    public async Task<Resultado<List<CategoriaResponse>>> ListarAsync(string usuarioId)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<CategoriaResponse>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var categorias = await categoriaRepository.ListarPorCasamentoAsync(casamentoId.Value);

        return Resultado<List<CategoriaResponse>>.Ok(categorias.Select(Mapear).ToList());
    }

    public async Task<Resultado<CategoriaResponse>> CriarAsync(string usuarioId, CriarCategoriaRequest request)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<CategoriaResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var nome = request.Nome.Trim();
        if (await categoriaRepository.ExistePorNomeAsync(casamentoId.Value, nome))
            return Resultado<CategoriaResponse>.Falha(TipoErro.Conflito, NomeRepetido);

        var categoria = new Categoria { Nome = nome, CasamentoId = casamentoId.Value };

        categoriaRepository.Adicionar(categoria);
        await unitOfWork.SalvarAsync();

        return Resultado<CategoriaResponse>.Ok(Mapear(categoria));
    }

    public async Task<Resultado<CategoriaResponse>> AtualizarAsync(
        string usuarioId, int id, AtualizarCategoriaRequest request)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<CategoriaResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var categoria = await categoriaRepository.ObterPorCasamentoAsync(casamentoId.Value, id);
        if (categoria is null)
            return Resultado<CategoriaResponse>.Falha(TipoErro.NaoEncontrado, NaoEncontrada);

        var nome = request.Nome.Trim();
        if (await categoriaRepository.ExistePorNomeAsync(casamentoId.Value, nome, ignorarId: id))
            return Resultado<CategoriaResponse>.Falha(TipoErro.Conflito, NomeRepetido);

        categoria.Nome = nome;
        await unitOfWork.SalvarAsync();

        return Resultado<CategoriaResponse>.Ok(Mapear(categoria));
    }

    public async Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var categoria = await categoriaRepository.ObterPorCasamentoAsync(casamentoId.Value, id);
        if (categoria is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, NaoEncontrada);

        // O banco usa Restrict, então a regra é validada aqui para devolver uma mensagem clara
        if (await categoriaRepository.PossuiDespesasAsync(id))
            return Resultado<bool>.Falha(TipoErro.Conflito,
                "Não é possível excluir uma categoria que possui despesas.");

        categoriaRepository.Remover(categoria);
        await unitOfWork.SalvarAsync();

        return Resultado<bool>.Ok(true);
    }

    private static CategoriaResponse Mapear(Categoria c) => new() { Id = c.Id, Nome = c.Nome };
}