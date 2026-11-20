using Vowly.Api.Services.Relatorios;

namespace Vowly.Api.Tests;

public class RelatorioEmpresaRegrasTests
{
    private const decimal Preco = 19.90m;
    private const decimal Custos = 120m;

    // RN02

    [Fact]
    public void Lucro_DezAssinantes_EhR79()
    {
        var receita = 10 * Preco; // 199,00

        Assert.Equal(79.00m, RelatorioEmpresaRegras.Lucro(receita, Custos));
    }

    [Fact]
    public void Lucro_SeisAssinantes_DaPrejuizo()
    {
        var receita = 6 * Preco; // 119,40

        Assert.Equal(-0.60m, RelatorioEmpresaRegras.Lucro(receita, Custos));
    }

    [Fact]
    public void Lucro_SeteAssinantes_CobreOsCustos()
    {
        var receita = 7 * Preco; // 139,30

        Assert.Equal(19.30m, RelatorioEmpresaRegras.Lucro(receita, Custos));
    }

    [Fact]
    public void Lucro_SemAssinantes_EhOPrejuizoDosCustos()
    {
        Assert.Equal(-120m, RelatorioEmpresaRegras.Lucro(0m, Custos));
    }

    // RN03

    [Theory]
    [InlineData(12000, 1990, 7)] // R$ 120,00 ÷ R$ 19,90 = 6,03, arredonda para 7 (exemplo da Ficha)
    [InlineData(10000, 2000, 5)] // divisão exata não arredonda: 100 ÷ 20 = 5
    [InlineData(1990, 1990, 1)]  // custo igual ao preço
    [InlineData(1, 1990, 1)]     // qualquer custo exige pelo menos 1 assinante
    [InlineData(0, 1990, 0)]     // sem custos
    public void PontoEquilibrio_ArredondaParaCima(int custosEmCentavos, int precoEmCentavos, int esperado)
    {
        var resultado = RelatorioEmpresaRegras.PontoEquilibrio(custosEmCentavos / 100m, precoEmCentavos / 100m);

        Assert.Equal(esperado, resultado);
    }

    [Theory]
    [InlineData(12000, 1990)]
    [InlineData(10000, 2000)]
    [InlineData(25050, 1990)]
    public void PontoEquilibrio_ComEsseNumeroDeAssinantes_NaoHaPrejuizo_ComUmAMenosHa(
        int custosEmCentavos, int precoEmCentavos)
    {
        var custos = custosEmCentavos / 100m;
        var preco = precoEmCentavos / 100m;

        var ponto = RelatorioEmpresaRegras.PontoEquilibrio(custos, preco);

        Assert.True(RelatorioEmpresaRegras.Lucro(ponto * preco, custos) >= 0);
        Assert.True(RelatorioEmpresaRegras.Lucro((ponto - 1) * preco, custos) < 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1990)]
    public void PontoEquilibrio_PrecoZeroOuNegativo_LancaExcecao(int precoEmCentavos)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RelatorioEmpresaRegras.PontoEquilibrio(120m, precoEmCentavos / 100m));
    }

    [Theory]
    [InlineData(7, 3, 4)]
    [InlineData(7, 7, 0)]
    [InlineData(7, 10, 0)]
    public void AssinantesFaltantes_NuncaFicaNegativo(int ponto, int vigentes, int esperado)
    {
        Assert.Equal(esperado, RelatorioEmpresaRegras.AssinantesFaltantes(ponto, vigentes));
    }
}