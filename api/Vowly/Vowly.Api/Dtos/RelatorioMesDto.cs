namespace Vowly.Api.Dtos;

public class RelatorioMesDto
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public int Assinantes { get; set; }
    public decimal Receita { get; set; }
    public decimal Custos { get; set; }
    public decimal Lucro { get; set; }
}