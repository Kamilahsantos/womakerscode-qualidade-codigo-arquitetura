# Separação de Responsabilidades Aplicada no Projeto

Este documento explica a separação de responsabilidades aplicada no projeto e o fluxo entre Controller, Service e Repository, mostrando onde cada camada atua e o que muda na arquitetura da aplicação.

---

## 1. Visão Geral do Fluxo

O projeto foi organizado em camadas com responsabilidades bem definidas:

- `Controller`: recebe a requisição HTTP e encaminha para a lógica de negócio.
- `Service`: executa a regra de negócio e orquestra a operação.
- `Repository`: acessa e persiste os dados.

Esse fluxo deixa a aplicação mais organizada, fácil de testar e menos acoplada.

---

## 2. Controller: Camada de Entrada

### Onde está aplicado
- `Controllers/ContaController.cs`
- `Controllers/CarteiraController.cs`

### O que ele faz
O controller é responsável por:
- receber os dados enviados pelo cliente;
- validar o formato da requisição;
- chamar o serviço correto;
- transformar o resultado em resposta HTTP.

### Exemplo de responsabilidade
O controller não decide regras de negócio, como saldo suficiente, criação de conta ou cálculo de transação. Ele apenas delega essa responsabilidade para a service.

```csharp
[HttpPost("depositar")]
public IActionResult Depositar([FromBody] DepositarRequest request)
{
    var conta = _carteiraService.Depositar(request.ContaId, request.Valor);
    return Ok(conta);
}
```

### O que mudou
Antes, uma classe de controller poderia acumular:
- acesso direto aos dados;
- validações de regra de negócio;
- manipulação de entidades;
- respostas HTTP.

Com a separação, o controller vira uma camada fina, com foco em comunicação com o cliente.

---

## 3. Service: Camada de Regras de Negócio

### Onde está aplicado
- `Services/ContaService.cs`
- `Services/CarteiraService.cs`
- `Services/TransacaoService.cs`
- Interfaces em `Services/Interfaces/`

### O que ele faz
A service concentra a lógica do domínio, por exemplo:
- verificar se a conta existe;
- validar valores de depósito, saque e transferência;
- controlar saldo;
- coordinar transações;
- decidir quando lançar exceções de domínio.

### Exemplo de responsabilidade
A lógica de negócio fica em um lugar centralizado, em vez de estar espalhada pelos controllers.

```csharp
public class CarteiraService : ICarteiraService
{
    private readonly IContaRepository _contaRepository;
    private readonly ITransacaoRepository _transacaoRepository;

    public CarteiraService(IContaRepository contaRepository, ITransacaoRepository transacaoRepository)
    {
        _contaRepository = contaRepository;
        _transacaoRepository = transacaoRepository;
    }

    public Conta Sacar(int contaId, decimal valor)
    {
        var conta = _contaRepository.BuscarPorId(contaId);

        if (conta == null)
            throw new ContaNaoEncontradaException();

        if (conta.Saldo < valor)
            throw new SaldoInsuficienteException();

        conta.Saldo -= valor;
        return _contaRepository.Atualizar(conta);
    }
}
```

### O que mudou
A regra passou a ser:
- Controller coleta a entrada;
- Service decide o comportamento;
- Repository apenas salva e consulta dados.

Isso reduz duplicação, facilita manutenção e deixa a regra mais consistente.

---

## 4. Repository: Camada de Persistência

### Onde está aplicado
- `Repositories/ContaRepository.cs`
- `Repositories/TransacaoRepository.cs`
- Interfaces em `Repositories/Interfaces/`

### O que ele faz
O repository é responsável por:
- buscar dados;
- salvar dados;
- atualizar registros;
- listar informações;
- abstrair a fonte de dados da lógica de negócio.

### Exemplo de responsabilidade
A service chama o repository para acessar detalhes da conta e registrar transações, sem se preocupar com a implementação interna.

```csharp
public interface IContaRepository
{
    Conta BuscarPorId(int id);
    Conta Salvar(Conta conta);
    Conta Atualizar(Conta conta);
}
```

### O que mudou
A service deixa de conhecer detalhes de armazenamento. Ela fala apenas com a interface, e a implementação concreta fica isolada no repository.

Isso é importante porque:
- a regra de negócio não depende de como os dados são guardados;
- a implementação pode mudar sem mexer na service;
- a aplicação fica mais testável.

---

## 5. Fluxo Completo de uma Operação

### Cenário: depósito em conta

1. O cliente envia uma requisição para o endpoint de depósito.
2. O `CarteiraController` recebe a requisição e chama o `ICarteiraService`.
3. A `CarteiraService` valida a conta, o valor e a regra de negócio.
4. A service chama o `IContaRepository` para buscar e atualizar a conta.
5. A `TransacaoRepository` pode registrar a transação associada.
6. O controller devolve a resposta HTTP ao cliente.

### Fluxo resumido

```text
Cliente
  ↓
Controller
  ↓
Service
  ↓
Repository
  ↓
Dados / Persistência
```

Esse fluxo mantém as responsabilidades separadas e evita misturar “entrada HTTP”, “regra de negócio” e “acesso ao banco” em uma mesma classe.

---

## 6. Onde isso está aplicado no projeto

### Controller
- `ContaController` e `CarteiraController`
- Recebem requisições e devolvem respostas HTTP.

### Service
- `ContaService`, `CarteiraService`, `TransacaoService`
- Contêm a lógica de negócio e coordenação entre as operações.

### Repository
- `ContaRepository` e `TransacaoRepository`
- Encapsulam acesso aos dados.

### Interfaces
- `IContaRepository`, `ITransacaoRepository`
- `IContaService`, `ICarteiraService`, `ITransacaoService`
- Definem contratos e deixam o código desacoplado.

---

## 7. O que muda na prática

### Sem separação de responsabilidades
A lógica ficaria espalhada em vários pontos, por exemplo:
- controller validando saldo;
- controller buscando conta;
- controller alterando estado;
- controller persisting dados;
- controller montando a resposta.

Isso gera:
- acoplamento;
- duplicação de código;
- dificuldade de manter e testar;
- alta chance de regressões.

### Com separação de responsabilidades
A estrutura fica assim:
- Controller: fala com o cliente;
- Service: aplica as regras;
- Repository: guarda e recupera dados.

Isso traz:
- código mais limpo;
- maior organização;
- melhor reuso;
- menor acoplamento;
- testes mais simples;
- manutenção mais segura.

---

## 8. Benefício arquitetural dessa organização

Essa separação torna o projeto mais alinhado com boas práticas de arquitetura e com princípios como:
- responsabilidade única;
- inversão de dependência;
- baixo acoplamento;
- camada de domínio isolada da infraestrutura.

Em resumo, o projeto deixa claro que a camada web não deve decidir regras de negócio, e a camada de persistência não deve conter lógica financeira ou de domínio.

---

## Conclusão

A separação de responsabilidades aplicada no projeto funciona como uma estrutura limpa e eficiente:

- `Controller` recebe e responde.
- `Service` decide e executa a regra de negócio.
- `Repository` acessa os dados.

Essa divisão cria um fluxo mais claro, reduz acoplamento e melhora a qualidade do código, além de facilitar evoluções futuras do sistema.
