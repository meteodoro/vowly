namespace Vowly.Api.Services;

public record ParcelaCalculada(int Numero, decimal Valor, DateOnly DataVencimento);

public static class ParcelasCalculadora
{
    public static List<ParcelaCalculada> Gerar(decimal valorTotal, int quantidade, DateOnly primeiroVencimento)
    {
        if (valorTotal <= 0)
            throw new ArgumentOutOfRangeException(nameof(valorTotal), "O valor total deve ser maior que zero.");

        if (quantidade < 1)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade de parcelas deve ser pelo menos 1.");

        // Trunca nos centavos, e a última parcela absorve a diferença
        var valorBase = Math.Round(valorTotal / quantidade, 2, MidpointRounding.ToZero);

        var parcelas = new List<ParcelaCalculada>(quantidade);

        for (var numero = 1; numero <= quantidade; numero++)
        {
            var valor = numero == quantidade
                ? valorTotal - valorBase * (quantidade - 1)
                : valorBase;

            parcelas.Add(new ParcelaCalculada(numero, valor, primeiroVencimento.AddMonths(numero - 1)));
        }

        return parcelas;
    }
}