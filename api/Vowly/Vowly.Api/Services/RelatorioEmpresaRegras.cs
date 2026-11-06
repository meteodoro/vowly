namespace Vowly.Api.Services;

public static class RelatorioEmpresaRegras
{
    // RN02: lucro do mês = receita - custos (negativo significa prejuízo)
    public static decimal Lucro(decimal receita, decimal custos) => receita - custos;

    // RN03: custos fixos ÷ preço do Premium, arredondado para cima
    public static int PontoEquilibrio(decimal custosFixosMensais, decimal precoPremium)
    {
        if (precoPremium <= 0)
            throw new ArgumentOutOfRangeException(nameof(precoPremium), "O preço do Premium deve ser maior que zero.");

        if (custosFixosMensais <= 0)
            return 0;

        return (int)Math.Ceiling(custosFixosMensais / precoPremium);
    }

    public static int AssinantesFaltantes(int pontoEquilibrio, int assinantesVigentes) =>
        Math.Max(0, pontoEquilibrio - assinantesVigentes);
}