using Microsoft.Extensions.Options;
using Vowly.Api.Dtos.Despesas;
using Vowly.Api.Enums;
using Vowly.Api.Models;
using Vowly.Api.Repositories.Assinaturas;
using Vowly.Api.Repositories.Casamentos;
using Vowly.Api.Repositories.Categorias;
using Vowly.Api.Repositories.Comum;
using Vowly.Api.Repositories.Despesas;
using Vowly.Api.Repositories.Fornecedores;
using Vowly.Api.Services.Assinaturas;

namespace Vowly.Api.Services.Despesas;

public interface IDespesaService
{
    Task<Resultado<List<DespesaResponse>>> ListarAsync(string usuarioId, int? categoriaId, StatusDespesa? status);
    Task<Resultado<DespesaResponse>> ObterAsync(string usuarioId, int id);
    Task<Resultado<DespesaResponse>> CriarAsync(string usuarioId, CriarDespesaRequest request);
    Task<Resultado<DespesaResponse>> AtualizarAsync(string usuarioId, int id, AtualizarDespesaRequest request);
    Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id);
}

public class DespesaService(
    ICasamentoRepository casamentoRepository,
    ICategoriaRepository categoriaRepository,
    IFornecedorRepository fornecedorRepository,
    IAssinaturaRepository assinaturaRepository,
    IDespesaRepository despesaRepository,
    IUnitOfWork unitOfWork,
    IOptions<OpcoesPlano> opcoesPlano) : IDespesaService
{
    private const string SemCasamento = "Cadastre o casamento antes de usar as despesas.";
    private const string NaoEncontrada = "Despesa não encontrada.";
    private const string StatusAutomatico =
        "O status 'em pagamento' é definido automaticamente quando a despesa é parcelada.";

    public async Task<Resultado<List<DespesaResponse>>> ListarAsync(
        string usuarioId, int? categoriaId, StatusDespesa? status)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<DespesaResponse>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesas = await despesaRepository.ListarPorCasamentoAsync(casamentoId.Value, categoriaId, status);

        return Resultado<List<DespesaResponse>>.Ok(despesas.Select(Mapear).ToList());
    }

    public async Task<Resultado<DespesaResponse>> ObterAsync(string usuarioId, int id)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<DespesaResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesa = await despesaRepository.ObterDetalhadaAsync(casamentoId.Value, id);

        return despesa is null
            ? Resultado<DespesaResponse>.Falha(TipoErro.NaoEncontrado, NaoEncontrada)
            : Resultado<DespesaResponse>.Ok(Mapear(despesa));
    }

    public async Task<Resultado<DespesaResponse>> CriarAsync(string usuarioId, CriarDespesaRequest request)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<DespesaResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        if (request.Status == StatusDespesa.EmPagamento)
            return Resultado<DespesaResponse>.Falha(TipoErro.Invalido, StatusAutomatico);

        var erroReferencia = await ValidarReferenciasAsync(
            casamentoId.Value, request.CategoriaId, request.FornecedorId);
        if (erroReferencia is not null)
            return Resultado<DespesaResponse>.Falha(TipoErro.Invalido, erroReferencia);

        // RN01: limite de despesas do plano gratuito
        var limite = opcoesPlano.Value.LimiteDespesasGratuito;
        var assinatura = await assinaturaRepository.ObterPorUsuarioAsync(usuarioId, somenteLeitura: true);
        var quantidadeAtual = await despesaRepository.ContarPorCasamentoAsync(casamentoId.Value);

        if (!PlanoRegras.PodeCadastrarDespesa(assinatura, quantidadeAtual, DateTime.UtcNow, limite))
            return Resultado<DespesaResponse>.Falha(TipoErro.Conflito,
                $"O plano gratuito permite até {limite} despesas. Assine o Premium para cadastrar mais.");

        var despesa = new Despesa
        {
            CasamentoId = casamentoId.Value,
            Descricao = request.Descricao.Trim(),
            ValorTotal = request.ValorTotal,
            Observacoes = request.Observacoes,
            CategoriaId = request.CategoriaId,
            FornecedorId = request.FornecedorId,
            Status = request.Status
        };

        despesaRepository.Adicionar(despesa);
        await unitOfWork.SalvarAsync();

        return await ObterAsync(usuarioId, despesa.Id);
    }

    public async Task<Resultado<DespesaResponse>> AtualizarAsync(
        string usuarioId, int id, AtualizarDespesaRequest request)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<DespesaResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesa = await despesaRepository.ObterPorCasamentoAsync(casamentoId.Value, id);
        if (despesa is null)
            return Resultado<DespesaResponse>.Falha(TipoErro.NaoEncontrado, NaoEncontrada);

        var erroReferencia = await ValidarReferenciasAsync(
            casamentoId.Value, request.CategoriaId, request.FornecedorId);
        if (erroReferencia is not null)
            return Resultado<DespesaResponse>.Falha(TipoErro.Invalido, erroReferencia);

        if (await despesaRepository.PossuiParcelasAsync(id))
        {
            // Com parcelas, o valor e o status dependem delas e não podem ser editados à mão
            if (request.ValorTotal != despesa.ValorTotal)
                return Resultado<DespesaResponse>.Falha(TipoErro.Conflito,
                    "Não é possível alterar o valor de uma despesa parcelada.");
        }
        else
        {
            if (request.Status == StatusDespesa.EmPagamento)
                return Resultado<DespesaResponse>.Falha(TipoErro.Invalido, StatusAutomatico);

            despesa.ValorTotal = request.ValorTotal;
            despesa.Status = request.Status;
        }

        despesa.Descricao = request.Descricao.Trim();
        despesa.Observacoes = request.Observacoes;
        despesa.CategoriaId = request.CategoriaId;
        despesa.FornecedorId = request.FornecedorId;

        await unitOfWork.SalvarAsync();

        return await ObterAsync(usuarioId, id);
    }

    public async Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesa = await despesaRepository.ObterPorCasamentoAsync(casamentoId.Value, id);
        if (despesa is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, NaoEncontrada);

        // As parcelas são apagadas junto (cascata configurada no AppDbContext)
        despesaRepository.Remover(despesa);
        await unitOfWork.SalvarAsync();

        return Resultado<bool>.Ok(true);
    }

    // Devolve a mensagem de erro, ou null se categoria e fornecedor forem do casamento
    private async Task<string?> ValidarReferenciasAsync(int casamentoId, int categoriaId, int? fornecedorId)
    {
        if (!await categoriaRepository.ExistePorCasamentoAsync(casamentoId, categoriaId))
            return "Categoria inválida.";

        if (fornecedorId is not null
            && !await fornecedorRepository.ExistePorCasamentoAsync(casamentoId, fornecedorId.Value))
            return "Fornecedor inválido.";

        return null;
    }

    // Espera a despesa com categoria e parcelas carregadas
    private static DespesaResponse Mapear(Despesa d) => new()
    {
        Id = d.Id,
        Descricao = d.Descricao,
        ValorTotal = d.ValorTotal,
        Observacoes = d.Observacoes,
        Status = d.Status,
        CategoriaId = d.CategoriaId,
        CategoriaNome = d.Categoria.Nome,
        FornecedorId = d.FornecedorId,
        QuantidadeParcelas = d.Parcelas.Count
    };
}