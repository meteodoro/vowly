namespace Vowly.Api.Dtos;

public class ParcelasPendentesRespostaDto
{
    public DateOnly Hoje { get; set; }
    public int JanelaDias { get; set; }
    public List<ParcelaPendenteDto> Atrasadas { get; set; } = [];
    public List<ParcelaPendenteDto> AVencer { get; set; } = [];
    public decimal TotalAtrasado { get; set; }
    public decimal TotalAVencer { get; set; }
}