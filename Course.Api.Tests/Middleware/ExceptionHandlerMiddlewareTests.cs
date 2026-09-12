using System.Text.Json;
using Course.Api.Exceptions;
using Course.Api.Middleware;
using Course.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Course.Api.Tests.Middleware;

public class ExceptionHandlerMiddlewareTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static async Task<(int StatusCode, ErroResposta? Corpo)> ExecutarAsync(RequestDelegate next)
    {
        var middleware = new ExceptionHandlerMiddleware(next, NullLogger<ExceptionHandlerMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        ErroResposta? corpo = null;
        if (context.Response.ContentType?.StartsWith("application/json") == true)
        {
            corpo = await JsonSerializer.DeserializeAsync<ErroResposta>(context.Response.Body, JsonOptions);
        }

        return (context.Response.StatusCode, corpo);
    }

    [Fact]
    public async Task InvokeAsync_SemExcecao_NaoAlteraResposta()
    {
        var (statusCode, corpo) = await ExecutarAsync(context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });

        Assert.Equal(StatusCodes.Status200OK, statusCode);
        Assert.Null(corpo);
    }

    [Fact]
    public async Task InvokeAsync_ContaNaoEncontradaException_Retorna404ComMensagem()
    {
        var (statusCode, corpo) = await ExecutarAsync(_ => throw new ContaNaoEncontradaException(42));

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
        Assert.NotNull(corpo);
        Assert.Equal(string.Format(MensagensErro.ContaNaoEncontrada, 42), corpo!.Msg);
        Assert.Equal(StatusCodes.Status404NotFound, corpo.StatusCode);
    }

    [Theory]
    [InlineData(typeof(SaldoInsuficienteException))]
    [InlineData(typeof(ValorInvalidoException))]
    [InlineData(typeof(CpfJaCadastradoException))]
    public async Task InvokeAsync_DomainException_Retorna400ComMensagem(Type tipoExcecao)
    {
        var excecao = (DomainException)Activator.CreateInstance(tipoExcecao)!;

        var (statusCode, corpo) = await ExecutarAsync(_ => throw excecao);

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.NotNull(corpo);
        Assert.Equal(excecao.Message, corpo!.Msg);
    }

    [Fact]
    public async Task InvokeAsync_ExcecaoNaoMapeada_Retorna500SemVazarDetalhes()
    {
        var (statusCode, corpo) = await ExecutarAsync(_ => throw new InvalidOperationException("detalhe interno sensivel"));

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.NotNull(corpo);
        Assert.Equal(MensagensErro.ErroInterno, corpo!.Msg);
        Assert.DoesNotContain("detalhe interno sensivel", corpo.Msg);
    }
}
