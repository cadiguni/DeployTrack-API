# Etapa 3 - Containerizacao Local

Esta etapa coloca as duas APIs em containers separados e valida o ambiente local com Docker Compose.

O objetivo e reproduzir, na maquina local, o desenho que depois sera levado para AWS ECS Fargate.

## Arquitetura local

```text
Docker Compose
|-- deploytrack-api
|-- orders-api
`-- postgres
```

## Containers

### deploytrack-api

Dockerfile:

```text
deploytrack-api/Dockerfile
```

Responsavel por empacotar a DeployTrack API.

No container, a API escuta na porta interna `8080`.

No Docker Compose, ela fica exposta na porta local:

```text
http://localhost:8080
```

Ela depende do PostgreSQL para iniciar corretamente, porque aplica migrations no startup.

### orders-api

Dockerfile:

```text
orders-api/Dockerfile
```

Responsavel por empacotar a Sample Orders API.

No container, a API escuta na porta interna `8080`.

No Docker Compose, ela fica exposta na porta local:

```text
http://localhost:8081
```

Ela nao depende de banco nesta etapa, porque armazena pedidos em memoria.

### postgres

Imagem:

```text
postgres:16-alpine
```

Banco usado pela DeployTrack API.

Configuracao local:

- Database: `deploytrack`
- User: `deploytrack`
- Password: `deploytrack`
- Porta local: `5432`

O Compose tambem define um health check com `pg_isready`. A DeployTrack API so inicia depois que o PostgreSQL estiver saudavel.

## Dockerfiles

Os dois Dockerfiles usam build multi-stage.

Fluxo:

```text
SDK image
  -> restore
  -> publish
  -> runtime image
  -> dotnet <api>.dll
```

Por que multi-stage?

- A imagem final fica menor.
- O SDK do .NET fica apenas na etapa de build.
- O container final usa somente o runtime ASP.NET Core.

## docker-compose.yml

Arquivo:

```text
docker-compose.yml
```

Servicos:

- `deploytrack-api`
- `orders-api`
- `postgres`

Portas:

```text
DeployTrack API:    http://localhost:8080
Sample Orders API: http://localhost:8081
PostgreSQL:         localhost:5432
```

## Como validar localmente

Subir tudo:

```bash
docker compose up --build
```

Rodar em background:

```bash
docker compose up -d --build
```

Ver logs:

```bash
docker compose logs -f
```

Parar:

```bash
docker compose down
```

Parar e remover volume do banco:

```bash
docker compose down -v
```

## Testes manuais

### DeployTrack API

Swagger:

```text
http://localhost:8080/swagger
```

Health do container depende da API subir e conseguir aplicar migrations no PostgreSQL.

### Sample Orders API

Health:

```http
GET http://localhost:8081/health
```

Criar pedido:

```http
POST http://localhost:8081/orders
Content-Type: application/json

{
  "customerName": "Maria Silva",
  "items": [
    {
      "productName": "Keyboard",
      "quantity": 1,
      "unitPrice": 250.00
    }
  ]
}
```

Listar pedidos:

```http
GET http://localhost:8081/orders
```

Atualizar status:

```http
PATCH http://localhost:8081/orders/{id}/status
Content-Type: application/json

{
  "status": "Paid"
}
```

## Relacao com AWS

Depois de validar localmente, cada API pode virar uma imagem separada no ECR:

```text
deploytrack-api -> ECR -> ECS service DeployTrack
orders-api      -> ECR -> ECS service Orders
```

O PostgreSQL local sera substituido por RDS PostgreSQL.

Esse passo reduz risco antes de gastar na AWS, porque valida:

- Build das imagens.
- Startup das APIs.
- Variaveis de ambiente.
- Conexao DeployTrack -> PostgreSQL.
- Exposicao de portas.
- Funcionamento basico da Orders API.
