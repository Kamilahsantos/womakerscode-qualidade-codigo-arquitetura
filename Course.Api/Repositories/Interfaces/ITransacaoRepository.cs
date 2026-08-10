using Course.Api.Models;

namespace Course.Api.Repositories.Interfaces;

public interface ITransacaoRepository
{
    void Adicionar(Transacao transacao);
    IReadOnlyList<Transacao> ListarTodas();
    IReadOnlyList<Transacao> BuscarPorContaId(int contaId);
}
