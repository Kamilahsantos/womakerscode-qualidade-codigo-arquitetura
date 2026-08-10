using Course.Api.Models;
using Course.Api.Repositories.Interfaces;
using Course.Api.Services.Interfaces;

namespace Course.Api.Services;

public class TransacaoService : ITransacaoService
{
    private readonly ITransacaoRepository _transacaoRepository;

    public TransacaoService(ITransacaoRepository transacaoRepository)
    {
        _transacaoRepository = transacaoRepository;
    }

    public IReadOnlyList<Transacao> ListarTransacoes() =>
        _transacaoRepository.ListarTodas();

    public IReadOnlyList<Transacao> BuscarExtrato(int contaId) =>
        _transacaoRepository.BuscarPorContaId(contaId);
}
