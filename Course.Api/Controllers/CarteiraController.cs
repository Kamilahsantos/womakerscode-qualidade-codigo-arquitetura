using System.ComponentModel.DataAnnotations;
using Course.Api.Controllers.Requests;
using Course.Api.Exceptions;
using Course.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CarteiraController : ControllerBase
{
    private readonly IContaService _contaService;
    private readonly ICarteiraService _carteiraService;
    private readonly ITransacaoService _transacaoService;

    public CarteiraController(IContaService contaService, ICarteiraService carteiraService, ITransacaoService transacaoService)
    {
        _contaService = contaService;
        _carteiraService = carteiraService;
        _transacaoService = transacaoService;
    }

    [HttpPost("criar-carteira")]
    public IActionResult CriarCarteira([FromBody] CriarCarteiraRequest request)
    {
        if (request == null)
            return BadRequest(new { msg = MensagensErro.RequisicaoInvalida });

        var conta = _contaService.CriarConta(request);
        return Ok(new { id = conta.Id, nome = conta.NomeCompleto, saldo = conta.Saldo, ativo = conta.Ativa });
    }

    [HttpGet("{id}")]
    public IActionResult BuscarCarteira([Range(1, int.MaxValue, ErrorMessage = MensagensErro.IdContaInvalido)] int id)
    {
        var conta = _contaService.BuscarConta(id);
        return Ok(new { id = conta.Id, nome = conta.NomeCompleto, saldo = conta.Saldo, cpf = conta.Cpf, email = conta.Email, ativo = conta.Ativa });
    }

    [HttpPost("depositar")]
    public IActionResult Depositar([FromBody] DepositarRequest request)
    {
        if (request == null)
            return BadRequest(new { msg = MensagensErro.RequisicaoInvalida });

        var conta = _carteiraService.Depositar(request.Id, request.Valor);
        return Ok(new { msg = "deposito realizado com sucesso", novoSaldo = conta.Saldo });
    }

    [HttpPost("sacar")]
    public IActionResult Sacar([FromBody] SacarRequest request)
    {
        if (request == null)
            return BadRequest(new { msg = MensagensErro.RequisicaoInvalida });

        var conta = _carteiraService.Sacar(request.Id, request.Valor);
        return Ok(new { msg = "saque realizado", novoSaldo = conta.Saldo });
    }

    [HttpPost("transferir")]
    public IActionResult Transferir([FromBody] TransferirRequest request)
    {
        if (request == null)
            return BadRequest(new { msg = MensagensErro.RequisicaoInvalida });

        _carteiraService.Transferir(request.IdOrigem, request.IdDestino, request.Valor);
        return Ok(new { msg = "transferencia realizada" });
    }

    [HttpGet("{id}/extrato")]
    public IActionResult BuscarExtrato([Range(1, int.MaxValue, ErrorMessage = MensagensErro.IdContaInvalido)] int id)
    {
        _contaService.BuscarConta(id);
        var transacoes = _transacaoService.BuscarExtrato(id);
        var lista = transacoes.Select(t => new { id = t.Id, tipo = t.Tipo, valor = t.Valor, descricao = t.Descricao, data = t.DataHora, status = t.Status });
        return Ok(lista);
    }

    [HttpDelete("{id}")]
    public IActionResult DeletarConta([Range(1, int.MaxValue, ErrorMessage = MensagensErro.IdContaInvalido)] int id)
    {
        _contaService.DesativarConta(id);
        return Ok(new { msg = "conta desativada com sucesso" });
    }
}
