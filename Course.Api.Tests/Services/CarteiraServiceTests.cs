using Course.Api.Exceptions;
using Course.Api.Models;
using Course.Api.Repositories.Interfaces;
using Course.Api.Services;
using Moq;
using Xunit;

namespace Course.Api.Tests.Services;

public class CarteiraServiceTests
{
    private readonly Mock<IContaRepository> _contaRepoMock = new();
    private readonly Mock<ITransacaoRepository> _transacaoRepoMock = new();
    private readonly CarteiraService _service;

    public CarteiraServiceTests()
    {
        _service = new CarteiraService(_contaRepoMock.Object, _transacaoRepoMock.Object);
    }

    private static Conta ContaAtiva(int id = 1, decimal saldo = 500) => new()
    {
        Id = id,
        NomeCompleto = "Ana Lima",
        Cpf = "111.222.333-44",
        SenhaHash = "hash",
        Email = "ana@email.com",
        Saldo = saldo,
        Ativa = true,
        DataCriacao = DateTime.UtcNow
    };

    private static Conta ContaInativa(int id = 2) => new()
    {
        Id = id,
        NomeCompleto = "Pedro Inativo",
        Cpf = "999.888.777-66",
        SenhaHash = "hash",
        Email = "pedro@email.com",
        Saldo = 0,
        Ativa = false,
        DataCriacao = DateTime.UtcNow
    };

    // --- Depositar ---

    [Fact]
    public void Depositar_ValorValido_AtualizaSaldoERegistraTransacao()
    {
        var conta = ContaAtiva(saldo: 100);
        _contaRepoMock.Setup(r => r.BuscarPorId(1)).Returns(conta);

        var resultado = _service.Depositar(1, 200);

        Assert.Equal(300, resultado.Saldo);
        _transacaoRepoMock.Verify(r => r.Adicionar(It.Is<Transacao>(t => t.Tipo == "D" && t.Valor == 200)), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Depositar_ValorInvalido_LancaValorInvalidoException(decimal valor)
    {
        Assert.Throws<ValorInvalidoException>(() => _service.Depositar(1, valor));
    }

    [Fact]
    public void Depositar_ContaInexistente_LancaContaNaoEncontradaException()
    {
        _contaRepoMock.Setup(r => r.BuscarPorId(It.IsAny<int>())).Returns((Conta?)null);

        Assert.Throws<ContaNaoEncontradaException>(() => _service.Depositar(99, 100));
    }

    [Fact]
    public void Depositar_ContaInativa_LancaContaInativaException()
    {
        _contaRepoMock.Setup(r => r.BuscarPorId(2)).Returns(ContaInativa());

        Assert.Throws<ContaInativaException>(() => _service.Depositar(2, 100));
    }

    // --- Sacar ---

    [Fact]
    public void Sacar_ValorValido_DebitaSaldoERegistraTransacao()
    {
        var conta = ContaAtiva(saldo: 300);
        _contaRepoMock.Setup(r => r.BuscarPorId(1)).Returns(conta);

        var resultado = _service.Sacar(1, 100);

        Assert.Equal(200, resultado.Saldo);
        _transacaoRepoMock.Verify(r => r.Adicionar(It.Is<Transacao>(t => t.Tipo == "S" && t.Valor == 100)), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Sacar_ValorInvalido_LancaValorInvalidoException(decimal valor)
    {
        Assert.Throws<ValorInvalidoException>(() => _service.Sacar(1, valor));
    }

    [Fact]
    public void Sacar_SaldoInsuficiente_LancaSaldoInsuficienteException()
    {
        _contaRepoMock.Setup(r => r.BuscarPorId(1)).Returns(ContaAtiva(saldo: 50));

        Assert.Throws<SaldoInsuficienteException>(() => _service.Sacar(1, 200));
    }

    [Fact]
    public void Sacar_ContaInexistente_LancaContaNaoEncontradaException()
    {
        _contaRepoMock.Setup(r => r.BuscarPorId(It.IsAny<int>())).Returns((Conta?)null);

        Assert.Throws<ContaNaoEncontradaException>(() => _service.Sacar(99, 100));
    }

    [Fact]
    public void Sacar_ContaInativa_LancaContaInativaException()
    {
        _contaRepoMock.Setup(r => r.BuscarPorId(2)).Returns(ContaInativa());

        Assert.Throws<ContaInativaException>(() => _service.Sacar(2, 50));
    }

    // --- Transferir ---

    [Fact]
    public void Transferir_ValorValido_TransfereERegistraTransacao()
    {
        var origem = ContaAtiva(id: 1, saldo: 500);
        var destino = ContaAtiva(id: 2, saldo: 0);
        _contaRepoMock.Setup(r => r.BuscarPorId(1)).Returns(origem);
        _contaRepoMock.Setup(r => r.BuscarPorId(2)).Returns(destino);

        _service.Transferir(1, 2, 200);

        Assert.Equal(300, origem.Saldo);
        Assert.Equal(200, destino.Saldo);
        _transacaoRepoMock.Verify(r => r.Adicionar(It.Is<Transacao>(t => t.Tipo == "T" && t.Valor == 200)), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Transferir_ValorInvalido_LancaValorInvalidoException(decimal valor)
    {
        Assert.Throws<ValorInvalidoException>(() => _service.Transferir(1, 2, valor));
    }

    [Fact]
    public void Transferir_SaldoInsuficiente_LancaSaldoInsuficienteException()
    {
        _contaRepoMock.Setup(r => r.BuscarPorId(1)).Returns(ContaAtiva(saldo: 50));
        _contaRepoMock.Setup(r => r.BuscarPorId(2)).Returns(ContaAtiva(id: 2));

        Assert.Throws<SaldoInsuficienteException>(() => _service.Transferir(1, 2, 200));
    }

    [Fact]
    public void Transferir_ContaOrigemInexistente_LancaContaNaoEncontradaException()
    {
        _contaRepoMock.Setup(r => r.BuscarPorId(99)).Returns((Conta?)null);

        Assert.Throws<ContaNaoEncontradaException>(() => _service.Transferir(99, 2, 100));
    }

    [Fact]
    public void Transferir_ContaDestinoInativa_LancaContaInativaException()
    {
        _contaRepoMock.Setup(r => r.BuscarPorId(1)).Returns(ContaAtiva(saldo: 500));
        _contaRepoMock.Setup(r => r.BuscarPorId(2)).Returns(ContaInativa(id: 2));

        Assert.Throws<ContaInativaException>(() => _service.Transferir(1, 2, 100));
    }
}
