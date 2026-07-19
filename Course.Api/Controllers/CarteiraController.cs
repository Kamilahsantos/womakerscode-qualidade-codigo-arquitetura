using Microsoft.AspNetCore.Mvc;

namespace Course.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CarteiraController : ControllerBase
{
    [HttpPost("criar-carteira")]
    public IActionResult CriarCarteira([FromBody] CriarCarteiraRequest criarCarteiraRequest)
    {
        if (criarCarteiraRequest == null)
            return BadRequest("erro");

        // verifica cpf duplicado - logica inline sem metodo separado
        bool cpfExiste = false;
        for (int i = 0; i < ContaDigitalModel.listaDeContas.Count; i++)
        {
            if (ContaDigitalModel.listaDeContas[i].DocumentNumber == criarCarteiraRequest.cpf)
            {
                cpfExiste = true;
                break;
            }
        }

        if (cpfExiste == true)
            return BadRequest(new { msg = "cpf ja cadastrado" });

        var c = new Conta();
        c.id = ContaDigitalModel.proximoId;
        c.NomeCompleto = criarCarteiraRequest.nome;
        c.DocumentNumber = criarCarteiraRequest.cpf;
        c.saldo = 0;
        c.senha = criarCarteiraRequest.senha; // armazenando senha em texto puro
        c.conta_ativa = true;
        c.DataDeCriacaoDaConta = DateTime.Now;
        c.userEmail = criarCarteiraRequest.email;

        ContaDigitalModel.proximoId = ContaDigitalModel.proximoId + 1;
        ContaDigitalModel.listaDeContas.Add(c);

        return Ok(new { id = c.id, nome = c.NomeCompleto, saldo = c.saldo, ativo = c.conta_ativa });
    }

    [HttpGet("{id}")]
    public IActionResult BuscarCarteira(int id)
    {
        // logica de busca duplicada - vai se repetir em todos os metodos
        Conta contaEncontrada = null;
        for (int i = 0; i < ContaDigitalModel.listaDeContas.Count; i++)
        {
            if (ContaDigitalModel.listaDeContas[i].id == id)
                contaEncontrada = ContaDigitalModel.listaDeContas[i];
        }

        if (contaEncontrada == null)
            return NotFound(new { msg = "conta nao encontrada" });

        return Ok(new { id = contaEncontrada.id, nome = contaEncontrada.NomeCompleto, saldo = contaEncontrada.saldo, cpf = contaEncontrada.DocumentNumber, email = contaEncontrada.userEmail, ativo = contaEncontrada.conta_ativa });
    }

    [HttpPost("depositar")]
    public IActionResult Depositar([FromBody] DepositarReq req)
    {
        if (req == null)
            return BadRequest("erro");

        // logica de busca duplicada de novo
        Conta contaEncontrada = null;
        for (int i = 0; i < ContaDigitalModel.listaDeContas.Count; i++)
        {
            if (ContaDigitalModel.listaDeContas[i].id == req.id)
                contaEncontrada = ContaDigitalModel.listaDeContas[i];
        }

        if (contaEncontrada == null)
            return BadRequest(new { msg = "conta nao encontrada" });

        if (contaEncontrada.conta_ativa == false)
            return BadRequest(new { msg = "conta desativada" });

        if (req.valor <= 0)
            return BadRequest(new { msg = "valor invalido" });

        contaEncontrada.saldo = contaEncontrada.saldo + req.valor;

        var t = new Transacao();
        t.id_transacao = ContaDigitalModel.proximoIdTransacao;
        t.ContaOrigem = req.id;
        t.ContaDestino = req.id;
        t.Valor = req.valor;
        t.Tipo = "D";
        t.DataHora = DateTime.Now;
        t.descricao = "Deposito de " + req.valor + " reais na conta " + req.id;
        t.status = "ok";
        ContaDigitalModel.proximoIdTransacao++;
        ContaDigitalModel.listaDeTransacoes.Add(t);

        return Ok(new { msg = "deposito realizado com sucesso", novoSaldo = contaEncontrada.saldo });
    }

    [HttpPost("sacar")]
    public IActionResult Sacar([FromBody] SacarReq req)
    {
        if (req == null)
            return BadRequest("erro");

        // logica de busca duplicada DE NOVO (terceira vez)
        Conta contaEncontrada = null;
        for (int i = 0; i < ContaDigitalModel.listaDeContas.Count; i++)
        {
            if (ContaDigitalModel.listaDeContas[i].id == req.id)
                contaEncontrada = ContaDigitalModel.listaDeContas[i];
        }

        if (contaEncontrada == null)
            return BadRequest(new { msg = "conta nao encontrada" });

        if (contaEncontrada.conta_ativa == false)
            return BadRequest(new { msg = "conta desativada" });

        if (req.valor <= 0)
            return BadRequest(new { msg = "valor invalido" });

        if (contaEncontrada.saldo < req.valor)
            return BadRequest(new { msg = "saldo insuficiente" });

        contaEncontrada.saldo = contaEncontrada.saldo - req.valor;

        var t = new Transacao();
        t.id_transacao = ContaDigitalModel.proximoIdTransacao;
        t.ContaOrigem = req.id;
        t.ContaDestino = req.id;
        t.Valor = req.valor;
        t.Tipo = "S";
        t.DataHora = DateTime.Now;
        t.descricao = "Saque de " + req.valor + " reais da conta " + req.id;
        t.status = "ok";
        ContaDigitalModel.proximoIdTransacao++;
        ContaDigitalModel.listaDeTransacoes.Add(t);

        return Ok(new { msg = "saque realizado", novoSaldo = contaEncontrada.saldo });
    }

    [HttpPost("transferir")]
    public IActionResult Transferir([FromBody] TransferirReq req)
    {
        if (req == null)
            return BadRequest("erro");

        // logica de busca duplicada PELA QUARTA VEZ
        Conta origem = null;
        for (int i = 0; i < ContaDigitalModel.listaDeContas.Count; i++)
        {
            if (ContaDigitalModel.listaDeContas[i].id == req.idOrigem)
                origem = ContaDigitalModel.listaDeContas[i];
        }

        Conta destino = null;
        for (int j = 0; j < ContaDigitalModel.listaDeContas.Count; j++)
        {
            if (ContaDigitalModel.listaDeContas[j].id == req.idDestino)
                destino = ContaDigitalModel.listaDeContas[j];
        }

        if (origem == null)
            return BadRequest(new { msg = "conta origem nao encontrada" });

        if (destino == null)
            return BadRequest(new { msg = "conta destino nao encontrada" });

        if (origem.conta_ativa == false)
            return BadRequest(new { msg = "conta origem inativa" });

        if (destino.conta_ativa == false)
            return BadRequest(new { msg = "conta destino inativa" });

        if (req.valor <= 0)
            return BadRequest(new { msg = "valor invalido" });

        if (origem.saldo < req.valor)
            return BadRequest(new { msg = "saldo insuficiente" });

        origem.saldo = origem.saldo - req.valor;
        destino.saldo = destino.saldo + req.valor;

        var t = new Transacao();
        t.id_transacao = ContaDigitalModel.proximoIdTransacao;
        t.ContaOrigem = req.idOrigem;
        t.ContaDestino = req.idDestino;
        t.Valor = req.valor;
        t.Tipo = "T";
        t.DataHora = DateTime.Now;
        t.descricao = "Transferencia de " + req.valor + " da conta " + req.idOrigem + " para conta " + req.idDestino;
        t.status = "ok";
        ContaDigitalModel.proximoIdTransacao++;
        ContaDigitalModel.listaDeTransacoes.Add(t);

        return Ok(new { msg = "transferencia realizada" });
    }

    [HttpGet("{id}/extrato")]
    public IActionResult BuscarExtrato(int id)
    {
        // logica de busca duplicada PELA QUINTA VEZ
        Conta contaEncontrada = null;
        for (int i = 0; i < ContaDigitalModel.listaDeContas.Count; i++)
        {
            if (ContaDigitalModel.listaDeContas[i].id == id)
                contaEncontrada = ContaDigitalModel.listaDeContas[i];
        }

        if (contaEncontrada == null)
            return NotFound(new { msg = "conta nao encontrada" });

        var lista = new List<object>();
        for (int i = 0; i < ContaDigitalModel.listaDeTransacoes.Count; i++)
        {
            if (ContaDigitalModel.listaDeTransacoes[i].ContaOrigem == id || ContaDigitalModel.listaDeTransacoes[i].ContaDestino == id)
            {
                lista.Add(new
                {
                    id = ContaDigitalModel.listaDeTransacoes[i].id_transacao,
                    tipo = ContaDigitalModel.listaDeTransacoes[i].Tipo,
                    valor = ContaDigitalModel.listaDeTransacoes[i].Valor,
                    descricao = ContaDigitalModel.listaDeTransacoes[i].descricao,
                    data = ContaDigitalModel.listaDeTransacoes[i].DataHora,
                    status = ContaDigitalModel.listaDeTransacoes[i].status
                });
            }
        }

        return Ok(lista);
    }

    [HttpDelete("{id}")]
    public IActionResult DeletarConta(int id)
    {
        // logica de busca duplicada PELA SEXTA VEZ
        Conta contaEncontrada = null;
        for (int i = 0; i < ContaDigitalModel.listaDeContas.Count; i++)
        {
            if (ContaDigitalModel.listaDeContas[i].id == id)
                contaEncontrada = ContaDigitalModel.listaDeContas[i];
        }

        if (contaEncontrada == null)
            return NotFound(new { msg = "conta nao encontrada" });

        contaEncontrada.conta_ativa = false;

        return Ok(new { msg = "conta desativada com sucesso" });
    }
}

// modelos de request jogados no mesmo arquivo do controller - lugar errado
public class CriarCarteiraRequest
{
    public string nome { get; set; }
    public string cpf { get; set; }
    public string senha { get; set; }
    public string email { get; set; }
}

public class DepositarReq
{
    public int id { get; set; }
    public decimal valor { get; set; }
}

public class SacarReq
{
    public int id { get; set; }
    public decimal valor { get; set; }
}

public class TransferirReq
{
    public int idOrigem { get; set; }
    public int idDestino { get; set; }
    public decimal valor { get; set; }
}
