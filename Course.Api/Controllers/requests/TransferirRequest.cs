namespace Course.Api.Controllers;

public class TransferirReq
{
    public int idOrigem { get; set; }
    public int idDestino { get; set; }
    public decimal valor { get; set; }
}
