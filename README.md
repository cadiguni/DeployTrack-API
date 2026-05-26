# DeployTrack API

API para registrar e consultar deploys de aplicacoes em multiplos ambientes. Esta primeira etapa do portfolio entrega a API local com ASP.NET Core, Entity Framework Core, PostgreSQL via Docker, Swagger, autenticacao JWT e CRUDs principais.

## Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Bearer Authentication
- Swagger
- Docker e Docker Compose

## Estrutura

```text
DeployTrack-API/
|-- src/
|   `-- DevOpsBoard.Api/
|-- infra/
|   |-- aws/
|   |   `-- task-definitions/
|   `-- terraform/
|-- pipelines/
|   `-- github-actions/
|-- .github/
|   `-- workflows/
|-- Dockerfile
|-- docker-compose.yml
`-- DevOpsBoard.sln
```

Observacao: o projeto .NET ainda usa o nome interno `DevOpsBoard.Api`, mas a API exposta e a configuracao local ja estao alinhadas ao produto `DeployTrack`.

## Modulos do MVP

- Auth: registro, login, JWT e roles `Admin`, `DevOps`, `Viewer`
- Applications: cadastrar, listar, consultar, editar e remover aplicacoes
- Environments: ambientes seedados `dev`, `staging`, `production`
- Deployments: cadastrar, listar, consultar, editar, remover, consultar ultimo deploy e historico por aplicacao/ambiente
- Health Checks: registrar status e consultar status atual

## Documentacao do codigo

- [Guia do codigo](docs/CODE_WALKTHROUGH.md): explica arquitetura, pastas, fluxo das requisicoes, autenticacao, banco, controllers e endpoints.

## Como rodar com Docker

```bash
docker compose up --build
```

A API sobe em:

```text
http://localhost:8080
```

Swagger:

```text
http://localhost:8080/swagger
```

## Como rodar localmente

Suba apenas o PostgreSQL pelo compose:

```bash
docker compose up postgres
```

Depois rode a API:

```bash
dotnet run --project src/DevOpsBoard.Api
```

A connection string padrao esta em `src/DevOpsBoard.Api/appsettings.Development.json`.

## Fluxo rapido

1. Crie um usuario:

```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Admin",
  "email": "admin@deploytrack.local",
  "password": "ChangeMe123",
  "role": "Admin"
}
```

2. Use o token retornado no header:

```http
Authorization: Bearer <token>
```

3. Crie uma aplicacao:

```http
POST /api/applications
Content-Type: application/json

{
  "name": "orders-api",
  "description": "Sample Orders API",
  "repositoryUrl": "https://github.com/user/orders-api"
}
```

4. Registre um deploy:

```http
POST /api/deployments
Content-Type: application/json

{
  "applicationName": "orders-api",
  "environment": "production",
  "version": "1.0.0",
  "status": "Success",
  "deployedBy": "github-actions",
  "commitSha": "a1b2c3d",
  "pipelineUrl": "https://github.com/user/repo/actions/runs/123",
  "startedAt": "2026-05-18T20:10:00Z",
  "finishedAt": "2026-05-18T20:14:00Z"
}
```

## Endpoints principais

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/applications`
- `GET /api/applications/{id}`
- `POST /api/applications`
- `PUT /api/applications/{id}`
- `DELETE /api/applications/{id}`
- `GET /api/environments`
- `GET /api/deployments`
- `GET /api/deployments/{id}`
- `POST /api/deployments`
- `PUT /api/deployments/{id}`
- `DELETE /api/deployments/{id}`
- `GET /api/deployments/latest?applicationName=orders-api&environment=production`
- `GET /api/deployments/history?applicationName=orders-api`
- `POST /api/health-checks`
- `GET /api/health-checks/current?applicationName=orders-api&environment=production`

## Observacoes

As migrations sao aplicadas automaticamente no startup da API. Em producao, substitua `Jwt__SigningKey` e a connection string por secrets gerenciados, por exemplo via AWS Secrets Manager.
