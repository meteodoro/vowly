using System.ComponentModel.DataAnnotations;

namespace Vowly.Api.Dtos;

public class SalvarCategoriaDto
{
    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;
}