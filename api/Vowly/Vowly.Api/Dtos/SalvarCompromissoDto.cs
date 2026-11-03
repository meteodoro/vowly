using System.ComponentModel.DataAnnotations;
using Vowly.Api.Enums;

namespace Vowly.Api.Dtos;

public class SalvarCompromissoDto
{
    [Required]
    [MaxLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Tipo { get; set; } = string.Empty;

    public required DateTime DataHora { get; set; }

    [MaxLength(200)]
    public string? Local { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [EnumDataType(typeof(StatusCompromisso))]
    public StatusCompromisso Status { get; set; } = StatusCompromisso.Pendente;

    [Range(0, 90, ErrorMessage = "A antecedência do lembrete deve ficar entre 0 e 90 dias.")]
    public int AntecedenciaLembreteDias { get; set; } = 7;
}