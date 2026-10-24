using Vowly.Api.Enums;

namespace Vowly.Api.Models;

public class Compromisso
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public string? Local { get; set; }
    public string? Observacoes { get; set; }
    public StatusCompromisso Status { get; set; }
    public int AntecedenciaLembreteDias { get; set; }
    
    public int CasamentoId { get; set; }
    public Casamento Casamento { get; set; } = null!;

}