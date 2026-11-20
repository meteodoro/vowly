using Vowly.Api.Dtos.Parcelas;
using Vowly.Api.Enums;
using Vowly.Api.Models;
using Vowly.Api.Repositories.Casamentos;
using Vowly.Api.Repositories.Comum;
using Vowly.Api.Repositories.Despesas;
using Vowly.Api.Repositories.Parcelas;
using Vowly.Api.Services.Comum;

namespace Vowly.Api.Services.Parcelas;

public interface IParcelaService
{
    Task<Resultado<List<ParcelaResponse>>> ListarAsync(string usuarioId, int despesaId);
    Task<Resultado<List<ParcelaResponse>>> GerarAsync(string usuarioId, int despesaId, GerarParcelasRequest request);
    Task<Resultado<ParcelaResponse>> PagarAsync(string usuarioId, int parcelaId, DateOnly? dataPagamento);
    Task<Resultado<ParcelasPendentesResponse>> ListarPendentesAsync(string usuarioId, int dias);
}

public class ParcelaService(
    ICasamentoRepository casamentoRepository,
    IDespesaRepository despesaRepository,
    IParcelaRepository parcelaRepository,
    IUnitOfWork unitOfWork) : IParcelaService
{
    private const string SemCasamento = "Cadastre o casamento antes de usar as despesas.";
    private const string DespesaNaoEncontrada = "Despesa não encontrada.";

    public async Task<Resultado<List<ParcelaResponse>>> ListarAsync(string usuarioId, int despesaId)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<ParcelaResponse>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        if (!await despesaRepository.ExistePorCasamentoAsync(casamentoId.Value, despesaId))
            return Resultado<List<ParcelaResponse>>.Falha(TipoErro.NaoEncontrado, DespesaNaoEncontrada);

        var parcelas = await parcelaRepository.ListarPorDespesaAsync(despesaId);

        return Resultado<List<ParcelaResponse>>.Ok(parcelas.Select(Mapear).ToList());
    }

    public async Task<Resultado<List<ParcelaResponse>>> GerarAsync(
        string usuarioId, int despesaId, GerarParcelasRequest request)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<List<ParcelaResponse>>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var despesa = await despesaRepository.ObterPorCasamentoAsync(casamentoId.Value, despesaId);
        if (despesa is null)
            return Resultado<List<ParcelaResponse>>.Falha(TipoErro.NaoEncontrado, DespesaNaoEncontrada);

        if (await despesaRepository.PossuiParcelasAsync(despesaId))
            return Resultado<List<ParcelaResponse>>.Falha(TipoErro.Conflito, "Esta despesa já foi parcelada.");

        if (despesa.Status != StatusDespesa.APagar)
            return Resultado<List<ParcelaResponse>>.Falha(TipoErro.Conflito,
                "Só é possível parcelar uma despesa com status 'a pagar'.");

        if (despesa.ValorTotal < request.Quantidade * 0.01m)
            return Resultado<List<ParcelaResponse>>.Falha(TipoErro.Invalido,
                "O valor da despesa é pequeno demais para essa quantidade de parcelas.");

        var calculadas = ParcelasCalculadora.Gerar(
            despesa.ValorTotal, request.Quantidade, request.PrimeiroVencimento);

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

        parcelaRepository.AdicionarVarias(parcelas);
        despesa.Status = StatusDespesa.EmPagamento;

        // Parcelas e status da despesa são gravados juntos, na mesma transação
        await unitOfWork.SalvarAsync();

        return Resultado<List<ParcelaResponse>>.Ok(parcelas.Select(Mapear).ToList());
    }

    public async Task<Resultado<ParcelaResponse>> PagarAsync(
        string usuarioId, int parcelaId, DateOnly? dataPagamento)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<ParcelaResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var parcela = await parcelaRepository.ObterPorCasamentoAsync(casamentoId.Value, parcelaId);
        if (parcela is null)
            return Resultado<ParcelaResponse>.Falha(TipoErro.NaoEncontrado, "Parcela não encontrada.");

        if (parcela.Paga)
            return Resultado<ParcelaResponse>.Falha(TipoErro.Conflito, "Esta parcela já foi paga.");

        var hoje = DataLocal.Hoje();

        // Um dia de tolerância, por causa do fuso horário do usuário
        if (dataPagamento is not null && dataPagamento > hoje.AddDays(1))
            return Resultado<ParcelaResponse>.Falha(TipoErro.Invalido,
                "A data de pagamento não pode estar no futuro.");

        parcela.Paga = true;
        parcela.DataPagamento = dataPagamento ?? hoje;

        // Se não sobrou nenhuma parcela em aberto (além desta), a despesa fica paga
        var restantes = await parcelaRepository.ContarEmAbertoAsync(parcela.DespesaId, parcela.Id);
        if (restantes == 0)
            parcela.Despesa.Status = StatusDespesa.Paga;

        // Parcela e despesa são gravadas juntas, na mesma transação
        await unitOfWork.SalvarAsync();

        return Resultado<ParcelaResponse>.Ok(Mapear(parcela));
    }

    public async Task<Resultado<ParcelasPendentesResponse>> ListarPendentesAsync(string usuarioId, int dias)
    {
        var casamentoId = await casamentoRepository.ObterIdPorUsuarioAsync(usuarioId);
        if (casamentoId is null)
            return Resultado<ParcelasPendentesResponse>.Falha(TipoErro.NaoEncontrado, SemCasamento);

        var hoje = DataLocal.Hoje();

        var pendentes = await parcelaRepository.ListarPendentesAsync(casamentoId.Value, hoje.AddDays(dias));

        var itens = pendentes.Select(p => MapearPendente(p, hoje)).ToList();
        var atrasadas = itens.Where(i => i.Situacao == SituacaoParcela.Atrasada).ToList();
        var aVencer = itens.Where(i => i.Situacao == SituacaoParcela.AVencer).ToList();

        return Resultado<ParcelasPendentesResponse>.Ok(new ParcelasPendentesResponse
        {
            Hoje = hoje,
            JanelaDias = dias,
            Atrasadas = atrasadas,
            AVencer = aVencer,
            TotalAtrasado = atrasadas.Sum(i => i.Valor),
            TotalAVencer = aVencer.Sum(i => i.Valor)
        });
    }

    private static ParcelaResponse Mapear(Parcela p) => new()
    {
        Id = p.Id,
        DespesaId = p.DespesaId,
        Numero = p.Numero,
        Valor = p.Valor,
        DataVencimento = p.DataVencimento,
        DataPagamento = p.DataPagamento,
        Paga = p.Paga
    };

    private static ParcelaPendenteResponse MapearPendente(ParcelaPendenteConsulta p, DateOnly hoje) => new()
    {
        ParcelaId = p.ParcelaId,
        DespesaId = p.DespesaId,
        DespesaDescricao = p.DespesaDescricao,
        CategoriaNome = p.CategoriaNome,
        Numero = p.Numero,
        TotalParcelas = p.TotalParcelas,
        Valor = p.Valor,
        DataVencimento = p.DataVencimento,
        Situacao = ParcelaRegras.Classificar(p.DataVencimento, hoje),
        DiasParaVencer = ParcelaRegras.DiasParaVencer(p.DataVencimento, hoje)
    };
}