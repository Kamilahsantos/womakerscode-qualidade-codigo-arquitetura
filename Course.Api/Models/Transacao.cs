namespace Course.Api.Models;

public class Transacao
{
    public int Id { get; set; }
    public int ContaOrigemId { get; set; }
    public int ContaDestinoId { get; set; }
    public decimal Valor { get; set; }
    public required string Tipo { get; set; }
    public DateTime DataHora { get; set; }
    public required string Descricao { get; set; }
    public required string Status { get; set; }
}
