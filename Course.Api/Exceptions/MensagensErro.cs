namespace Course.Api.Exceptions;

// Fonte unica das mensagens de erro da API, evitando strings soltas espalhadas pelo codigo.
public static class MensagensErro
{
    // Erros gerais / infraestrutura
    public const string RequisicaoInvalida = "Requisicao invalida. Verifique o corpo enviado.";
    public const string DadosInvalidos = "Os dados informados sao invalidos. Verifique os campos e tente novamente.";
    public const string ErroInterno = "Ocorreu um erro interno inesperado. Tente novamente mais tarde.";

    // Regras de dominio
    public const string ContaNaoEncontrada = "Conta {0} nao encontrada.";
    public const string ContaInativa = "Conta {0} esta desativada.";
    public const string CpfJaCadastrado = "CPF ja cadastrado.";
    public const string SaldoInsuficiente = "Saldo insuficiente.";
    public const string ValorInvalido = "Valor deve ser maior que zero.";

    // Validacao de campos - Conta / Carteira
    public const string NomeObrigatorio = "O nome e obrigatorio.";
    public const string NomeTamanho = "O nome deve ter entre 3 e 150 caracteres.";
    public const string CpfObrigatorio = "O CPF e obrigatorio.";
    public const string CpfFormatoInvalido = "O CPF deve estar no formato 000.000.000-00 ou conter 11 digitos.";
    public const string SenhaObrigatoria = "A senha e obrigatoria.";
    public const string SenhaTamanhoMinimo = "A senha deve ter no minimo 6 caracteres.";
    public const string EmailObrigatorio = "O email e obrigatorio.";
    public const string EmailFormatoInvalido = "O email informado nao e valido.";

    // Validacao de campos - Transacoes
    public const string IdContaInvalido = "O identificador da conta deve ser maior que zero.";
    public const string ValorForaDoIntervalo = "O valor deve ser maior que zero.";
}
