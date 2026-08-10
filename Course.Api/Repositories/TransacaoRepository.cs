using Course.Api.Models;
using Course.Api.Repositories.Interfaces;

namespace Course.Api.Repositories;

public class TransacaoRepository : ITransacaoRepository
{
    private readonly List<Transacao> _transacoes = new();
    private int _proximoId = 100;

    public void Adicionar(Transacao transacao)
    {
        transacao.Id = _proximoId++;
        _transacoes.Add(transacao);
    }

    public IReadOnlyList<Transacao> ListarTodas() => _transacoes.AsReadOnly();

    public IReadOnlyList<Transacao> BuscarPorContaId(int contaId) =>
        _transacoes
            .Where(t => t.ContaOrigemId == contaId || t.ContaDestinoId == contaId)
            .ToList()
            .AsReadOnly();
}
