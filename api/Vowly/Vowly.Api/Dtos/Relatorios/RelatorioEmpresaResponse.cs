namespace Vowly.Api.Dtos.Relatorios;

public class RelatorioEmpresaResponse
{
    public decimal PrecoPremium { get; set; }
    public decimal CustosFixosMensais { get; set; }
    public List<ItemCustoResponse> Custos { get; set; } = [];
    public int PontoEquilibrio { get; set; }
    public int AssinantesPremiumVigentes { get; set; }
    public int FaltamParaEquilibrio { get; set; }
    public List<RelatorioMesResponse> Meses { get; set; } = [];
}