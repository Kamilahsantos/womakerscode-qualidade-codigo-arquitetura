namespace Course.Api.Controllers.Requests;

public class TransferirRequest
{
    public int IdOrigem { get; set; }
    public int IdDestino { get; set; }
    public decimal Valor { get; set; }
}
