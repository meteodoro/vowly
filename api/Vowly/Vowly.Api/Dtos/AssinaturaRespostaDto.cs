using Vowly.Api.Enums;

namespace Vowly.Api.Dtos;

public class AssinaturaRespostaDto
{
    public SituacaoPlano Situacao { get; set; }
    public bool PremiumVigente { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public decimal? ValorMensal { get; set; }
    public int DespesasCadastradas { get; set; }

    // Nulo quando o Premium está vigente (sem limite)
    public int? LimiteDespesas { get; set; }

    public decimal PrecoPremium { get; set; }
}