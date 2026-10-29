using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;
using Vowly.Api.Models;

namespace Vowly.Api.Services;

public interface ICategoriaService
{
    Task<Resultado<List<CategoriaRespostaDto>>> ListarAsync(string usuarioId);
    Task<Resultado<CategoriaRespostaDto>> CriarAsync(string usuarioId, SalvarCategoriaDto dto);
    Task<Resultado<CategoriaRespostaDto>> AtualizarAsync(string usuarioId, int id, SalvarCategoriaDto dto);
    Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id);
}

public class CategoriaService(AppDbContext context) : ICategoriaService
{
    private const string SemCasamento = "Cadastre o casamento antes de usar as categorias.";

    public async Task<Resultado<List<CategoriaRespostaDto>>> ListarAsync(string usuarioId)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<CategoriaRespostaDto>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var lista = await context.Categorias
            .AsNoTracking()
            .Where(c => c.CasamentoId == casamentoId)
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaRespostaDto { Id = c.Id, Nome = c.Nome })
            .ToListAsync();

        return Resultado<List<CategoriaRespostaDto>>.Ok(lista);
    }

    public async Task<Resultado<CategoriaRespostaDto>> CriarAsync(string usuarioId, SalvarCategoriaDto dto)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<CategoriaRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var nome = dto.Nome.Trim();
        if (await NomeJaExisteAsync(casamentoId.Value, nome, null))
            return Resultado<CategoriaRespostaDto>.Falha(TipoErro.Conflito, "Já existe uma categoria com esse nome.");

        var categoria = new Categoria { Nome = nome, CasamentoId = casamentoId.Value };
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();

        return Resultado<CategoriaRespostaDto>.Ok(Mapear(categoria));
    }

    public async Task<Resultado<CategoriaRespostaDto>> AtualizarAsync(string usuarioId, int id, SalvarCategoriaDto dto)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<CategoriaRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var categoria = await context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.CasamentoId == casamentoId);
        if (categoria is null)
            return Resultado<CategoriaRespostaDto>.Falha(TipoErro.NaoEncontrado, "Categoria não encontrada.");

        var nome = dto.Nome.Trim();
        if (await NomeJaExisteAsync(casamentoId.Value, nome, id))
            return Resultado<CategoriaRespostaDto>.Falha(TipoErro.Conflito, "Já existe uma categoria com esse nome.");

        categoria.Nome = nome;
        await context.SaveChangesAsync();

        return Resultado<CategoriaRespostaDto>.Ok(Mapear(categoria));
    }

    public async Task<Resultado<bool>> ExcluirAsync(string usuarioId, int id)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var categoria = await context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.CasamentoId == casamentoId);
        if (categoria is null)
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, "Categoria não encontrada.");

        var temDespesas = await context.Despesas.AnyAsync(d => d.CategoriaId == id);
        if (temDespesas)
            return Resultado<bool>.Falha(TipoErro.Conflito, "Não é possível excluir uma categoria que possui despesas.");

        context.Categorias.Remove(categoria);
        await context.SaveChangesAsync();

        return Resultado<bool>.Ok(true);
    }

    private Task<int?> ObterCasamentoIdAsync(string usuarioId) =>
        context.Casamentos
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();

    private Task<bool> NomeJaExisteAsync(int casamentoId, string nome, int? ignorarId) =>
        context.Categorias.AnyAsync(c =>
            c.CasamentoId == casamentoId &&
            c.Id != ignorarId &&
            c.Nome.ToLower() == nome.ToLower());

    private static CategoriaRespostaDto Mapear(Categoria c) => new() { Id = c.Id, Nome = c.Nome };
}