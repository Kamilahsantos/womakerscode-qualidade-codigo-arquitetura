namespace Course.Api.Controllers.Requests;

public class CriarCarteiraRequest
{
    public required string Nome { get; set; }
    public required string Cpf { get; set; }
    public required string Senha { get; set; }
    public required string Email { get; set; }
}
