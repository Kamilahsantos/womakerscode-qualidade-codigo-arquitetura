namespace Course.Api.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class ContaNaoEncontradaException : DomainException
{
    public ContaNaoEncontradaException(int id) : base($"Conta {id} nao encontrada.") { }
}

public class ContaInativaException : DomainException
{
    public ContaInativaException(int id) : base($"Conta {id} esta desativada.") { }
}

public class CpfJaCadastradoException : DomainException
{
    public CpfJaCadastradoException() : base("CPF ja cadastrado.") { }
}

public class SaldoInsuficienteException : DomainException
{
    public SaldoInsuficienteException() : base("Saldo insuficiente.") { }
}

public class ValorInvalidoException : DomainException
{
    public ValorInvalidoException() : base("Valor deve ser maior que zero.") { }
}
