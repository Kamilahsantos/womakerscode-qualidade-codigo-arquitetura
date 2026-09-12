using System.ComponentModel.DataAnnotations;
using Course.Api.Controllers.Requests;
using Xunit;

namespace Course.Api.Tests.Controllers.Requests;

public class RequestValidationTests
{
    private static IList<ValidationResult> Validar(object request)
    {
        var contexto = new ValidationContext(request);
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(request, contexto, resultados, validateAllProperties: true);
        return resultados;
    }

    // --- CriarCarteiraRequest ---

    [Fact]
    public void CriarCarteiraRequest_DadosValidos_NaoGeraErros()
    {
        var request = new CriarCarteiraRequest { Nome = "Maria Souza", Cpf = "123.456.789-00", Senha = "senha123", Email = "maria@email.com" };

        Assert.Empty(Validar(request));
    }

    [Fact]
    public void CriarCarteiraRequest_NomeVazio_GeraErro()
    {
        var request = new CriarCarteiraRequest { Nome = "", Cpf = "123.456.789-00", Senha = "senha123", Email = "maria@email.com" };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(CriarCarteiraRequest.Nome)));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc.def.ghi-jk")]
    public void CriarCarteiraRequest_CpfFormatoInvalido_GeraErro(string cpf)
    {
        var request = new CriarCarteiraRequest { Nome = "Maria Souza", Cpf = cpf, Senha = "senha123", Email = "maria@email.com" };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(CriarCarteiraRequest.Cpf)));
    }

    [Fact]
    public void CriarCarteiraRequest_SenhaMenorQueMinimo_GeraErro()
    {
        var request = new CriarCarteiraRequest { Nome = "Maria Souza", Cpf = "123.456.789-00", Senha = "123", Email = "maria@email.com" };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(CriarCarteiraRequest.Senha)));
    }

    [Fact]
    public void CriarCarteiraRequest_EmailFormatoInvalido_GeraErro()
    {
        var request = new CriarCarteiraRequest { Nome = "Maria Souza", Cpf = "123.456.789-00", Senha = "senha123", Email = "email-invalido" };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(CriarCarteiraRequest.Email)));
    }

    // --- AtualizarContaRequest ---

    [Fact]
    public void AtualizarContaRequest_DadosValidos_NaoGeraErros()
    {
        var request = new AtualizarContaRequest { NovoNome = "Maria Souza", NovoEmail = "maria@email.com", NovaSenha = "senha123" };

        Assert.Empty(Validar(request));
    }

    [Fact]
    public void AtualizarContaRequest_EmailFormatoInvalido_GeraErro()
    {
        var request = new AtualizarContaRequest { NovoNome = "Maria Souza", NovoEmail = "email-invalido", NovaSenha = "senha123" };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(AtualizarContaRequest.NovoEmail)));
    }

    [Fact]
    public void AtualizarContaRequest_SenhaMenorQueMinimo_GeraErro()
    {
        var request = new AtualizarContaRequest { NovoNome = "Maria Souza", NovoEmail = "maria@email.com", NovaSenha = "123" };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(AtualizarContaRequest.NovaSenha)));
    }

    // --- DepositarRequest / SacarRequest ---

    [Fact]
    public void DepositarRequest_DadosValidos_NaoGeraErros()
    {
        var request = new DepositarRequest { Id = 1, Valor = 100 };

        Assert.Empty(Validar(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DepositarRequest_IdInvalido_GeraErro(int id)
    {
        var request = new DepositarRequest { Id = id, Valor = 100 };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(DepositarRequest.Id)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void DepositarRequest_ValorInvalido_GeraErro(decimal valor)
    {
        var request = new DepositarRequest { Id = 1, Valor = valor };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(DepositarRequest.Valor)));
    }

    [Fact]
    public void SacarRequest_DadosValidos_NaoGeraErros()
    {
        var request = new SacarRequest { Id = 1, Valor = 50 };

        Assert.Empty(Validar(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SacarRequest_IdInvalido_GeraErro(int id)
    {
        var request = new SacarRequest { Id = id, Valor = 50 };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(SacarRequest.Id)));
    }

    // --- TransferirRequest ---

    [Fact]
    public void TransferirRequest_DadosValidos_NaoGeraErros()
    {
        var request = new TransferirRequest { IdOrigem = 1, IdDestino = 2, Valor = 30 };

        Assert.Empty(Validar(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TransferirRequest_IdOrigemInvalido_GeraErro(int idOrigem)
    {
        var request = new TransferirRequest { IdOrigem = idOrigem, IdDestino = 2, Valor = 30 };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(TransferirRequest.IdOrigem)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TransferirRequest_IdDestinoInvalido_GeraErro(int idDestino)
    {
        var request = new TransferirRequest { IdOrigem = 1, IdDestino = idDestino, Valor = 30 };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(TransferirRequest.IdDestino)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TransferirRequest_ValorInvalido_GeraErro(decimal valor)
    {
        var request = new TransferirRequest { IdOrigem = 1, IdDestino = 2, Valor = valor };

        Assert.Contains(Validar(request), r => r.MemberNames.Contains(nameof(TransferirRequest.Valor)));
    }
}
