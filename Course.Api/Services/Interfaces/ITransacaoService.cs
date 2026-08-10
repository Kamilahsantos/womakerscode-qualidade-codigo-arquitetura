using Course.Api.Models;

namespace Course.Api.Services.Interfaces;

public interface ITransacaoService
{
    IReadOnlyList<Transacao> ListarTransacoes();
    IReadOnlyList<Transacao> BuscarExtrato(int contaId);
}
