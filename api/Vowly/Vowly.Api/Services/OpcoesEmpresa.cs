namespace Vowly.Api.Services;

public class ItemCusto
{
    public string Nome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

public class OpcoesEmpresa
{
    public const string Secao = "Empresa";

    public List<ItemCusto> CustosFixos { get; set; } = [];

    public decimal CustosFixosMensais => CustosFixos.Sum(c => c.Valor);
}