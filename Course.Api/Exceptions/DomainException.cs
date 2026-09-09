namespace Course.Api.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class ContaNaoEncontradaException : DomainException
{
    public ContaNaoEncontradaException(int id) : base(string.Format(MensagensErro.ContaNaoEncontrada, id)) { }
}

public class ContaInativaException : DomainException
{
    public ContaInativaException(int id) : base(string.Format(MensagensErro.ContaInativa, id)) { }
}

public class CpfJaCadastradoException : DomainException
{
    public CpfJaCadastradoException() : base(MensagensErro.CpfJaCadastrado) { }
}

public class SaldoInsuficienteException : DomainException
{
    public SaldoInsuficienteException() : base(MensagensErro.SaldoInsuficiente) { }
}

public class ValorInvalidoException : DomainException
{
    public ValorInvalidoException() : base(MensagensErro.ValorInvalido) { }
}
