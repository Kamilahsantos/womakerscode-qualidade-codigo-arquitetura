using Microsoft.AspNetCore.Mvc;

namespace Course.Api.Controllers;

// TODO: UNIFICAR AS CONTROLLERS
[ApiController]
[Route("[controller]")]
public class ContaController : ControllerBase
{
    [HttpGet("todas")]
    public IActionResult listarContas()
    {

        //TODO: criar metodo em outra service para listar contas, pois ele é usado em 2 metodos diferentes
        var lista = new List<object>();
        for (int x = 0; x < ContaDigitalModel.listaDeContas.Count; x++)
        {
            var cc = ContaDigitalModel.listaDeContas[x];
            // expondo senha em texto puro na resposta - NUNCA fazer isso
            lista.Add(new { id = cc.id, nome = cc.NomeCompleto, cpf = cc.DocumentNumber, saldo = cc.saldo, ativo = cc.conta_ativa, email = cc.userEmail, senha = cc.senha });
        }
        return Ok(lista);
    }

    [HttpGet("saldo/{id}")]
    public IActionResult GetSaldo(int id)
    {
        // TODO: logica de busca duplicada do CarteiraController
        Conta c = null;
        for (int i = 0; i < ContaDigitalModel.listaDeContas.Count; i++)
        {
            if (ContaDigitalModel.listaDeContas[i].id == id)
                c = ContaDigitalModel.listaDeContas[i];
        }

        if (c == null) return NotFound("conta nao existe");

        return Ok(new { saldo = c.saldo, id = id });
    }

    [HttpPut("atualizar/{id}")]
    public IActionResult AtualizarConta(int id, [FromBody] AtualizarContaReq req)
    {
        if (req == null) return BadRequest();

        // TODO: logica de busca duplicada OUTRA VEZ
        Conta c = null;
        for (int i = 0; i < ContaDigitalModel.listaDeContas.Count; i++)
        {
            if (ContaDigitalModel.listaDeContas[i].id == id)
                c = ContaDigitalModel.listaDeContas[i];
        }

        if (c == null) return NotFound("conta nao existe");

        c.NomeCompleto = req.novoNome;
        c.userEmail = req.novoEmail;
        c.senha = req.novaSenha; // ainda em texto puro!

        return Ok(new { msg = "dados atualizados", id = c.id, nome = c.NomeCompleto });
    }

//TODO: DESABILITAR ESSE ENDPOINT, POIS ELE RETORNA TODAS AS TRANSACOES DE TODOS OS USUARIOS SEM AUTENTICACAO NENHUMA
    [HttpGet("historico-transacoes")]
    public IActionResult GetTodasTransacoes()
    {
        // retorna TODAS as transacoes de TODOS os usuarios sem autenticacao nenhuma
        var lista = new List<object>();
        for (int i = 0; i < ContaDigitalModel.listaDeTransacoes.Count; i++)
        {
            var t = ContaDigitalModel.listaDeTransacoes[i];
            lista.Add(new { id = t.id_transacao, origem = t.ContaOrigem, destino = t.ContaDestino, valor = t.Valor, tipo = t.Tipo, data = t.DataHora, status = t.status });
        }
        return Ok(lista);
    }
}

//TODO:  mais uma request model no lugar errado - deveria estar em pasta Models separada
public class AtualizarContaReq
{
    public string novoNome { get; set; }
    public string novoEmail { get; set; }
    public string novaSenha { get; set; }
}
