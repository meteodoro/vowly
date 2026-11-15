using System.ComponentModel.DataAnnotations;

namespace Vowly.Api.Dtos.Categorias;

public class AtualizarCategoriaRequest
{
    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;
}