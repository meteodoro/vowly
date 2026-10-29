using Vowly.Api.Enums;

namespace Vowly.Api.Services;

public class Resultado<T>
{
    public T? Valor { get; private init; }
    public TipoErro? Erro { get; private init; }
    public string? Mensagem { get; private init; }
    public bool Sucesso => Erro is null;

    public static Resultado<T> Ok(T valor) => new() { Valor = valor };

    public static Resultado<T> Falha(TipoErro erro, string mensagem) =>
        new() { Erro = erro, Mensagem = mensagem };
}