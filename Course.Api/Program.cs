using Course.Api.Exceptions;
using Course.Api.Middleware;
using Course.Api.Models;
using Course.Api.Repositories;
using Course.Api.Repositories.Interfaces;
using Course.Api.Services;
using Course.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Padroniza a resposta de erros de validacao (DataAnnotations) no mesmo formato usado pelo middleware de excecoes.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var detalhes = context.ModelState
            .Where(par => par.Value?.Errors.Count > 0)
            .ToDictionary(
                par => par.Key,
                par => par.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ValidacaoEntrada")
            .LogWarning("Falha de validacao em {Path}: {@Detalhes}", context.HttpContext.Request.Path, detalhes);

        var resposta = new ErroResposta
        {
            Msg = MensagensErro.DadosInvalidos,
            StatusCode = StatusCodes.Status400BadRequest,
            TraceId = context.HttpContext.TraceIdentifier,
            Detalhes = detalhes
        };

        return new BadRequestObjectResult(resposta);
    };
});

builder.Services.AddSingleton<IContaRepository, ContaRepository>();
builder.Services.AddSingleton<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<ICarteiraService, CarteiraService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

