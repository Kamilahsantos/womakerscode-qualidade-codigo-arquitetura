using System.ComponentModel.DataAnnotations;
using Course.Api.Exceptions;

namespace Course.Api.Controllers.Requests;

public class TransferirRequest
{
    [Range(1, int.MaxValue, ErrorMessage = MensagensErro.IdContaInvalido)]
    public int IdOrigem { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = MensagensErro.IdContaInvalido)]
    public int IdDestino { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = MensagensErro.ValorForaDoIntervalo)]
    public decimal Valor { get; set; }
}
