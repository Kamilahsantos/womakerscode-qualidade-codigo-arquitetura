using System.ComponentModel.DataAnnotations;
using Course.Api.Controllers.Requests;
using Course.Api.Exceptions;
using Course.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ContaController : ControllerBase
{
    private readonly IContaService _contaService;
    private readonly ITransacaoService _transacaoService;

    public ContaController(IContaService contaService, ITransacaoService transacaoService)
    {
        _contaService = contaService;
        _transacaoService = transacaoService;
    }

    [HttpGet("todas")]
    public IActionResult ListarContas()
    {
        var contas = _contaService.ListarContas();
        var lista = contas.Select(c => new { id = c.Id, nome = c.NomeCompleto, cpf = c.Cpf, saldo = c.Saldo, ativo = c.Ativa, email = c.Email });
        return Ok(lista);
    }

    [HttpGet("saldo/{id}")]
    public IActionResult ConsultarSaldo([Range(1, int.MaxValue, ErrorMessage = MensagensErro.IdContaInvalido)] int id)
    {
        var saldo = _contaService.ConsultarSaldo(id);
        return Ok(new { saldo, id });
    }

    [HttpPut("atualizar/{id}")]
    public IActionResult AtualizarConta([Range(1, int.MaxValue, ErrorMessage = MensagensErro.IdContaInvalido)] int id, [FromBody] AtualizarContaRequest request)
    {
        if (request == null)
            return BadRequest(new { msg = MensagensErro.RequisicaoInvalida });

        var conta = _contaService.AtualizarConta(id, request);
        return Ok(new { msg = "dados atualizados", id = conta.Id, nome = conta.NomeCompleto });
    }

    [HttpGet("historico-transacoes")]
    public IActionResult ListarTransacoes()
    {
        var transacoes = _transacaoService.ListarTransacoes();
        var lista = transacoes.Select(t => new { id = t.Id, origem = t.ContaOrigemId, destino = t.ContaDestinoId, valor = t.Valor, tipo = t.Tipo, data = t.DataHora, status = t.Status });
        return Ok(lista);
    }
}
