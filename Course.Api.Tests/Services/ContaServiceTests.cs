using Course.Api.Controllers.Requests;
using Course.Api.Exceptions;
using Course.Api.Models;
using Course.Api.Repositories.Interfaces;
using Course.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Course.Api.Tests.Services;

public class ContaServiceTests
{
    private readonly Mock<IContaRepository> _repositoryMock = new();
    private readonly ContaService _service;

    public ContaServiceTests()
    {
        _service = new ContaService(_repositoryMock.Object, NullLogger<ContaService>.Instance);
    }

    private static CriarCarteiraRequest RequestBase() => new()
    {
        Nome = "Maria Souza",
        Cpf = "123.456.789-00",
        Senha = "senha123",
        Email = "maria@email.com"
    };

    private static Conta ContaAtiva(int id = 1, decimal saldo = 500) => new()
    {
        Id = id,
        NomeCompleto = "Maria Souza",
        Cpf = "123.456.789-00",
        SenhaHash = "hash",
        Email = "maria@email.com",
        Saldo = saldo,
        Ativa = true,
        DataCriacao = DateTime.UtcNow
    };

    // --- CriarConta ---

    [Fact]
    public void CriarConta_ComDadosValidos_RetornaConta()
    {
        _repositoryMock.Setup(r => r.BuscarPorCpf(It.IsAny<string>())).Returns((Conta?)null);
        _repositoryMock.Setup(r => r.Adicionar(It.IsAny<Conta>())).Returns<Conta>(c => { c.Id = 1; return c; });

        var conta = _service.CriarConta(RequestBase());

        Assert.Equal(1, conta.Id);
        Assert.Equal("Maria Souza", conta.NomeCompleto);
        Assert.True(conta.Ativa);
        Assert.NotEmpty(conta.SenhaHash);
    }

    [Fact]
    public void CriarConta_ComCpfDuplicado_LancaCpfJaCadastradoException()
    {
        _repositoryMock.Setup(r => r.BuscarPorCpf(It.IsAny<string>())).Returns(ContaAtiva());

        Assert.Throws<CpfJaCadastradoException>(() => _service.CriarConta(RequestBase()));
    }

    [Fact]
    public void CriarConta_SenhaNaoArmazenadaEmTextoPlano()
    {
        _repositoryMock.Setup(r => r.BuscarPorCpf(It.IsAny<string>())).Returns((Conta?)null);
        _repositoryMock.Setup(r => r.Adicionar(It.IsAny<Conta>())).Returns<Conta>(c => c);

        var conta = _service.CriarConta(RequestBase());

        Assert.NotEqual("senha123", conta.SenhaHash);
    }

    // --- BuscarConta ---

    [Fact]
    public void BuscarConta_ContaExistente_RetornaConta()
    {
        _repositoryMock.Setup(r => r.BuscarPorId(1)).Returns(ContaAtiva());

        var conta = _service.BuscarConta(1);

        Assert.Equal(1, conta.Id);
    }

    [Fact]
    public void BuscarConta_ContaInexistente_LancaContaNaoEncontradaException()
    {
        _repositoryMock.Setup(r => r.BuscarPorId(It.IsAny<int>())).Returns((Conta?)null);

        Assert.Throws<ContaNaoEncontradaException>(() => _service.BuscarConta(99));
    }

    // --- AtualizarConta ---

    [Fact]
    public void AtualizarConta_ContaExistente_AtualizaERetorna()
    {
        _repositoryMock.Setup(r => r.BuscarPorId(1)).Returns(ContaAtiva());

        var request = new AtualizarContaRequest { NovoNome = "Novo Nome", NovoEmail = "novo@email.com", NovaSenha = "novaSenha" };
        var resultado = _service.AtualizarConta(1, request);

        Assert.Equal("Novo Nome", resultado.NomeCompleto);
        Assert.Equal("novo@email.com", resultado.Email);
    }

    [Fact]
    public void AtualizarConta_ContaInexistente_LancaContaNaoEncontradaException()
    {
        _repositoryMock.Setup(r => r.BuscarPorId(It.IsAny<int>())).Returns((Conta?)null);

        var request = new AtualizarContaRequest { NovoNome = "x", NovoEmail = "x@x.com", NovaSenha = "x" };
        Assert.Throws<ContaNaoEncontradaException>(() => _service.AtualizarConta(99, request));
    }

    // --- DesativarConta ---

    [Fact]
    public void DesativarConta_ContaAtiva_DesativaERertorna()
    {
        _repositoryMock.Setup(r => r.BuscarPorId(1)).Returns(ContaAtiva());

        var resultado = _service.DesativarConta(1);

        Assert.False(resultado.Ativa);
        _repositoryMock.Verify(r => r.Atualizar(It.IsAny<Conta>()), Times.Once);
    }

    [Fact]
    public void DesativarConta_ContaInexistente_LancaContaNaoEncontradaException()
    {
        _repositoryMock.Setup(r => r.BuscarPorId(It.IsAny<int>())).Returns((Conta?)null);

        Assert.Throws<ContaNaoEncontradaException>(() => _service.DesativarConta(99));
    }

    // --- ConsultarSaldo ---

    [Fact]
    public void ConsultarSaldo_ContaExistente_RetornaSaldo()
    {
        _repositoryMock.Setup(r => r.BuscarPorId(1)).Returns(ContaAtiva(saldo: 750));

        var saldo = _service.ConsultarSaldo(1);

        Assert.Equal(750, saldo);
    }

    [Fact]
    public void ConsultarSaldo_ContaInexistente_LancaContaNaoEncontradaException()
    {
        _repositoryMock.Setup(r => r.BuscarPorId(It.IsAny<int>())).Returns((Conta?)null);

        Assert.Throws<ContaNaoEncontradaException>(() => _service.ConsultarSaldo(99));
    }

    // --- ListarContas ---

    [Fact]
    public void ListarContas_RetornaListaDoRepositorio()
    {
        var contas = new List<Conta> { ContaAtiva(1), ContaAtiva(2) }.AsReadOnly();
        _repositoryMock.Setup(r => r.ListarTodas()).Returns(contas);

        var resultado = _service.ListarContas();

        Assert.Equal(2, resultado.Count);
    }
}
