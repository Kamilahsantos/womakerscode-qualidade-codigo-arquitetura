# Tratamento de Exceções, Validação, Mensagens de Erro e Logs

Este documento descreve as melhorias implementadas na API relacionadas a **tratamento de
exceções**, **validação de entradas**, **organização de mensagens de erro** e
**logs/monitoramento**, sem alterar as regras de negócio existentes. Também traz exemplos
de boas práticas aplicadas e recomendações para evolução futura.

---

## 1. Tratamento de exceções

### O que foi feito
- O [ExceptionHandlerMiddleware](Course.Api/Middleware/ExceptionHandlerMiddleware.cs) passou a
  ter um `catch (Exception ex)` genérico, além dos `catch` específicos já existentes
  (`ContaNaoEncontradaException`, `DomainException`). Assim nenhuma exceção não mapeada
  vaza para o cliente como um erro cru do ASP.NET Core.
- Erros inesperados retornam `500` com uma mensagem genérica
  (`MensagensErro.ErroInterno`), **sem expor stack trace ou detalhes internos** — o
  detalhe completo vai apenas para o log do servidor.
- Todas as respostas de erro agora usam um modelo padronizado,
  [ErroResposta](Course.Api/Models/ErroResposta.cs), com `msg`, `statusCode`, `traceId`,
  `timestamp` e, quando aplicável, `detalhes` (erros de validação por campo).

### Exemplo

```csharp
catch (Exception ex)
{
    // Excecoes nao mapeadas nunca devem vazar detalhes internos para o cliente.
    _logger.LogError(ex, "Erro nao tratado ao processar {Method} {Path}", context.Request.Method, context.Request.Path);
    await EscreverRespostaAsync(context, MensagensErro.ErroInterno, StatusCodes.Status500InternalServerError, LogLevel.Error, logarMensagem: false);
}
```

Resposta antes (erro não mapeado explodia sem tratamento, gerando 500 padrão do ASP.NET):

```json
{}
```

Resposta depois:

```json
{
  "msg": "Ocorreu um erro interno inesperado. Tente novamente mais tarde.",
  "statusCode": 500,
  "traceId": "0HN...",
  "timestamp": "2026-09-09T12:00:00Z"
}
```

### Boas práticas aplicadas
- **Nunca expor detalhes internos** (stack trace, nome de classes, SQL) em respostas de erro.
- **Sempre ter um handler global** (middleware) em vez de `try/catch` espalhados pelos
  controllers.
- **Diferenciar exceções de negócio (esperadas) de falhas inesperadas** — as primeiras
  viram `400`/`404` com mensagem amigável; as segundas viram `500` genérico.
- Exceções de domínio herdam de uma base comum ([DomainException](Course.Api/Exceptions/DomainException.cs)),
  o que simplifica o `catch` no middleware.

---

## 2. Validação de entradas

### O que foi feito
- As DTOs de request ([CriarCarteiraRequest](Course.Api/Controllers/requests/CriarCarteiraRequest.cs),
  [AtualizarContaRequest](Course.Api/Controllers/requests/AtualizarContaRequest.cs),
  [DepositarRequest](Course.Api/Controllers/requests/DepositarRequest.cs),
  [SacarRequest](Course.Api/Controllers/requests/SacarRequest.cs),
  [TransferirRequest](Course.Api/Controllers/requests/TransferirRequest.cs)) passaram a usar
  `System.ComponentModel.DataAnnotations` para validar formato, tamanho e obrigatoriedade
  dos campos (nome, CPF, e-mail, senha, valores monetários e identificadores).
- Parâmetros de rota (`id`) usados em `GET`, `PUT` e `DELETE` também recebem `[Range(1, int.MaxValue)]`,
  garantindo que identificadores inválidos (`0`, negativos) sejam rejeitados antes de chegar
  à camada de serviço.
- Como os controllers usam `[ApiController]`, o ASP.NET Core valida automaticamente o
  `ModelState` e recusa a requisição antes de executar qualquer lógica de negócio.

### Exemplo

```csharp
public class CriarCarteiraRequest
{
    [Required(ErrorMessage = MensagensErro.NomeObrigatorio)]
    [StringLength(150, MinimumLength = 3, ErrorMessage = MensagensErro.NomeTamanho)]
    public required string Nome { get; set; }

    [Required(ErrorMessage = MensagensErro.CpfObrigatorio)]
    [RegularExpression(@"^(\d{3}\.\d{3}\.\d{3}-\d{2}|\d{11})$", ErrorMessage = MensagensErro.CpfFormatoInvalido)]
    public required string Cpf { get; set; }

    [Required(ErrorMessage = MensagensErro.SenhaObrigatoria)]
    [MinLength(6, ErrorMessage = MensagensErro.SenhaTamanhoMinimo)]
    public required string Senha { get; set; }

    [Required(ErrorMessage = MensagensErro.EmailObrigatorio)]
    [EmailAddress(ErrorMessage = MensagensErro.EmailFormatoInvalido)]
    public required string Email { get; set; }
}
```

```csharp
[HttpGet("saldo/{id}")]
public IActionResult ConsultarSaldo([Range(1, int.MaxValue, ErrorMessage = MensagensErro.IdContaInvalido)] int id)
```

Requisição inválida (`POST /carteira/depositar` com `Valor: -10`) agora retorna `400` com o
campo que falhou, em vez de deixar a regra de negócio (`ValorInvalidoException`) tratar um
caso que já poderia ter sido barrado na borda da API:

```json
{
  "msg": "Os dados informados sao invalidos. Verifique os campos e tente novamente.",
  "statusCode": 400,
  "traceId": "0HN...",
  "detalhes": {
    "Valor": ["O valor deve ser maior que zero."]
  }
}
```

### Boas práticas aplicadas
- **Validar o mais cedo possível** (na borda da API), evitando que dados inválidos cheguem
  às camadas de serviço/repositório.
- **Mensagens de validação claras e específicas por campo**, reaproveitando as constantes
  centralizadas de mensagens.
- Regras de negócio mais profundas (ex.: saldo insuficiente, conta inativa) continuam na
  camada de serviço — validação de formato/entrada não substitui regras de domínio.

---

## 3. Organização de mensagens de erro

### O que foi feito
- Criada a classe estática [MensagensErro](Course.Api/Exceptions/MensagensErro.cs),
  centralizando todas as mensagens (erros de domínio, erros gerais e mensagens de
  validação) em um único lugar.
- As exceções de domínio ([DomainException](Course.Api/Exceptions/DomainException.cs)) e os
  atributos de validação das DTOs passaram a referenciar essas constantes em vez de strings
  soltas espalhadas pelo código.

### Exemplo

```csharp
public static class MensagensErro
{
    public const string ContaNaoEncontrada = "Conta {0} nao encontrada.";
    public const string SaldoInsuficiente = "Saldo insuficiente.";
    public const string ValorInvalido = "Valor deve ser maior que zero.";
    // ...
}

public class ContaNaoEncontradaException : DomainException
{
    public ContaNaoEncontradaException(int id) : base(string.Format(MensagensErro.ContaNaoEncontrada, id)) { }
}
```

### Boas práticas aplicadas
- **Uma única fonte de verdade** para mensagens evita divergência de texto entre
  validação, exceções e logs.
- Facilita **internacionalização futura** (bastaria trocar a fonte das strings por um
  arquivo de recursos `.resx` por idioma).
- Facilita **revisão de UX de erros** por times de produto/QA, pois todas as mensagens
  ficam em um único arquivo.

---

## 4. Introdução a logs e monitoramento

### O que foi feito
- Injeção de `ILogger<T>` nos serviços de negócio
  ([ContaService](Course.Api/Services/ContaService.cs), [CarteiraService](Course.Api/Services/CarteiraService.cs)),
  registrando em nível `Information` as operações relevantes (criação de conta, depósito,
  saque, transferência, desativação de conta).
- O [ExceptionHandlerMiddleware](Course.Api/Middleware/ExceptionHandlerMiddleware.cs) loga em
  `Warning` as exceções de negócio (esperadas) e em `Error` (com stack trace completo) as
  exceções não tratadas.
- Novo middleware [RequestLoggingMiddleware](Course.Api/Middleware/RequestLoggingMiddleware.cs),
  que registra método, rota, status code, tempo de resposta e `traceId` de cada
  requisição — uma base simples de observabilidade.
- Adicionado endpoint de *health check* (`GET /health`) via `AddHealthChecks()` /
  `MapHealthChecks()`, útil para monitoramento por orquestradores (Kubernetes, load
  balancers, etc.).
- Configuração de logging em [appsettings.json](Course.Api/appsettings.json) /
  [appsettings.Development.json](Course.Api/appsettings.Development.json) com uma categoria
  dedicada (`Course.Api`), permitindo ajustar o nível de verbosidade da aplicação
  separadamente do nível do framework.

### Exemplo

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var stopwatch = Stopwatch.StartNew();
    await _next(context);
    stopwatch.Stop();

    _logger.LogInformation(
        "{Method} {Path} -> {StatusCode} ({ElapsedMs}ms) [traceId={TraceId}]",
        context.Request.Method, context.Request.Path, context.Response.StatusCode,
        stopwatch.ElapsedMilliseconds, context.TraceIdentifier);
}
```

```csharp
_transacaoRepository.Adicionar(CriarTransacao(contaId, contaId, valor, "D", $"Deposito de {valor} na conta {contaId}"));
_logger.LogInformation("Deposito de {Valor} realizado na conta {ContaId}", valor, contaId);
```

### Boas práticas aplicadas
- **Logging estruturado** (`{Placeholder}` em vez de concatenação de strings), o que
  facilita busca/filtragem em ferramentas como Seq, Elastic ou Application Insights.
- **Nível de log adequado à severidade**: `Information` para eventos de negócio,
  `Warning` para erros esperados/validação, `Error` para falhas inesperadas.
- **Nunca logar dados sensíveis** (senha em texto plano, hash de senha, dados de cartão) —
  os logs desta API só referenciam identificadores de conta e valores.
- **`traceId` correlacionando logs e resposta de erro**, permitindo localizar rapidamente
  no log do servidor o que aconteceu em uma requisição específica reportada pelo cliente.
- Expor um endpoint de *health check* dedicado (`/health`), separado das rotas de negócio.

---

## Próximos passos sugeridos (fora do escopo desta implementação)
- Adotar um provedor de log estruturado externo (ex.: Serilog + Seq/Elastic) para
  persistência e busca de logs em produção.
- Adicionar métricas (contadores de requisições, latência, taxa de erro) via
  `System.Diagnostics.Metrics` ou OpenTelemetry.
- Adicionar `FluentValidation` caso as regras de validação fiquem mais complexas do que
  `DataAnnotations` consegue expressar de forma legível.
- Traduzir `MensagensErro` para múltiplos idiomas usando arquivos de recurso (`.resx`).
