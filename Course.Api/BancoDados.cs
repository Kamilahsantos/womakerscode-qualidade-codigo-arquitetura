// TODO: mover essas classes para arquivos separados (nunca foi feito)
// banco de dados em memoria - estado global estatico
// validar que na vdd, uma transacao pertence a uma conta, e nao a duas contas diferentes, entao na tabela transacao eu devo ter um id de conta 

public static class ContaDigitalModel
{
    public static List<Conta> listaDeContas = new List<Conta>();
    public static List<Transacao> listaDeTransacoes = new List<Transacao>();
    public static int proximoId = 1;
    public static int proximoIdTransacao = 100;
}

// modelo de conta - nomes inconsistentes proposital
public class Conta
{
    public int id { get; set; }
    public string NomeCompleto { get; set; }
    public string DocumentNumber { get; set; }
    public decimal saldo { get; set; }
    public string senha { get; set; } // senha em texto puro - NUNCA fazer isso
    public bool conta_ativa { get; set; }
    public DateTime DataDeCriacaoDaConta { get; set; }
    public string userEmail { get; set; }
}

// nome da classe completamente ruim
public class Transacao
{
    public int id_transacao { get; set; }
    public int ContaOrigem { get; set; }
    public int ContaDestino { get; set; }
    public decimal Valor { get; set; }
    public string Tipo { get; set; } // D=deposito S=saque T=transferencia
    public DateTime DataHora { get; set; }
    public string descricao { get; set; }
    public string status { get; set; }
}
