namespace Vowly.Api.Services;

public class OpcoesPlano
{
    public const string Secao = "Plano";

    public int LimiteDespesasGratuito { get; set; } = PlanoRegras.LimiteDespesasGratuito;
    public decimal PrecoPremium { get; set; } = 19.90m;
}