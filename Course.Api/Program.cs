using Course.Api.Middleware;
using Course.Api.Repositories;
using Course.Api.Repositories.Interfaces;
using Course.Api.Services;
using Course.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IContaRepository, ContaRepository>();
builder.Services.AddSingleton<ITransacaoRepository, TransacaoRepository>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<ICarteiraService, CarteiraService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
