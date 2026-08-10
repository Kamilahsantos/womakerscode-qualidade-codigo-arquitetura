using Course.Api.Models;
using Course.Api.Repositories.Interfaces;
using Course.Api.Services;
using Moq;
using Xunit;

namespace Course.Api.Tests.Services;

public class TransacaoServiceTests
{
    private readonly Mock<ITransacaoRepository> _repositoryMock = new();
    private readonly TransacaoService _service;

    public TransacaoServiceTests()
    {
        _service = new TransacaoService(_repositoryMock.Object);
    }

    private static Transacao TransacaoBase(int origemId, int destinoId) => new()
    {
        Id = 100,
        ContaOrigemId = origemId,
        ContaDestinoId = destinoId,
        Valor = 100,
        Tipo = "D",
        DataHora = DateTime.UtcNow,
        Descricao = "Deposito",
        Status = "ok"
    };

    [Fact]
    public void ListarTransacoes_RetornaTodasDoRepositorio()
    {
        var lista = new List<Transacao> { TransacaoBase(1, 1), TransacaoBase(2, 3) }.AsReadOnly();
        _repositoryMock.Setup(r => r.ListarTodas()).Returns(lista);

        var resultado = _service.ListarTransacoes();

        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public void ListarTransacoes_SemTransacoes_RetornaListaVazia()
    {
        _repositoryMock.Setup(r => r.ListarTodas()).Returns(new List<Transacao>().AsReadOnly());

        var resultado = _service.ListarTransacoes();

        Assert.Empty(resultado);
    }

    [Fact]
    public void BuscarExtrato_RetornaTransacoesDaContaViaRepositorio()
    {
        var lista = new List<Transacao> { TransacaoBase(1, 1) }.AsReadOnly();
        _repositoryMock.Setup(r => r.BuscarPorContaId(1)).Returns(lista);

        var resultado = _service.BuscarExtrato(1);

        Assert.Single(resultado);
        Assert.Equal(1, resultado[0].ContaOrigemId);
    }

    [Fact]
    public void BuscarExtrato_ContaSemTransacoes_RetornaListaVazia()
    {
        _repositoryMock.Setup(r => r.BuscarPorContaId(99)).Returns(new List<Transacao>().AsReadOnly());

        var resultado = _service.BuscarExtrato(99);

        Assert.Empty(resultado);
    }
}
