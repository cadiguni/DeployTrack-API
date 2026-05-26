# DeployTrack API - Guia do Codigo

Este documento explica a estrutura da API, o papel de cada pasta e o fluxo principal do sistema. Ele foi escrito para servir tanto como referencia tecnica quanto como material de portfolio.

## Visao geral

A DeployTrack API registra aplicacoes, ambientes, deploys e health checks. A ideia e permitir que uma pipeline, como GitHub Actions, registre cada deploy feito por outra aplicacao, por exemplo a futura `Sample Orders API`.

O projeto usa:

- ASP.NET Core Web API para expor endpoints HTTP.
- Entity Framework Core para acessar o banco.
- PostgreSQL como banco relacional.
- JWT Bearer para autenticacao e autorizacao por roles.
- Swagger para documentar e testar a API localmente.
- Docker Compose para subir API e PostgreSQL em ambiente local.

## Estrutura principal

```text
src/DevOpsBoard.Api/
|-- Controllers/
|-- Data/
|-- Dtos/
|-- Models/
|-- Services/
|-- Program.cs
|-- appsettings.json
|-- appsettings.Development.json
`-- DevOpsBoard.Api.http
```

Observacao: o nome interno do projeto ainda e `DevOpsBoard.Api`, mas o produto exposto esta documentado como `DeployTrack`.

## Program.cs

O arquivo `Program.cs` e o ponto de entrada da API.

Responsabilidades principais:

- Registra os controllers.
- Configura serializacao de enums como texto no JSON.
- Configura o `DevOpsBoardDbContext` com PostgreSQL.
- Registra os servicos de senha e JWT.
- Configura autenticacao JWT Bearer.
- Configura autorizacao.
- Configura Swagger com suporte a token Bearer.
- Aplica migrations automaticamente ao iniciar a API.
- Liga o pipeline HTTP com HTTPS redirection, autenticacao, autorizacao e controllers.

Fluxo simplificado:

```text
Request HTTP
  -> ASP.NET Core middleware
  -> Authentication
  -> Authorization
  -> Controller
  -> DbContext
  -> PostgreSQL
```

## Data

### DevOpsBoardDbContext

Arquivo: `src/DevOpsBoard.Api/Data/DevOpsBoardDbContext.cs`

O `DbContext` representa a sessao com o banco de dados. Ele expoe os `DbSet`, que sao as tabelas manipuladas pela API:

- `Users`
- `Applications`
- `Environments`
- `Deployments`
- `HealthChecks`

Tambem configura regras do modelo:

- Indices unicos, como email de usuario e nome da aplicacao.
- Tamanho maximo de campos de texto.
- Conversao de enums para string no banco.
- Relacionamentos entre deploys, aplicacoes e ambientes.
- Seed inicial dos ambientes `dev`, `staging` e `production`.

## Models

Os models representam as entidades persistidas no banco.

### User

Representa um usuario que pode autenticar na API.

Campos importantes:

- `Id`
- `Name`
- `Email`
- `PasswordHash`
- `Role`
- `CreatedAt`

O campo `Role` usa o enum `UserRole`, com valores como `Admin`, `DevOps` e `Viewer`.

### Application

Representa uma aplicacao monitorada pelo DeployTrack.

Exemplo: `orders-api`, `billing-api`, `payments-api`.

Campos importantes:

- `Id`
- `Name`
- `Description`
- `RepositoryUrl`
- `CreatedAt`
- `UpdatedAt`

Uma aplicacao pode ter muitos deploys e muitos health checks.

### Environment

Representa o ambiente onde a aplicacao roda.

Exemplos:

- `dev`
- `staging`
- `production`

Esses ambientes sao criados automaticamente pelo seed do Entity Framework Core.

### Deployment

Representa um deploy registrado no sistema.

Campos importantes:

- `ApplicationId`
- `EnvironmentId`
- `Version`
- `Status`
- `DeployedBy`
- `CommitSha`
- `PipelineUrl`
- `StartedAt`
- `FinishedAt`
- `CreatedAt`

O status usa o enum `DeploymentStatus`, com valores como `Success`, `Failed`, `Running` e `Canceled`.

### ApplicationHealthCheck

Representa uma verificacao de saude de uma aplicacao em um ambiente.

Campos importantes:

- `ApplicationId`
- `EnvironmentId`
- `Status`
- `Details`
- `CheckedBy`
- `CheckedAt`

O status usa o enum `HealthStatus`.

## Dtos

DTOs sao objetos usados na entrada e saida dos endpoints. Eles evitam expor diretamente os models do banco na API publica.

Exemplos:

- `RegisterUserRequest`
- `LoginRequest`
- `AuthResponse`
- `ApplicationRequest`
- `ApplicationResponse`
- `CreateDeploymentRequest`
- `UpdateDeploymentRequest`
- `DeploymentResponse`
- `CreateHealthCheckRequest`
- `HealthCheckResponse`

Essa separacao ajuda a controlar quais campos o cliente pode enviar e quais campos a API devolve.

## Services

### PasswordHasher

Arquivo: `src/DevOpsBoard.Api/Services/PasswordHasher.cs`

Responsavel por:

- Gerar hash de senha no cadastro.
- Validar senha no login.

Ele usa `PasswordHasher<T>` do ASP.NET Core, que ja implementa uma estrategia segura para armazenamento de senhas.

### JwtTokenService

Arquivo: `src/DevOpsBoard.Api/Services/JwtTokenService.cs`

Responsavel por criar o token JWT depois do registro ou login.

O token inclui claims como:

- Id do usuario.
- Email.
- Nome.
- Role.

Essas claims sao usadas pelo ASP.NET Core para autorizar endpoints protegidos por role.

## Controllers

Controllers recebem as requisicoes HTTP, validam dados basicos, chamam o banco via `DbContext` e retornam respostas HTTP.

### AuthController

Rota base: `/api/auth`

Endpoints:

- `POST /api/auth/register`
- `POST /api/auth/login`

Fluxo do cadastro:

```text
Cliente envia nome, email, senha e role
  -> API valida os campos
  -> API verifica se email ja existe
  -> API gera hash da senha
  -> API salva usuario
  -> API cria JWT
  -> API retorna token e dados do usuario
```

Fluxo do login:

```text
Cliente envia email e senha
  -> API busca usuario pelo email
  -> API valida senha com PasswordHasher
  -> API cria JWT
  -> API retorna token e dados do usuario
```

### ApplicationsController

Rota base: `/api/applications`

Endpoints:

- `GET /api/applications`
- `GET /api/applications/{id}`
- `POST /api/applications`
- `PUT /api/applications/{id}`
- `DELETE /api/applications/{id}`

Regras de autorizacao:

- Listar e consultar exige usuario autenticado.
- Criar e editar exige role `Admin` ou `DevOps`.
- Remover exige role `Admin`.

Regra de negocio importante:

- Uma aplicacao nao pode ser removida se ja tiver deploys ou health checks associados.

### EnvironmentsController

Rota base: `/api/environments`

Endpoint:

- `GET /api/environments`

Esse controller apenas lista os ambientes cadastrados. Hoje os ambientes sao seedados no banco.

### DeploymentsController

Rota base: `/api/deployments`

Endpoints:

- `GET /api/deployments`
- `GET /api/deployments/{id}`
- `POST /api/deployments`
- `PUT /api/deployments/{id}`
- `DELETE /api/deployments/{id}`
- `GET /api/deployments/latest`
- `GET /api/deployments/history`

Regras de autorizacao:

- Listar, consultar historico e consultar ultimo deploy exige usuario autenticado.
- Criar e editar exige role `Admin` ou `DevOps`.
- Remover exige role `Admin`.

Fluxo de criacao de deployment:

```text
Cliente envia applicationName, environment, version, status e metadados
  -> API valida campos obrigatorios
  -> API busca aplicacao pelo nome
  -> API busca ambiente pelo nome
  -> API cria registro de deploy
  -> API salva no PostgreSQL
  -> API retorna o deploy criado
```

O endpoint `latest` retorna o deploy mais recente por aplicacao e, opcionalmente, por ambiente.

### HealthChecksController

Rota base: `/api/health-checks`

Endpoints:

- `POST /api/health-checks`
- `GET /api/health-checks/current`

Regras de autorizacao:

- Registrar health check exige role `Admin` ou `DevOps`.
- Consultar status atual exige usuario autenticado.

O endpoint `current` retorna a verificacao de saude mais recente para uma aplicacao em um ambiente.

## Autenticacao e autorizacao

A API usa JWT Bearer.

Endpoints protegidos usam `[Authorize]`.

Endpoints com restricao de role usam:

```csharp
[Authorize(Roles = "Admin,DevOps")]
```

ou:

```csharp
[Authorize(Roles = "Admin")]
```

Fluxo de uso:

```text
1. Criar usuario em /api/auth/register
2. Copiar o token retornado
3. Enviar Authorization: Bearer <token>
4. Chamar endpoints protegidos
```

## Banco de dados e migrations

As migrations ficam em:

```text
src/DevOpsBoard.Api/Data/Migrations/
```

No startup da API, este trecho aplica migrations automaticamente:

```csharp
await dbContext.Database.MigrateAsync();
```

Isso facilita o uso local com Docker, porque a API sobe e prepara o banco automaticamente.

## Docker Compose

O arquivo `docker-compose.yml` sobe:

- API na porta `8080`.
- PostgreSQL na porta `5432`.

Tambem injeta variaveis de ambiente para:

- Connection string.
- Issuer e audience do JWT.
- Chave de assinatura JWT local.

## Swagger

Em ambiente de desenvolvimento, o Swagger fica em:

```text
http://localhost:8080/swagger
```

Ele permite testar os endpoints e configurar o token JWT no botao de autorizacao.

## Fluxo esperado com a futura Sample Orders API

```text
GitHub Actions da Orders API
  -> Build e deploy da Orders API
  -> POST /api/deployments na DeployTrack API
  -> DeployTrack grava versao, commit, ambiente e status
  -> Portfolio mostra rastreabilidade de deploy
```

Esse fluxo transforma a DeployTrack em uma API de observabilidade simples para releases.

## Proximos passos sugeridos

- Renomear internamente o projeto de `DevOpsBoard.Api` para `DeployTrack.Api`.
- Criar testes automatizados para controllers e regras principais.
- Criar a `Sample Orders API`.
- Criar workflow do GitHub Actions que registra deploy na DeployTrack.
- Criar tela ou dashboard para visualizar deploys e health checks.
