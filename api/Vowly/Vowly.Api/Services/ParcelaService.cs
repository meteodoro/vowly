using Microsoft.EntityFrameworkCore;
using Vowly.Api.Data;
using Vowly.Api.Dtos;
using Vowly.Api.Enums;
using Vowly.Api.Models;

namespace Vowly.Api.Services;

public interface IParcelaService
{
    Task<Resultado<List<ParcelaRespostaDto>>> ListarAsync(string usuarioId, int despesaId);
    Task<Resultado<List<ParcelaRespostaDto>>> GerarAsync(string usuarioId, int despesaId, GerarParcelasDto dto);
    Task<Resultado<ParcelaRespostaDto>> PagarAsync(string usuarioId, int parcelaId, DateOnly? dataPagamento);
    Task<Resultado<ParcelasPendentesRespostaDto>> ListarPendentesAsync(string usuarioId, int dias);
}

public class ParcelaService(AppDbContext context) : IParcelaService
{
    private const string SemCasamento = "Cadastre o casamento antes de usar as despesas.";
    private const string DespesaNaoEncontrada = "Despesa não encontrada.";

    public async Task<Resultado<List<ParcelaRespostaDto>>> ListarAsync(string usuarioId, int despesaId)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesaExiste = await context.Despesas
            .AnyAsync(d => d.Id == despesaId && d.CasamentoId == casamentoId);
        if (!despesaExiste)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.NaoEncontrado, DespesaNaoEncontrada);

        var parcelas = await context.Parcelas
            .AsNoTracking()
            .Where(p => p.DespesaId == despesaId)
            .OrderBy(p => p.Numero)
            .Select(p => new ParcelaRespostaDto
            {
                Id = p.Id,
                DespesaId = p.DespesaId,
                Numero = p.Numero,
                Valor = p.Valor,
                DataVencimento = p.DataVencimento,
                DataPagamento = p.DataPagamento,
                Paga = p.Paga
            })
            .ToListAsync();

        return Resultado<List<ParcelaRespostaDto>>.Ok(parcelas);
    }

    public async Task<Resultado<List<ParcelaRespostaDto>>> GerarAsync(
        string usuarioId, int despesaId, GerarParcelasDto dto)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesa = await context.Despesas
            .FirstOrDefaultAsync(d => d.Id == despesaId && d.CasamentoId == casamentoId);
        if (despesa is null)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.NaoEncontrado, DespesaNaoEncontrada);

        var jaParcelada = await context.Parcelas.AnyAsync(p => p.DespesaId == despesaId);
        if (jaParcelada)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.Conflito, "Esta despesa já foi parcelada.");

        if (despesa.Status != StatusDespesa.APagar)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.Conflito,
                "Só é possível parcelar uma despesa com status 'a pagar'.");

        if (despesa.ValorTotal < dto.Quantidade * 0.01m)
            return Resultado<List<ParcelaRespostaDto>>.Falha(TipoErro.Invalido,"O valor da despesa é pequeno demais para essa quantidade de parcelas.");
        
        var calculadas = ParcelasCalculadora.Gerar(despesa.ValorTotal, dto.Quantidade, dto.PrimeiroVencimento);

        var parcelas = calculadas
            .Select(p => new Parcela
            {
                DespesaId = despesa.Id,
                Numero = p.Numero,
                Valor = p.Valor,
                DataVencimento = p.DataVencimento,
                Paga = false
            })
            .ToList();

        context.Parcelas.AddRange(parcelas);
        despesa.Status = StatusDespesa.EmPagamento;

        await context.SaveChangesAsync();

        return Resultado<List<ParcelaRespostaDto>>.Ok(parcelas.Select(Mapear).ToList());
    }

    private Task<int?> ObterCasamentoIdAsync(string usuarioId) =>
        context.Casamentos
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();

    private static ParcelaRespostaDto Mapear(Parcela p) => new()
    {
        Id = p.Id,
        DespesaId = p.DespesaId,
        Numero = p.Numero,
        Valor = p.Valor,
        DataVencimento = p.DataVencimento,
        DataPagamento = p.DataPagamento,
        Paga = p.Paga
    };
    
    public async Task<Resultado<ParcelaRespostaDto>> PagarAsync(string usuarioId, int parcelaId, DateOnly? dataPagamento)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<ParcelaRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var parcela = await context.Parcelas
            .Include(p => p.Despesa)
            .FirstOrDefaultAsync(p => p.Id == parcelaId && p.Despesa.CasamentoId == casamentoId);
        if (parcela is null)
            return Resultado<ParcelaRespostaDto>.Falha(TipoErro.NaoEncontrado, "Parcela não encontrada.");

        if (parcela.Paga)
            return Resultado<ParcelaRespostaDto>.Falha(TipoErro.Conflito, "Esta parcela já foi paga.");

        var hoje = DataLocal.Hoje();
        
        // Um dia de tolerância, por causa do fuso horário do usuário
        if (dataPagamento is not null && dataPagamento > hoje.AddDays(1))
            return Resultado<ParcelaRespostaDto>.Falha(TipoErro.Invalido,
                "A data de pagamento não pode estar no futuro.");

        parcela.Paga = true;
        parcela.DataPagamento = dataPagamento ?? hoje;

        // Se não sobrou nenhuma parcela em aberto (além desta), a despesa fica paga
        var restantes = await context.Parcelas
            .CountAsync(p => p.DespesaId == parcela.DespesaId && !p.Paga && p.Id != parcela.Id);

        if (restantes == 0)
            parcela.Despesa.Status = StatusDespesa.Paga;

        await context.SaveChangesAsync();

        return Resultado<ParcelaRespostaDto>.Ok(Mapear(parcela));
    }
    
    public async Task<Resultado<ParcelasPendentesRespostaDto>> ListarPendentesAsync(string usuarioId, int dias)
    {
        var casamentoId = await ObterCasamentoIdAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<ParcelasPendentesRespostaDto>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var hoje = DataLocal.Hoje();
        var limite = hoje.AddDays(dias);

        // Traz todas as atrasadas (vencimento antes de hoje) e as que vencem dentro da janela
        var pendentes = await context.Parcelas
            .AsNoTracking()
            .Where(p => p.Despesa.CasamentoId == casamentoId && !p.Paga && p.DataVencimento <= limite)
            .OrderBy(p => p.DataVencimento).ThenBy(p => p.DespesaId).ThenBy(p => p.Numero)
            .Select(p => new
            {
                p.Id,
                p.DespesaId,
                Descricao = p.Despesa.Descricao,
                Categoria = p.Despesa.Categoria.Nome,
                p.Numero,
                Total = p.Despesa.Parcelas.Count,
                p.Valor,
                p.DataVencimento
            })
            .ToListAsync();

        var itens = pendentes
            .Select(p => new ParcelaPendenteDto
            {
                ParcelaId = p.Id,
                DespesaId = p.DespesaId,
                DespesaDescricao = p.Descricao,
                CategoriaNome = p.Categoria,
                Numero = p.Numero,
                TotalParcelas = p.Total,
                Valor = p.Valor,
                DataVencimento = p.DataVencimento,
                Situacao = ParcelaRegras.Classificar(p.DataVencimento, hoje),
                DiasParaVencer = ParcelaRegras.DiasParaVencer(p.DataVencimento, hoje)
            })
            .ToList();

        var atrasadas = itens.Where(i => i.Situacao == SituacaoParcela.Atrasada).ToList();
        var aVencer = itens.Where(i => i.Situacao == SituacaoParcela.AVencer).ToList();

        return Resultado<ParcelasPendentesRespostaDto>.Ok(new ParcelasPendentesRespostaDto
        {
            Hoje = hoje,
            JanelaDias = dias,
            Atrasadas = atrasadas,
            AVencer = aVencer,
            TotalAtrasado = atrasadas.Sum(i => i.Valor),
            TotalAVencer = aVencer.Sum(i => i.Valor)
        });
    }
}