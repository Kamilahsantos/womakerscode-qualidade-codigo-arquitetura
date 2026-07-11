// programa principal - configuracao da aplicacao
var b = WebApplication.CreateBuilder(args);

b.Services.AddControllers();
b.Services.AddEndpointsApiExplorer();
b.Services.AddOpenApi();

var a = b.Build();

if (a.Environment.IsDevelopment())
{
    a.MapOpenApi();
}

a.UseHttpsRedirection();
a.MapControllers();
a.Run();
