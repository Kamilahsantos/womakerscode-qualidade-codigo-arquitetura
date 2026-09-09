using Course.Api.Exceptions;
using Course.Api.Models;

namespace Course.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ContaNaoEncontradaException ex)
        {
            await EscreverRespostaAsync(context, ex.Message, StatusCodes.Status404NotFound, LogLevel.Warning);
        }
        catch (DomainException ex)
        {
            await EscreverRespostaAsync(context, ex.Message, StatusCodes.Status400BadRequest, LogLevel.Warning);
        }
        catch (Exception ex)
        {
            // Excecoes nao mapeadas nunca devem vazar detalhes internos para o cliente.
            _logger.LogError(ex, "Erro nao tratado ao processar {Method} {Path}", context.Request.Method, context.Request.Path);
            await EscreverRespostaAsync(context, MensagensErro.ErroInterno, StatusCodes.Status500InternalServerError, LogLevel.Error, logarMensagem: false);
        }
    }

    private async Task EscreverRespostaAsync(HttpContext context, string mensagem, int statusCode, LogLevel logLevel, bool logarMensagem = true)
    {
        if (logarMensagem)
        {
            _logger.Log(logLevel, "Falha ao processar {Method} {Path}: {Mensagem}", context.Request.Method, context.Request.Path, mensagem);
        }

        var resposta = new ErroResposta
        {
            Msg = mensagem,
            StatusCode = statusCode,
            TraceId = context.TraceIdentifier
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(resposta);
    }
}
