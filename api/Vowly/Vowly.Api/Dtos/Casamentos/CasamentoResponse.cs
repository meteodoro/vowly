namespace Vowly.Api.Dtos.Casamentos;

public class CasamentoResponse
{
    public int Id { get; set; }
    public string NomeNoivo1 { get; set; } = string.Empty;
    public string NomeNoivo2 { get; set; } = string.Empty;
    public DateOnly DataPrevista { get; set; }
    public string? Cidade { get; set; }
    public decimal OrcamentoTotal { get; set; }
}