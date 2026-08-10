namespace Course.Api.Models;

public class Conta
{
    public int Id { get; set; }
    public required string NomeCompleto { get; set; }
    public required string Cpf { get; set; }
    public decimal Saldo { get; set; }
    public required string SenhaHash { get; set; }
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public required string Email { get; set; }
}
