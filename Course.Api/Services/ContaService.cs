using Course.Api.Controllers.Requests;
using Course.Api.Exceptions;
using Course.Api.Models;
using Course.Api.Repositories.Interfaces;
using Course.Api.Services.Interfaces;
using Course.Api.Utils;

namespace Course.Api.Services;

public class ContaService : IContaService
{
    private readonly IContaRepository _contaRepository;
    private readonly ILogger<ContaService> _logger;

    public ContaService(IContaRepository contaRepository, ILogger<ContaService> logger)
    {
        _contaRepository = contaRepository;
        _logger = logger;
    }

    public Conta CriarConta(CriarCarteiraRequest request)
    {
        if (_contaRepository.BuscarPorCpf(request.Cpf) != null)
            throw new CpfJaCadastradoException();

        var conta = new Conta
        {
            NomeCompleto = request.Nome,
            Cpf = request.Cpf,
            SenhaHash = PasswordUtils.HashSenha(request.Senha),
            Email = request.Email,
            Saldo = 0,
            Ativa = true,
            DataCriacao = DateTime.UtcNow
        };

        conta = _contaRepository.Adicionar(conta);
        _logger.LogInformation("Conta {ContaId} criada com sucesso", conta.Id);
        return conta;
    }

    public Conta BuscarConta(int id)
    {
        var conta = _contaRepository.BuscarPorId(id);
        if (conta == null)
            throw new ContaNaoEncontradaException(id);
        return conta;
    }

    public Conta AtualizarConta(int id, AtualizarContaRequest request)
    {
        var conta = BuscarConta(id);
        conta.NomeCompleto = request.NovoNome;
        conta.Email = request.NovoEmail;
        conta.SenhaHash = PasswordUtils.HashSenha(request.NovaSenha);
        _contaRepository.Atualizar(conta);
        return conta;
    }

    public Conta DesativarConta(int id)
    {
        var conta = BuscarConta(id);
        conta.Ativa = false;
        _contaRepository.Atualizar(conta);
        _logger.LogInformation("Conta {ContaId} desativada", id);
        return conta;
    }

    public decimal ConsultarSaldo(int id) => BuscarConta(id).Saldo;

    public IReadOnlyList<Conta> ListarContas() => _contaRepository.ListarTodas();
}
