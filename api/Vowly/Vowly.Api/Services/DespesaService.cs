using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;
using Vowly.Api.Models;

namespace Vowly.Api.Services;

public interface IDespesaService
{
    Task<Resultado<List<DespesaRespostaDto>>> ListarAsync(string usuarioId, int? categoriaId, StatusDespesa? status);
    Task<Resultado<DespesaRespostaDto>> ObterAsync(string usuarioId, int id);
    Task<Resultado<DespesaRespostaDto>> CriarAsync(string usuarioId, SalvarDespesaDto dto);
    Task<Resultado<DespesaRespostaDto>> AtualizarAsync(string usuarioId, int id, SalvarDespesaDto dto);
    Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id);
}

public class DespesaService(AppDbContext context) : IDespesaService
{
    private const string SemCasamento = "Cadastre o casamento antes de usar as despesas.";
    private const string StatusAutomatico = "O status 'em pagamento' é definido automaticamente quando a despesa é parcelada.";

    private static readonly Expression<Func<Despesa, DespesaRespostaDto>> ParaDto = d => new DespesaRespostaDto
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

    public async Task<Resultado<List<DespesaRespostaDto>>> ListarAsync(
        string usuarioId, int? categoriaId, StatusDespesa? status)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<DespesaRespostaDto>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var consulta = context.Despesas
            .AsNoTracking()
            .Where(d => d.CasamentoId == casamentoId);

        if (categoriaId is not null)
            consulta = consulta.Where(d => d.CategoriaId == categoriaId);

        if (status is not null)
            consulta = consulta.Where(d => d.Status == status);

        var lista = await consulta
            .OrderByDescending(d => d.Id)
            .Select(ParaDto)
            .ToListAsync();

        return Resultado<List<DespesaRespostaDto>>.Ok(lista);
    }

    public async Task<Resultado<DespesaRespostaDto>> ObterAsync(string usuarioId, int id)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<DespesaRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesa = await BuscarDtoAsync(casamentoId.Value, id);
        return despesa is null
            ? Resultado<DespesaRespostaDto>.Falha(TipoErro.NaoEncontrado, "Despesa não encontrada.")
            : Resultado<DespesaRespostaDto>.Ok(despesa);
    }

    public async Task<Resultado<DespesaRespostaDto>> CriarAsync(string usuarioId, SalvarDespesaDto dto)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<DespesaRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        if (dto.Status == StatusDespesa.EmPagamento)
            return Resultado<DespesaRespostaDto>.Falha(TipoErro.Invalido, StatusAutomatico);

        var erroReferencia = await ValidarReferenciasAsync(casamentoId.Value, dto);
        if (erroReferencia is not null)
            return Resultado<DespesaRespostaDto>.Falha(TipoErro.Invalido, erroReferencia);

        // RN01 (limite de 20 despesas do plano gratuito) entra aqui, na tarefa 8.

        var despesa = new Despesa
        {
            CasamentoId = casamentoId.Value,
            Descricao = dto.Descricao.Trim(),
            ValorTotal = dto.ValorTotal,
            Observacoes = dto.Observacoes,
            CategoriaId = dto.CategoriaId,
            FornecedorId = dto.FornecedorId,
            Status = dto.Status
        };

        context.Despesas.Add(despesa);
        await context.SaveChangesAsync();

        var resposta = await BuscarDtoAsync(casamentoId.Value, despesa.Id);
        return Resultado<DespesaRespostaDto>.Ok(resposta!);
    }

    public async Task<Resultado<DespesaRespostaDto>> AtualizarAsync(string usuarioId, int id, SalvarDespesaDto dto)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<DespesaRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesa = await context.Despesas
            .FirstOrDefaultAsync(d => d.Id == id && d.CasamentoId == casamentoId);
        if (despesa is null)
            return Resultado<DespesaRespostaDto>.Falha(TipoErro.NaoEncontrado, "Despesa não encontrada.");

        var erroReferencia = await ValidarReferenciasAsync(casamentoId.Value, dto);
        if (erroReferencia is not null)
            return Resultado<DespesaRespostaDto>.Falha(TipoErro.Invalido, erroReferencia);

        var temParcelas = await context.Parcelas.AnyAsync(p => p.DespesaId == id);

        if (temParcelas)
        {
            // Com parcelas, o valor e o status dependem delas e não podem ser editados à mão
            if (dto.ValorTotal != despesa.ValorTotal)
                return Resultado<DespesaRespostaDto>.Falha(TipoErro.Conflito,
                    "Não é possível alterar o valor de uma despesa parcelada.");
        }
        else
        {
            if (dto.Status == StatusDespesa.EmPagamento)
                return Resultado<DespesaRespostaDto>.Falha(TipoErro.Invalido, StatusAutomatico);

            despesa.ValorTotal = dto.ValorTotal;
            despesa.Status = dto.Status;
        }

        despesa.Descricao = dto.Descricao.Trim();
        despesa.Observacoes = dto.Observacoes;
        despesa.CategoriaId = dto.CategoriaId;
        despesa.FornecedorId = dto.FornecedorId;

        await context.SaveChangesAsync();

        var resposta = await BuscarDtoAsync(casamentoId.Value, id);
        return Resultado<DespesaRespostaDto>.Ok(resposta!);
    }

    public async Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesa = await context.Despesas
            .FirstOrDefaultAsync(d => d.Id == id && d.CasamentoId == casamentoId);
        if (despesa is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, "Despesa não encontrada.");

        // As parcelas são apagadas junto (cascata configurada no AppDbContext)
        context.Despesas.Remove(despesa);
        await context.SaveChangesAsync();

        return Resultado<bool>.Ok(true);
    }

    private Task<int?> ObterCasamentoIdAsync(string usuarioId) =>
        context.Casamentos
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();

    private Task<DespesaRespostaDto?> BuscarDtoAsync(int casamentoId, int id) =>
        context.Despesas
            .AsNoTracking()
            .Where(d => d.Id == id && d.CasamentoId == casamentoId)
            .Select(ParaDto)
            .FirstOrDefaultAsync();

    // Devolve a mensagem de erro, ou null se categoria e fornecedor forem do casamento
    private async Task<string?> ValidarReferenciasAsync(int casamentoId, SalvarDespesaDto dto)
    {
        var categoriaValida = await context.Categorias
            .AnyAsync(c => c.Id == dto.CategoriaId && c.CasamentoId == casamentoId);
        if (!categoriaValida)
            return "Categoria inválida.";

        if (dto.FornecedorId is not null)
        {
            var fornecedorValido = await context.Fornecedores
                .AnyAsync(f => f.Id == dto.FornecedorId && f.CasamentoId == casamentoId);
            if (!fornecedorValido)
                return "Fornecedor inválido.";
        }

        return null;
    }
}