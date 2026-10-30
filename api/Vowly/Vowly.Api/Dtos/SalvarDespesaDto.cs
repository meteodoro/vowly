using System.ComponentModel.DataAnnotations;
using Vowly.Api.Enums;

namespace Vowly.Api.Dtos;

public class SalvarDespesaDto
{
    [Required]
    [MaxLength(150)]
    public string Descricao { get; set; } = string.Empty;

    [Range(0.01, 999999999.99, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal ValorTotal { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe a categoria.")]
    public int CategoriaId { get; set; }

    public int? FornecedorId { get; set; }

    [EnumDataType(typeof(StatusDespesa))]
    public StatusDespesa Status { get; set; } = StatusDespesa.APagar;
}