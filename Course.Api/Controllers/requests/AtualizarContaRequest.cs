namespace Course.Api.Controllers.Requests;

public class AtualizarContaRequest
{
    public required string NovoNome { get; set; }
    public required string NovoEmail { get; set; }
    public required string NovaSenha { get; set; }
}
