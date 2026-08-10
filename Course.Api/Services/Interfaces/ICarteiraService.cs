using Course.Api.Models;

namespace Course.Api.Services.Interfaces;

public interface ICarteiraService
{
    Conta Depositar(int contaId, decimal valor);
    Conta Sacar(int contaId, decimal valor);
    void Transferir(int contaOrigemId, int contaDestinoId, decimal valor);
}
