using Course.Api.Models;
using Course.Api.Repositories.Interfaces;

namespace Course.Api.Repositories;

public class ContaRepository : IContaRepository
{
    private readonly List<Conta> _contas = new();
    private int _proximoId = 1;

    public Conta? BuscarPorId(int id) =>
        _contas.FirstOrDefault(c => c.Id == id);

    public Conta? BuscarPorCpf(string cpf) =>
        _contas.FirstOrDefault(c => c.Cpf == cpf);

    public IReadOnlyList<Conta> ListarTodas() => _contas.AsReadOnly();

    public Conta Adicionar(Conta conta)
    {
        conta.Id = _proximoId++;
        _contas.Add(conta);
        return conta;
    }

    public void Atualizar(Conta conta) { }
}
