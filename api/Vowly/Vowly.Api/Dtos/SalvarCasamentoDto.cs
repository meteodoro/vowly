using System.ComponentModel.DataAnnotations;

namespace Vowly.Api.Dtos;

public class SalvarCasamentoDto
{
    [Required]
    [MaxLength(100)]
    public string NomeNoivo1 { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NomeNoivo2 { get; set; } = string.Empty;

    [Required]
    public DateOnly DataPrevista { get; set; }

    [MaxLength(100)]
    public string? Cidade { get; set; }

    [Range(0.01, 999999999.99, ErrorMessage = "O orçamento deve ser maior que zero.")]
    public decimal OrcamentoTotal { get; set; }
}