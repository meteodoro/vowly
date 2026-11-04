namespace Vowly.Api.Dtos;

public class LembretesRespostaDto
{
    public List<CompromissoRespostaDto> Atrasados { get; set; } = [];
    public List<CompromissoRespostaDto> Proximos { get; set; } = [];
}