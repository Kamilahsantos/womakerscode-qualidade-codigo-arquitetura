using Course.Api.Controllers.Requests;
using Course.Api.Models;

namespace Course.Api.Services.Interfaces;

public interface IContaService
{
    Conta CriarConta(CriarCarteiraRequest request);
    Conta BuscarConta(int id);
    Conta AtualizarConta(int id, AtualizarContaRequest request);
    Conta DesativarConta(int id);
    decimal ConsultarSaldo(int id);
    IReadOnlyList<Conta> ListarContas();
}
