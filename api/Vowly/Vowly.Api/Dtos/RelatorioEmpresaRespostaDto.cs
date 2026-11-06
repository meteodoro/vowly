using Vowly.Api.Services;

namespace Vowly.Api.Dtos;

public class RelatorioEmpresaRespostaDto
{
    public decimal PrecoPremium { get; set; }
    public decimal CustosFixosMensais { get; set; }
    public List<ItemCusto> Custos { get; set; } = [];
    public int PontoEquilibrio { get; set; }
    public int AssinantesPremiumVigentes { get; set; }
    public int FaltamParaEquilibrio { get; set; }
    public List<RelatorioMesDto> Meses { get; set; } = [];
}