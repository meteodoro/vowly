using System.ComponentModel.DataAnnotations;

namespace Vowly.Api.Dtos;

public class GerarParcelasDto
{
    [Range(2, 60, ErrorMessage = "A quantidade de parcelas deve ficar entre 2 e 60.")]
    public int Quantidade { get; set; }

    [Required]
    public DateOnly PrimeiroVencimento { get; set; }
}