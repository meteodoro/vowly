namespace Vowly.Api.Dtos.Parcelas;

public class ParcelasPendentesResponse
{
    public DateOnly Hoje { get; set; }
    public int JanelaDias { get; set; }
    public List<ParcelaPendenteResponse> Atrasadas { get; set; } = [];
    public List<ParcelaPendenteResponse> AVencer { get; set; } = [];
    public decimal TotalAtrasado { get; set; }
    public decimal TotalAVencer { get; set; }
}