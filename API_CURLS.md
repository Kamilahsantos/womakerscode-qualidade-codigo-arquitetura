# Requisições da API (cURL)

URL base (perfil `http` em [launchSettings.json](Course.Api/Properties/launchSettings.json)): `http://localhost:5249`

## CarteiraController (`/Carteira`)

### Criar carteira
```bash
curl -X POST http://localhost:5249/Carteira/criar-carteira \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Maria da Silva",
    "cpf": "123.456.789-00",
    "senha": "senha123",
    "email": "maria@email.com"
  }'
```

### Buscar carteira por id
```bash
curl -X GET http://localhost:5249/Carteira/1
```

### Depositar
```bash
curl -X POST http://localhost:5249/Carteira/depositar \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "valor": 100.00
  }'
```

### Sacar
```bash
curl -X POST http://localhost:5249/Carteira/sacar \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "valor": 50.00
  }'
```

### Transferir
```bash
curl -X POST http://localhost:5249/Carteira/transferir \
  -H "Content-Type: application/json" \
  -d '{
    "idOrigem": 1,
    "idDestino": 2,
    "valor": 30.00
  }'
```

### Buscar extrato
```bash
curl -X GET http://localhost:5249/Carteira/1/extrato
```

### Deletar (desativar) conta
```bash
curl -X DELETE http://localhost:5249/Carteira/1
```

## ContaController (`/Conta`)

### Listar todas as contas
```bash
curl -X GET http://localhost:5249/Conta/todas
```

### Consultar saldo
```bash
curl -X GET http://localhost:5249/Conta/saldo/1
```

### Atualizar conta
```bash
curl -X PUT http://localhost:5249/Conta/atualizar/1 \
  -H "Content-Type: application/json" \
  -d '{
    "novoNome": "Maria Souza",
    "novoEmail": "maria.souza@email.com",
    "novaSenha": "novaSenha123"
  }'
```

### Listar histórico de transações
```bash
curl -X GET http://localhost:5249/Conta/historico-transacoes
```
