# Refatoração: Princípios SOLID e Boas Práticas

## Estrutura Final

```
Course.Api/
  Models/              → Entidades de domínio
  Exceptions/          → Exceções de domínio
  Repositories/
    Interfaces/        → Contratos de acesso a dados
    Implementations/   → Implementações in-memory
  Services/
    Interfaces/        → Contratos de negócio
    Implementations/   → Lógica de negócio
  Utils/               → Utilitários estáticos
  Middleware/          → Tratamento global de erros
  Controllers/
    requests/          → DTOs de entrada

Course.Api.Tests/
  Repositories/        → Testes de repositório
  Services/            → Testes de serviço (com Moq)
```

---

## S — Single Responsibility Principle

> *"Uma classe deve ter apenas um motivo para mudar."*

### Antes

`CarteiraController` acumulava **todas** as responsabilidades: buscar conta, validar saldo, registrar transação, montar resposta HTTP. O mesmo loop de busca de conta estava duplicado 6 vezes dentro dos controllers.

```csharp
// Antes: controller com lógica de negócio, acesso a dados e HTTP misturados
[HttpPost("sacar")]
public IActionResult Sacar([FromBody] SacarReq req)
{
    Conta contaEncontrada = null;
    for (int i = 0; i < ContaDigitalModel.listaDeContas.Count; i++) // acesso a dados
        if (ContaDigitalModel.listaDeContas[i].id == req.id)
            contaEncontrada = ContaDigitalModel.listaDeContas[i];

    if (contaEncontrada == null) return BadRequest(...);        // validação
    if (contaEncontrada.saldo < req.valor) return BadRequest(... // regra de negócio
    contaEncontrada.saldo -= req.valor;                         // mutação de estado
}
```

### Depois

Cada camada tem **um único motivo para mudar**:

| Classe | Responsabilidade |
|---|---|
| `CarteiraController` | Traduzir requisição HTTP → chamar serviço → resposta HTTP |
| `CarteiraService` | Regras de negócio (validar saldo, executar operação) |
| `ContaRepository` | Acesso e persistência dos dados de conta |
| `TransacaoRepository` | Acesso e persistência de transações |
| `ExceptionHandlerMiddleware` | Mapear exceções de domínio para status HTTP |
| `PasswordUtils` | Hashing de senha |

---

## O — Open/Closed Principle

> *"Aberto para extensão, fechado para modificação."*

### Antes

`BancoDados.cs` expunha listas estáticas globais diretamente. Qualquer mudança de fonte de dados exigiria alterar todos os controllers.

```csharp
// Antes: acesso direto ao "banco" em qualquer lugar do código
ContaDigitalModel.listaDeContas.Add(c);
ContaDigitalModel.proximoId++;
```

### Depois

O `CarteiraService` depende de `IContaRepository` — uma abstração. Para trocar de in-memory para um banco SQL, basta criar `SqlContaRepository : IContaRepository` e registrar no DI. **Nenhuma linha de serviço ou controller precisa mudar.**

```csharp
// Depois: depende da interface, não da implementação
public class CarteiraService : ICarteiraService
{
    private readonly IContaRepository _contaRepository; // ← abstração
}
```

---

## L — Liskov Substitution Principle

> *"Subtipos devem poder substituir seus tipos base sem quebrar o comportamento."*

### Aplicação

A hierarquia de exceções respeita LSP: qualquer `DomainException` pode ser tratada pelo middleware como um erro de domínio, independente da subclasse concreta.

```csharp
// Hierarquia de exceções
public class DomainException : Exception { }
public class ContaNaoEncontradaException : DomainException { }
public class SaldoInsuficienteException   : DomainException { }
public class ValorInvalidoException       : DomainException { }

// Middleware trata qualquer DomainException corretamente
catch (ContaNaoEncontradaException ex) → 404
catch (DomainException ex)             → 400
```

Isso é LSP: o `catch (DomainException)` funciona corretamente com qualquer subclasse.

---

## I — Interface Segregation Principle

> *"Clientes não devem ser forçados a depender de interfaces que não usam."*

### Antes

Uma única interface `ICarteiraService` com métodos de validação, criação de conta, busca, depósito — tudo junto. O controller era obrigado a depender de métodos que não usava.

```csharp
// Antes: interface "gorda" com responsabilidades misturadas
public interface ICarteiraService
{
    (bool, IActionResult) validaRequest(CriarCarteiraRequest req);
    (bool, IActionResult) validaCpf(CriarCarteiraRequest req);
    (bool, IActionResult) validaDeposito(DepositarReq req);
    (bool, IActionResult?) BuscarContaPorId(DepositarReq req, out Conta? conta);
    Conta criaConta(CriarCarteiraRequest req);
}
```

### Depois

Três interfaces enxutas, cada uma com coesão total:

```csharp
public interface IContaService    // CRUD de conta
{
    Conta CriarConta(CriarCarteiraRequest request);
    Conta BuscarConta(int id);
    Conta AtualizarConta(int id, AtualizarContaRequest request);
    Conta DesativarConta(int id);
    decimal ConsultarSaldo(int id);
    IReadOnlyList<Conta> ListarContas();
}

public interface ICarteiraService  // Operações financeiras
{
    Conta Depositar(int contaId, decimal valor);
    Conta Sacar(int contaId, decimal valor);
    void Transferir(int contaOrigemId, int contaDestinoId, decimal valor);
}

public interface ITransacaoService // Consulta de transações
{
    IReadOnlyList<Transacao> ListarTransacoes();
    IReadOnlyList<Transacao> BuscarExtrato(int contaId);
}
```

`ContaController` só injeta `IContaService` e `ITransacaoService`. `CarteiraController` injeta `ICarteiraService` e `IContaService`. Nenhum controller conhece métodos que não usa.

---

## D — Dependency Inversion Principle

> *"Módulos de alto nível não devem depender de módulos de baixo nível. Ambos devem depender de abstrações."*

### Antes

Controllers e a service acessavam `ContaDigitalModel` diretamente — um módulo de baixo nível (implementação de dados) invadindo a camada de negócio.

```csharp
// Antes: alto nível (service) dependendo de baixo nível (implementação estática)
public class CarteiraService
{
    public Conta criaConta(...) {
        ContaDigitalModel.listaDeContas.Add(c); // acoplamento concreto
        ContaDigitalModel.proximoId++;
    }
}
```

### Depois

Services dependem de interfaces de repositório, injetadas pelo container de DI. O `Program.cs` é o único lugar que conhece as implementações concretas.

```csharp
// Depois: alto nível depende de abstração
public class ContaService : IContaService
{
    private readonly IContaRepository _contaRepository; // abstração

    public ContaService(IContaRepository contaRepository) // injeção pelo DI
        => _contaRepository = contaRepository;
}

// Program.cs — único ponto de configuração de implementações concretas
builder.Services.AddSingleton<IContaRepository, ContaRepository>();
builder.Services.AddScoped<IContaService, ContaService>();
```

---

## Outras Boas Práticas Aplicadas

### Separação de Domínio e HTTP

**Antes:** Services retornavam `IActionResult` — a camada de negócio conhecia conceitos HTTP.

```csharp
// Antes: service retornando resposta HTTP — violação de separação de camadas
(bool flowControl, IActionResult value) validaDeposito(DepositarReq req);
```

**Depois:** Services lançam exceções de domínio. O `ExceptionHandlerMiddleware` faz a tradução para HTTP em um único lugar.

```csharp
// Depois: service fala apenas domínio
throw new SaldoInsuficienteException();

// Middleware faz a tradução
catch (DomainException ex) {
    context.Response.StatusCode = 400;
    await context.Response.WriteAsJsonAsync(new { msg = ex.Message });
}
```

### Remoção de Estado Global

**Antes:** `ContaDigitalModel` com campos `static` compartilhados em toda a aplicação — perigoso, impossível de testar de forma isolada.

**Depois:** Repositórios usam campos de **instância**. O comportamento singleton em produção vem do registro `AddSingleton` no DI, não de `static`. Isso permite que testes criem instâncias isoladas sem vazamento de estado entre casos de teste.

### Segurança (OWASP A02 — Cryptographic Failures)

**Antes:** Senha armazenada e exposta em texto puro.

```csharp
c.senha = criarCarteiraRequest.senha; // texto puro
lista.Add(new { senha = cc.senha }); // exposta na API
```

**Depois:** Hash via `PasswordUtils`. Nenhum endpoint expõe senha.

```csharp
SenhaHash = PasswordUtils.HashSenha(request.Senha); // SHA-256
// resposta da listagem não inclui campo de senha
```

### Nomenclatura Consistente

Todos os modelos e propriedades foram padronizados para **PascalCase** conforme convenção C#, eliminando a mistura de `id`, `NomeCompleto`, `conta_ativa`, `userEmail` que existia no código original.

---

## Testes Unitários

A arquitetura desacoplada viabilizou os testes. **46 testes** cobrindo cenários de sucesso e erro:

- **Repositories:** testados com instâncias reais (sem mock) — validam a lógica de armazenamento in-memory de forma isolada.
- **Services:** testados com **Moq** para isolar dependências de repositório — validam exclusivamente a lógica de negócio.

```csharp
// Exemplo: teste que só existiu possível pela injeção de dependência
var _contaRepoMock = new Mock<IContaRepository>();
_contaRepoMock.Setup(r => r.BuscarPorId(1)).Returns(ContaInativa());

Assert.Throws<ContaInativaException>(() => _service.Depositar(1, 100));
```
