using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Dtos;
using Vowly.Api.Models;

namespace Vowly.Api.Services;

public interface ICasamentoService
{
    Task<CasamentoRespostaDto?> ObterAsync(string usuarioId);
    Task<CasamentoRespostaDto?> CriarAsync(string usuarioId, SalvarCasamentoDto dto);
    Task<CasamentoRespostaDto?> AtualizarAsync(string usuarioId, SalvarCasamentoDto dto);
}

public class CasamentoService(AppDbContext context) : ICasamentoService
{
    private static readonly string[] CategoriasPadrao =
        ["Buffet", "Vestido", "Decoração", "Fotografia", "Música", "Local"];
    
    public async Task<CasamentoRespostaDto?> ObterAsync(string usuarioId)
    {
        var casamento = await context.Casamentos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        return casamento is null ? null : Mapear(casamento);
    }

    // Retorna null se o usuário já tem um casamento
    public async Task<CasamentoRespostaDto?> CriarAsync(string usuarioId, SalvarCasamentoDto dto)
    {
        var jaExiste = await context.Casamentos.AnyAsync(c => c.UsuarioId == usuarioId);
        if (jaExiste)
            return null;

        var casamento = new Casamento
        {
            UsuarioId = usuarioId,
            NomeNoivo1 = dto.NomeNoivo1,
            NomeNoivo2 = dto.NomeNoivo2,
            DataPrevista = dto.DataPrevista,
            Cidade = dto.Cidade,
            OrcamentoTotal = dto.OrcamentoTotal
        };

        foreach (var nome in CategoriasPadrao)
            casamento.Categorias.Add(new Categoria { Nome = nome });
        
        context.Casamentos.Add(casamento);
        await context.SaveChangesAsync();

        return Mapear(casamento);
    }

    // Retorna null se o usuário ainda não tem casamento
    public async Task<CasamentoRespostaDto?> AtualizarAsync(string usuarioId, SalvarCasamentoDto dto)
    {
        var casamento = await context.Casamentos
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (casamento is null)
            return null;

        casamento.NomeNoivo1 = dto.NomeNoivo1;
        casamento.NomeNoivo2 = dto.NomeNoivo2;
        casamento.DataPrevista = dto.DataPrevista;
        casamento.Cidade = dto.Cidade;
        casamento.OrcamentoTotal = dto.OrcamentoTotal;

        await context.SaveChangesAsync();

        return Mapear(casamento);
    }

    private static CasamentoRespostaDto Mapear(Casamento c) => new()
    {
        Id = c.Id,
        NomeNoivo1 = c.NomeNoivo1,
        NomeNoivo2 = c.NomeNoivo2,
        DataPrevista = c.DataPrevista,
        Cidade = c.Cidade,
        OrcamentoTotal = c.OrcamentoTotal
    };
}