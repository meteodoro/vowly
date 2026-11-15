using System.ComponentModel.DataAnnotations;

namespace Vowly.Api.Dtos.Categorias;

public class CriarCategoriaRequest
{
    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;
}