using Vowly.Api.Middleware;

namespace Vowly.Api.Tests;

public class ErrosDeValidacaoTests
{
    [Theory]
    [InlineData("Titulo", "The Titulo field is required.",
        "O campo titulo é obrigatório.")]
    [InlineData("NomeNoivo1", "The NomeNoivo1 field is required.",
        "O campo nomeNoivo1 é obrigatório.")]
    [InlineData("Titulo", "The field Titulo must be a string or array type with a maximum length of '150'.",
        "O campo titulo deve ter no máximo 150 caracteres.")]
    [InlineData("Email", "The Email field is not a valid e-mail address.",
        "O campo email não é um e-mail válido.")]
    [InlineData("Status", "The field Status is invalid.",
        "O campo status tem um valor inválido.")]
    [InlineData("dto", "A non-empty request body is required.",
        "O corpo da requisição é obrigatório.")]
    public void Traduzir_MensagensPadraoDoDotNet_ViramPortugues(string chave, string original, string esperado)
    {
        Assert.Equal(esperado, ErrosDeValidacao.Traduzir(chave, original));
    }

    [Fact]
    public void Traduzir_MensagemJaEmPortugues_NaoEhAlterada()
    {
        const string mensagem = "O orçamento deve ser maior que zero.";

        Assert.Equal(mensagem, ErrosDeValidacao.Traduzir("OrcamentoTotal", mensagem));
    }

    [Theory]
    [InlineData("$.dataHora")]
    [InlineData("$")]
    public void Traduzir_ErroDeLeituraDoJson_UsaMensagemGenerica(string chave)
    {
        var resultado = ErrosDeValidacao.Traduzir(chave, "The JSON value could not be converted to System.DateTime.");

        Assert.Equal(ErrosDeValidacao.MensagemCorpoInvalido, resultado);
    }

    [Theory]
    [InlineData("OrcamentoTotal", "orcamentoTotal")]
    [InlineData("$.dataHora", "dataHora")]
    [InlineData("$", "corpo")]
    [InlineData("", "corpo")]
    public void NormalizarChave_UsaOMesmoNomeDoJson(string chave, string esperado)
    {
        Assert.Equal(esperado, ErrosDeValidacao.NormalizarChave(chave));
    }
}