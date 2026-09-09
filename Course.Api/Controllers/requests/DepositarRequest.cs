using System.ComponentModel.DataAnnotations;
using Course.Api.Exceptions;

namespace Course.Api.Controllers.Requests;

public class DepositarRequest
{
    [Range(1, int.MaxValue, ErrorMessage = MensagensErro.IdContaInvalido)]
    public int Id { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = MensagensErro.ValorForaDoIntervalo)]
    public decimal Valor { get; set; }
}
