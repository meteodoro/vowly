using Microsoft.AspNetCore.Diagnostics;
using Npgsql;
using Vowly.Api.Dtos;

namespace Vowly.Api.Middleware;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment ambiente) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext contexto, Exception excecao, CancellationToken cancellationToken)
    {
        // O cliente desistiu da requisição: não é falha do servidor
        if (excecao is OperationCanceledException && contexto.RequestAborted.IsCancellationRequested)
        {
            contexto.Response.StatusCode = 499;
            return true;
        }

        var (status, mensagem) = Classificar(excecao);

        // A pilha completa vai para o log, nunca para o cliente
        logger.LogError(excecao, "Erro ao processar {Metodo} {Caminho}",
            contexto.Request.Method, contexto.Request.Path);

        contexto.Response.StatusCode = status;
        await contexto.Response.WriteAsJsonAsync(new ErroRespostaDto
        {
            Mensagem = mensagem,
            Detalhe = ambiente.IsDevelopment() ? excecao.Message : null
        }, cancellationToken);

        return true;
    }

    private static (int Status, string Mensagem) Classificar(Exception excecao) => excecao switch
    {
        BadHttpRequestException invalida =>
            (invalida.StatusCode, "Requisição inválida."),

        _ when SemConexaoComOBanco(excecao) =>
            (StatusCodes.Status503ServiceUnavailable,
                "Não foi possível acessar o banco de dados agora. Tente novamente em instantes."),

        _ => (StatusCodes.Status500InternalServerError,
                "Ocorreu um erro inesperado. Tente novamente.")
    };

    // Falha de conexão, e não um erro de SQL (PostgresException é erro de consulta ou de regra do banco)
    private static bool SemConexaoComOBanco(Exception excecao)
    {
        for (var atual = excecao; atual is not null; atual = atual.InnerException)
        {
            if (atual is NpgsqlException and not PostgresException)
                return true;
        }

        return false;
    }
}