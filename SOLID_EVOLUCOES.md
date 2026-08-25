# Evoluções Recomendadas com SOLID

Este documento complementa o `REFACTORING.md` com pontos adicionais que podem evoluir a arquitetura do projeto.

## 1. Validadores Dedicados por Caso de Uso (S e O)

### Princípios SOLID aplicados
- **S (Single Responsibility):** cada validador passa a ter uma única responsabilidade: validar um caso de uso específico.
- **O (Open/Closed):** novas regras de validação são adicionadas criando novos validadores ou novas regras no validador, sem alterar o núcleo das services.

### Situação atual
As validações de entrada e regras simples de consistência ainda tendem a ficar distribuídas entre controllers e services.

### Evolução sugerida
Criar validadores dedicados por request/caso de uso, por exemplo:

- `CriarContaValidator`
- `DepositarValidator`
- `TransferirValidator`

Esses validadores podem ser chamados antes da regra principal de negócio.

### Ganho
- Reduz responsabilidade das services.
- Facilita extensão de validações sem alterar regras centrais.
- Melhora legibilidade e cobertura de testes.

---

## 2. Política de Senha como Abstração (S, O e D)

### Princípios SOLID aplicados
- **S (Single Responsibility):** hashing e política de senha ficam separados da lógica de conta/carteira.
- **O (Open/Closed):** permite evoluir algoritmo e critérios de segurança sem modificar as services de negócio.
- **D (Dependency Inversion):** services dependem de interfaces (`IPasswordHasher`, `IPasswordPolicy`), não de implementações concretas.

### Situação atual
`PasswordUtils` resolve o hashing, mas a política de senha (força mínima, regras de composição, troca de algoritmo) não está centralizada como contrato de domínio.

### Evolução sugerida
Introduzir interfaces como:

- `IPasswordHasher`
- `IPasswordPolicy`

Implementações concretas ficam na infraestrutura, enquanto o domínio depende apenas das abstrações.

### Ganho
- Permite trocar algoritmo sem alterar service.
- Centraliza regras de segurança e evita duplicação.
- Mantém aderência ao DIP.

---

## 3. Repositórios com Contratos Mais Específicos (I)

### Princípios SOLID aplicados
- **I (Interface Segregation):** separa leitura e escrita para que cada cliente dependa apenas do que realmente usa.
- **D (Dependency Inversion):** services continuam dependendo de abstrações, agora mais coesas e granulares.

### Situação atual
Interfaces de repositório podem crescer ao longo do tempo e virar contratos amplos demais.

### Evolução sugerida
Segregar contratos por intenção de uso:

- leitura (`IContaReadRepository`)
- escrita (`IContaWriteRepository`)

Services de consulta dependem apenas de leitura; services de comando dependem de escrita.

### Ganho
- Menor acoplamento.
- Contratos mais claros.
- Reduz impacto de mudanças em operações não relacionadas.

---

## 4. Estratégias para Regras de Transação (O e L)

### Princípios SOLID aplicados
- **O (Open/Closed):** novas regras (tarifa, limite, janela de horário) entram como novas estratégias, sem alterar classes estáveis.
- **L (Liskov Substitution):** qualquer implementação de estratégia deve poder substituir outra mantendo o contrato e o comportamento esperado do fluxo.
- **D (Dependency Inversion):** serviço de transação passa a depender da interface da estratégia, não da regra concreta.

### Situação atual
Regras de transação (saque, depósito, transferência) podem crescer com variações por tipo de conta, limite diário, tarifa, horário etc.

### Evolução sugerida
Aplicar Strategy para regras variáveis:

- `IRegraSaque`
- `IRegraTransferencia`

Cada estratégia implementa o mesmo contrato e pode ser substituída sem quebrar comportamento esperado.

### Ganho
- Extensão por novos cenários sem modificar classes estáveis.
- Menor risco de regressão em regras já consolidadas.

---

## 5. Serviço de Domínio para Auditoria (S e D)

### Princípios SOLID aplicados
- **S (Single Responsibility):** auditoria deixa de ser detalhe espalhado e vira responsabilidade explícita em um serviço próprio.
- **D (Dependency Inversion):** domínio depende de `IAuditoriaService`; o destino (arquivo, banco, fila) é detalhe de infraestrutura.
- **O (Open/Closed):** novos destinos de auditoria podem ser adicionados sem alterar regras principais.

### Situação atual
Registro de transação atende o fluxo funcional, mas trilhas de auditoria (quem alterou, quando, origem da operação) podem ficar espalhadas no futuro.

### Evolução sugerida
Criar um contrato de auditoria, por exemplo `IAuditoriaService`, para concentrar eventos relevantes do domínio.

### Ganho
- Responsabilidade única para rastreabilidade.
- Facilita observabilidade e compliance.
- Permite trocar destino da auditoria (memória, arquivo, banco, fila) sem alterar services principais.

---

## 6. Padronizar Resultado de Casos de Uso (S e I)

### Princípios SOLID aplicados
- **S (Single Responsibility):** controllers focam em orquestrar HTTP; montagem do contrato de saída fica padronizada.
- **I (Interface Segregation):** consumidores passam a depender de DTOs específicos por caso de uso, evitando contratos genéricos inchados.
- **O (Open/Closed):** novos campos de saída podem evoluir por DTO específico sem quebrar outros fluxos.

### Situação atual
Mesmo com middleware para exceções, respostas de sucesso podem variar entre endpoints.

### Evolução sugerida
Definir um padrão de saída para casos de uso (DTO de resposta por operação), evitando que cada controller monte retornos diferentes para conceitos semelhantes.

### Ganho
- Contratos de API mais consistentes.
- Menos lógica de composição no controller.
- Facilidade para evolução e documentação da API.

---

## 7. Introduzir Camada de Application Use Cases (S e D)

### Princípios SOLID aplicados
- **S (Single Responsibility):** use case orquestra fluxo; serviços de domínio executam regras puras.
- **D (Dependency Inversion):** use cases dependem de interfaces (repositórios, auditoria, políticas), facilitando troca de implementação.
- **O (Open/Closed):** novos fluxos de aplicação entram como novos use cases, preservando os existentes.

### Situação atual
Services já concentram regras de negócio, mas alguns fluxos podem crescer e misturar orquestração com regra pura.

### Evolução sugerida
Separar:

- Application layer (orquestra fluxo do caso de uso)
- Domain services (regras de negócio puras)

Exemplo: `RealizarTransferenciaUseCase` orquestra validação, débito, crédito e auditoria, delegando regras específicas a serviços de domínio.

### Ganho
- Melhor coesão por camada.
- Maior testabilidade dos fluxos completos.
- Menos risco de classes de serviço inchadas.

---

## 8. Evolução de Testes Orientada a SOLID - desafio EXTRA

### Princípios SOLID aplicados
- **L (Liskov Substitution):** testes de substituição garantem que implementações diferentes respeitam o mesmo contrato.
- **I (Interface Segregation):** testes de contrato por interface pequena tornam mais claro o comportamento esperado de cada capacidade.
- **D (Dependency Inversion):** testes com mocks/stubs reforçam dependência em abstrações e não em detalhes concretos.

### Situação atual
A base de testes já cobre cenário funcional importante, mas pode evoluir com foco em contratos.

### Evolução sugerida
- Testes de contrato para interfaces críticas (`IContaRepository`, `ITransacaoRepository`).
- Testes de substituição para estratégias (garantir LSP).
- Testes de integração leves para validar composição real via DI.

### Ganho
- Segurança para trocar implementações.
- Validação explícita dos princípios SOLID no comportamento.
- Menor risco em refactors futuros.

---
