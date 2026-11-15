using System.ComponentModel.DataAnnotations;

namespace Vowly.Api.Dtos.Parcelas;

public class GerarParcelasRequest
{
    [Range(2, 60, ErrorMessage = "A quantidade de parcelas deve ficar entre 2 e 60.")]
    public int Quantidade { get; set; }

    public required DateOnly PrimeiroVencimento { get; set; }
}