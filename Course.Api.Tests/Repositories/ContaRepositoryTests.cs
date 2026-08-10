using Course.Api.Models;
using Course.Api.Repositories;
using Xunit;

namespace Course.Api.Tests.Repositories;

public class ContaRepositoryTests
{
    private readonly ContaRepository _repository = new();

    private static Conta ContaBase(string cpf = "111.111.111-11") => new()
    {
        NomeCompleto = "Joana Silva",
        Cpf = cpf,
        SenhaHash = "hash123",
        Email = "joana@email.com",
        Saldo = 0,
        Ativa = true,
        DataCriacao = DateTime.UtcNow
    };

    [Fact]
    public void Adicionar_NovaConta_AtribuiIdPartindoDe1()
    {
        var conta = _repository.Adicionar(ContaBase());

        Assert.Equal(1, conta.Id);
    }

    [Fact]
    public void Adicionar_MultiplasContas_IdsSequenciais()
    {
        var c1 = _repository.Adicionar(ContaBase("111.111.111-11"));
        var c2 = _repository.Adicionar(ContaBase("222.222.222-22"));

        Assert.Equal(1, c1.Id);
        Assert.Equal(2, c2.Id);
    }

    [Fact]
    public void BuscarPorId_ContaExistente_RetornaConta()
    {
        var adicionada = _repository.Adicionar(ContaBase());

        var encontrada = _repository.BuscarPorId(adicionada.Id);

        Assert.NotNull(encontrada);
        Assert.Equal(adicionada.Id, encontrada.Id);
    }

    [Fact]
    public void BuscarPorId_ContaInexistente_RetornaNull()
    {
        var resultado = _repository.BuscarPorId(99);

        Assert.Null(resultado);
    }

    [Fact]
    public void BuscarPorCpf_ContaExistente_RetornaConta()
    {
        _repository.Adicionar(ContaBase("123.456.789-00"));

        var encontrada = _repository.BuscarPorCpf("123.456.789-00");

        Assert.NotNull(encontrada);
        Assert.Equal("123.456.789-00", encontrada.Cpf);
    }

    [Fact]
    public void BuscarPorCpf_CpfInexistente_RetornaNull()
    {
        var resultado = _repository.BuscarPorCpf("999.999.999-99");

        Assert.Null(resultado);
    }

    [Fact]
    public void ListarTodas_RetornaTodasAsContasAdicionadas()
    {
        _repository.Adicionar(ContaBase("111.111.111-11"));
        _repository.Adicionar(ContaBase("222.222.222-22"));

        var lista = _repository.ListarTodas();

        Assert.Equal(2, lista.Count);
    }

    [Fact]
    public void ListarTodas_SemContas_RetornaListaVazia()
    {
        var lista = _repository.ListarTodas();

        Assert.Empty(lista);
    }
}
