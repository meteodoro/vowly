using Vowly.Api.Enums;

namespace Vowly.Api.Dtos;

public class CompromissoRespostaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public string? Local { get; set; }
    public string? Observacoes { get; set; }
    public StatusCompromisso Status { get; set; }
    public int AntecedenciaLembreteDias { get; set; }
    public bool Atrasado { get; set; }
}