# DevOpsBoard API

API desenvolvida para registrar e consultar deploys de aplicacoes em multiplos ambientes, com autenticacao JWT, PostgreSQL, Docker, deploy automatizado em AWS ECS Fargate, infraestrutura com Terraform, logs no CloudWatch e secrets no AWS Secrets Manager.

## Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Bearer Authentication
- Docker e Docker Compose
- GitHub Actions
- AWS ECR, ECS Fargate, RDS, Secrets Manager e CloudWatch
- Terraform

## Estrutura

```text
DevOpsBoard-API/
├─ src/
│  └─ DevOpsBoard.Api/
├─ infra/
│  ├─ aws/
│  │  └─ task-definitions/
│  └─ terraform/
├─ pipelines/
│  └─ github-actions/
├─ .github/
│  └─ workflows/
├─ Dockerfile
├─ docker-compose.yml
└─ DevOpsBoard.sln
```

Os workflows executaveis ficam em `.github/workflows`, que e o caminho reconhecido automaticamente pelo GitHub Actions. A pasta `pipelines/` documenta os fluxos e decisoes de CI/CD.

## Modulos do MVP

- Auth: criar usuario, login, JWT e roles `Admin`, `DevOps`, `Viewer`
- Applications: cadastrar, listar, editar e remover aplicacoes
- Environments: ambientes seedados `dev`, `staging`, `production`
- Deployments: registrar deploy, consultar ultimo deploy e historico por aplicacao/ambiente
- Health Checks: registrar status e consultar status atual

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

Suba um PostgreSQL local ou use apenas o banco do compose:

```bash
docker compose up postgres
```

Depois rode a API:

```bash
dotnet run --project src/DevOpsBoard.Api
```

A connection string padrao esta em `src/DevOpsBoard.Api/appsettings.Development.json`.

## CI/CD

Este projeto usa GitHub Actions:

- `.github/workflows/ci.yml`: restore, build e test da solution.
- `.github/workflows/deploy-dev.yml`: build da imagem Docker, push para Amazon ECR e update do servico no ECS Fargate.

O deploy usa OpenID Connect entre GitHub Actions e AWS IAM. Assim o repositorio nao precisa armazenar `AWS_ACCESS_KEY_ID` nem `AWS_SECRET_ACCESS_KEY`.

Configure um environment chamado `dev` no GitHub e adicione o secret:

```text
AWS_ROLE_TO_ASSUME=arn:aws:iam::<account-id>:role/<github-actions-deploy-role>
```

Antes do primeiro deploy, ajuste em `.github/workflows/deploy-dev.yml`:

- `AWS_REGION`
- `ECR_REPOSITORY`
- `ECS_CLUSTER`
- `ECS_SERVICE`
- `ECS_TASK_DEFINITION`
- `CONTAINER_NAME`

Tambem ajuste os ARNs placeholder em `infra/aws/task-definitions/devopsboard-api-dev.json`.

## Fluxo rapido

1. Crie um usuario:

```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Admin",
  "email": "admin@devopsboard.local",
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
  "name": "billing-api",
  "description": "Billing service API",
  "repositoryUrl": "https://github.com/user/billing-api"
}
```

4. Registre um deploy:

```http
POST /api/deployments
Content-Type: application/json

{
  "applicationName": "billing-api",
  "environment": "production",
  "version": "1.4.2",
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
- `POST /api/applications`
- `PUT /api/applications/{id}`
- `DELETE /api/applications/{id}`
- `GET /api/environments`
- `POST /api/deployments`
- `GET /api/deployments/latest?applicationName=billing-api&environment=production`
- `GET /api/deployments/history?applicationName=billing-api`
- `GET /api/deployments/history?environment=production`
- `POST /api/health-checks`
- `GET /api/health-checks/current?applicationName=billing-api&environment=production`

## Observacoes

As migrations sao aplicadas automaticamente no startup da API. Em producao, substitua `Jwt__SigningKey` e a connection string por secrets gerenciados, por exemplo via AWS Secrets Manager.
