using Course.Api.Models;

namespace Course.Api.Repositories.Interfaces;

public interface IContaRepository
{
    Conta? BuscarPorId(int id);
    Conta? BuscarPorCpf(string cpf);
    IReadOnlyList<Conta> ListarTodas();
    Conta Adicionar(Conta conta);
    void Atualizar(Conta conta);
}
