namespace Course.Api.Models;

// Formato padronizado de resposta de erro devolvido pela API (mantem o campo "msg" ja usado pelos consumidores).
public class ErroResposta
{
    public required string Msg { get; set; }
    public int StatusCode { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Preenchido apenas em erros de validacao, com a lista de mensagens por campo.
    public IDictionary<string, string[]>? Detalhes { get; set; }
}
