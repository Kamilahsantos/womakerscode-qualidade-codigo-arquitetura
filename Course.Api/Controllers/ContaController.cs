using Microsoft.AspNetCore.Mvc;

namespace Course.Api.Controllers;

// segundo controller que duplica logica do CarteiraController - nao deveria existir
[ApiController]
[Route("[controller]")]
public class ContaController : ControllerBase
{
    [HttpGet("todas")]
    public IActionResult GetTodasContas()
    {
        var lista = new List<object>();
        for (int x = 0; x < BancoDados.listaDeContas.Count; x++)
        {
            var cc = BancoDados.listaDeContas[x];
            // expondo senha em texto puro na resposta - NUNCA fazer isso
            lista.Add(new { id = cc.id, nome = cc.Nome_Completo_Do_Usuario, cpf = cc.cpf, saldo = cc.saldo, ativo = cc.conta_ativa, email = cc.email_do_usuario, senha = cc.senha });
        }
        return Ok(lista);
    }

    [HttpGet("saldo/{id}")]
    public IActionResult GetSaldo(int id)
    {
        // logica de busca duplicada do CarteiraController
        Conta c = null;
        for (int i = 0; i < BancoDados.listaDeContas.Count; i++)
        {
            if (BancoDados.listaDeContas[i].id == id)
                c = BancoDados.listaDeContas[i];
        }

        if (c == null) return NotFound("conta nao existe");

        return Ok(new { saldo = c.saldo, id = id });
    }

    [HttpPut("atualizar/{id}")]
    public IActionResult AtualizarConta(int id, [FromBody] AtualizarContaReq req)
    {
        if (req == null) return BadRequest();

        // logica de busca duplicada OUTRA VEZ
        Conta c = null;
        for (int i = 0; i < BancoDados.listaDeContas.Count; i++)
        {
            if (BancoDados.listaDeContas[i].id == id)
                c = BancoDados.listaDeContas[i];
        }

        if (c == null) return NotFound("conta nao existe");

        c.Nome_Completo_Do_Usuario = req.novoNome;
        c.email_do_usuario = req.novoEmail;
        c.senha = req.novaSenha; // ainda em texto puro!

        return Ok(new { msg = "dados atualizados", id = c.id, nome = c.Nome_Completo_Do_Usuario });
    }

    [HttpGet("historico-transacoes")]
    public IActionResult GetTodasTransacoes()
    {
        // retorna TODAS as transacoes de TODOS os usuarios sem autenticacao nenhuma
        var lista = new List<object>();
        for (int i = 0; i < BancoDados.listaDeTransacoes.Count; i++)
        {
            var t = BancoDados.listaDeTransacoes[i];
            lista.Add(new { id = t.id_transacao, origem = t.ContaOrigem, destino = t.ContaDestino, valor = t.Valor, tipo = t.Tipo, data = t.DataHora, status = t.status });
        }
        return Ok(lista);
    }
}

// mais uma request model no lugar errado - deveria estar em pasta Models separada
public class AtualizarContaReq
{
    public string novoNome { get; set; }
    public string novoEmail { get; set; }
    public string novaSenha { get; set; }
}
