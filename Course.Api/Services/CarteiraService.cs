using Course.Api.Exceptions;
using Course.Api.Models;
using Course.Api.Repositories.Interfaces;
using Course.Api.Services.Interfaces;

namespace Course.Api.Services;

public class CarteiraService : ICarteiraService
{
    private readonly IContaRepository _contaRepository;
    private readonly ITransacaoRepository _transacaoRepository;
    private readonly ILogger<CarteiraService> _logger;

    public CarteiraService(IContaRepository contaRepository, ITransacaoRepository transacaoRepository, ILogger<CarteiraService> logger)
    {
        _contaRepository = contaRepository;
        _transacaoRepository = transacaoRepository;
        _logger = logger;
    }

    public Conta Depositar(int contaId, decimal valor)
    {
        if (valor <= 0)
            throw new ValorInvalidoException();

        var conta = BuscarContaAtiva(contaId);
        conta.Saldo += valor;
        _contaRepository.Atualizar(conta);
        _transacaoRepository.Adicionar(CriarTransacao(contaId, contaId, valor, "D", $"Deposito de {valor} na conta {contaId}"));
        _logger.LogInformation("Deposito de {Valor} realizado na conta {ContaId}", valor, contaId);
        return conta;
    }

    public Conta Sacar(int contaId, decimal valor)
    {
        if (valor <= 0)
            throw new ValorInvalidoException();

        var conta = BuscarContaAtiva(contaId);

        if (conta.Saldo < valor)
            throw new SaldoInsuficienteException();

        conta.Saldo -= valor;
        _contaRepository.Atualizar(conta);
        _transacaoRepository.Adicionar(CriarTransacao(contaId, contaId, valor, "S", $"Saque de {valor} da conta {contaId}"));
        _logger.LogInformation("Saque de {Valor} realizado na conta {ContaId}", valor, contaId);
        return conta;
    }

    public void Transferir(int contaOrigemId, int contaDestinoId, decimal valor)
    {
        if (valor <= 0)
            throw new ValorInvalidoException();

        var origem = BuscarContaAtiva(contaOrigemId);
        var destino = BuscarContaAtiva(contaDestinoId);

        if (origem.Saldo < valor)
            throw new SaldoInsuficienteException();

        origem.Saldo -= valor;
        destino.Saldo += valor;
        _contaRepository.Atualizar(origem);
        _contaRepository.Atualizar(destino);
        _transacaoRepository.Adicionar(CriarTransacao(contaOrigemId, contaDestinoId, valor, "T",
            $"Transferencia de {valor} da conta {contaOrigemId} para conta {contaDestinoId}"));
        _logger.LogInformation("Transferencia de {Valor} realizada da conta {ContaOrigemId} para conta {ContaDestinoId}", valor, contaOrigemId, contaDestinoId);
    }

    private Conta BuscarContaAtiva(int id)
    {
        var conta = _contaRepository.BuscarPorId(id);
        if (conta == null)
            throw new ContaNaoEncontradaException(id);
        if (!conta.Ativa)
            throw new ContaInativaException(id);
        return conta;
    }

    private static Transacao CriarTransacao(int origemId, int destinoId, decimal valor, string tipo, string descricao) =>
        new()
        {
            ContaOrigemId = origemId,
            ContaDestinoId = destinoId,
            Valor = valor,
            Tipo = tipo,
            DataHora = DateTime.UtcNow,
            Descricao = descricao,
            Status = "ok"
        };
}