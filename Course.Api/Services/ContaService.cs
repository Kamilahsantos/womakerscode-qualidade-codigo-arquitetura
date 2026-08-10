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

    public ContaService(IContaRepository contaRepository)
    {
        _contaRepository = contaRepository;
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

        return _contaRepository.Adicionar(conta);
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
        return conta;
    }

    public decimal ConsultarSaldo(int id) => BuscarConta(id).Saldo;

    public IReadOnlyList<Conta> ListarContas() => _contaRepository.ListarTodas();
}
