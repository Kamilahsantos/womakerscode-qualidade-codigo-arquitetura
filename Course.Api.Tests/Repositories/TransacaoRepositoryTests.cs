using Course.Api.Models;
using Course.Api.Repositories;
using Xunit;

namespace Course.Api.Tests.Repositories;

public class TransacaoRepositoryTests
{
    private readonly TransacaoRepository _repository = new();

    private static Transacao TransacaoBase(int origemId, int destinoId, string tipo = "D") => new()
    {
        ContaOrigemId = origemId,
        ContaDestinoId = destinoId,
        Valor = 100,
        Tipo = tipo,
        DataHora = DateTime.UtcNow,
        Descricao = "Teste",
        Status = "ok"
    };

    [Fact]
    public void Adicionar_NovaTransacao_AtribuiIdIniciandoEm100()
    {
        var transacao = TransacaoBase(1, 1);
        _repository.Adicionar(transacao);

        var lista = _repository.ListarTodas();

        Assert.Single(lista);
        Assert.Equal(100, lista[0].Id);
    }

    [Fact]
    public void Adicionar_MultiplasTransacoes_IdsSequenciais()
    {
        var t1 = TransacaoBase(1, 1);
        var t2 = TransacaoBase(2, 3);
        _repository.Adicionar(t1);
        _repository.Adicionar(t2);

        Assert.Equal(100, t1.Id);
        Assert.Equal(101, t2.Id);
    }

    [Fact]
    public void ListarTodas_RetornaTodasAsTransacoes()
    {
        _repository.Adicionar(TransacaoBase(1, 1));
        _repository.Adicionar(TransacaoBase(2, 3));

        var lista = _repository.ListarTodas();

        Assert.Equal(2, lista.Count);
    }

    [Fact]
    public void BuscarPorContaId_RetornaTransacoesOndeContaEhOrigemOuDestino()
    {
        _repository.Adicionar(TransacaoBase(1, 1));
        _repository.Adicionar(TransacaoBase(2, 1, "T"));
        _repository.Adicionar(TransacaoBase(2, 3));

        var extrato = _repository.BuscarPorContaId(1);

        Assert.Equal(2, extrato.Count);
    }

    [Fact]
    public void BuscarPorContaId_ContaSemTransacoes_RetornaListaVazia()
    {
        _repository.Adicionar(TransacaoBase(1, 1));

        var extrato = _repository.BuscarPorContaId(99);

        Assert.Empty(extrato);
    }
}
