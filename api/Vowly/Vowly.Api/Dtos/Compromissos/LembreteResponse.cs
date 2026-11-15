namespace Vowly.Api.Dtos.Compromissos;

public class LembretesResponse
{
    public List<CompromissoResponse> Atrasados { get; set; } = [];
    public List<CompromissoResponse> Proximos { get; set; } = [];
}