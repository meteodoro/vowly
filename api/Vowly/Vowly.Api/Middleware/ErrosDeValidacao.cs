using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Vowly.Api.Dtos;

namespace Vowly.Api.Middleware;

public static class ErrosDeValidacao
{
    public const string MensagemGeral = "Verifique os campos informados.";
    public const string MensagemCorpoInvalido = "O corpo da requisição está incompleto ou em formato inválido.";

    private static readonly (Regex Padrao, string Modelo)[] Traducoes =
    [
        (new(@"^The (?<campo>\w+) field is required\.$"),
            "O campo {campo} é obrigatório."),
        (new(@"^The field (?<campo>\w+) must be a string or array type with a maximum length of '(?<max>\d+)'\.$"),
            "O campo {campo} deve ter no máximo {max} caracteres."),
        (new(@"^The field (?<campo>\w+) must be a string with a maximum length of (?<max>\d+)\.$"),
            "O campo {campo} deve ter no máximo {max} caracteres."),
        (new(@"^The field (?<campo>\w+) must be a string or array type with a minimum length of '(?<min>\d+)'\.$"),
            "O campo {campo} deve ter no mínimo {min} caracteres."),
        (new(@"^The (?<campo>\w+) field is not a valid e-mail address\.$"),
            "O campo {campo} não é um e-mail válido."),
        (new(@"^The field (?<campo>\w+) is invalid\.$"),
            "O campo {campo} tem um valor inválido."),
        (new(@"^A non-empty request body is required\.$"),
            "O corpo da requisição é obrigatório.")
    ];

    public static ErroRespostaDto Montar(ModelStateDictionary modelState)
    {
        var erros = new Dictionary<string, string[]>();

        foreach (var (chave, entrada) in modelState)
        {
            if (entrada is null || entrada.Errors.Count == 0)
                continue;

            erros[NormalizarChave(chave)] = entrada.Errors
                .Select(e => Traduzir(chave, e.ErrorMessage))
                .Distinct()
                .ToArray();
        }

        return new ErroRespostaDto { Mensagem = MensagemGeral, Erros = erros };
    }

    public static string Traduzir(string chave, string mensagem)
    {
        // Erros do leitor de JSON (chaves "$..."): a mensagem original é técnica demais
        if (chave.StartsWith('$') || string.IsNullOrWhiteSpace(mensagem))
            return MensagemCorpoInvalido;

        foreach (var (padrao, modelo) in Traducoes)
        {
            var encontrado = padrao.Match(mensagem);
            if (!encontrado.Success)
                continue;

            var texto = modelo;
            foreach (var nome in padrao.GetGroupNames().Where(n => !int.TryParse(n, out _)))
            {
                var valor = encontrado.Groups[nome].Value;
                if (nome == "campo")
                    valor = PrimeiraMinuscula(valor);

                texto = texto.Replace("{" + nome + "}", valor);
            }

            return texto;
        }

        return mensagem;
    }

    public static string NormalizarChave(string chave)
    {
        if (string.IsNullOrEmpty(chave) || chave == "$")
            return "corpo";

        if (chave.StartsWith("$."))
            chave = chave[2..];

        return PrimeiraMinuscula(chave);
    }

    private static string PrimeiraMinuscula(string texto) =>
        texto.Length == 0 ? texto : char.ToLowerInvariant(texto[0]) + texto[1..];
}