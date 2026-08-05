namespace Course.Api.Controllers;

// modelos de request jogados no mesmo arquivo do controller - lugar errado
public class CriarCarteiraRequest
{
    public string nome { get; set; }
    public string cpf { get; set; }
    public string senha { get; set; }
    public string email { get; set; }
}
