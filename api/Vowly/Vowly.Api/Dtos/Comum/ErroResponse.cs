using System.Text.Json.Serialization;

namespace Vowly.Api.Dtos.Comum;

public class ErroResponse
{
    public string Mensagem { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? Erros { get; set; }

    // Só preenchido em Desenvolvimento
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Detalhe { get; set; }
}