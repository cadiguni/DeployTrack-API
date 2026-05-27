# Sample Orders API - Etapa 2

Esta etapa adiciona uma segunda aplicacao ao portfolio: a `Sample Orders API`.

Ela e uma API simples de pedidos ficticios. O objetivo nao e ser um e-commerce completo, mas sim representar uma aplicacao real que pode ser publicada e rastreada pela DeployTrack.

## Papel na arquitetura

```text
Sample Orders API
  -> e publicada por uma pipeline
  -> a pipeline registra o deploy na DeployTrack API
  -> a DeployTrack guarda ambiente, versao, commit, status e link da execucao
```

Com isso, o portfolio passa a ter duas pecas:

- `DeployTrack API`: sistema que registra deploys.
- `Sample Orders API`: aplicacao exemplo que sera implantada e monitorada.

## Localizacao no projeto

```text
src/SampleOrders.Api/
|-- Dtos/
|-- Models/
|-- Services/
|-- Properties/
|-- Program.cs
|-- SampleOrders.Api.csproj
|-- SampleOrders.Api.http
`-- Dockerfile
```

## Funcionalidades

A API controla pedidos em memoria e expoe estes endpoints:

- `POST /orders`
- `GET /orders`
- `GET /orders/{id}`
- `PATCH /orders/{id}/status`
- `GET /health`

Os dados ficam em memoria. Isso significa que, ao reiniciar a aplicacao, os pedidos criados localmente sao perdidos. Para esta etapa isso e intencional: a API existe para simular uma aplicacao real no ciclo de deploy, nao para demonstrar persistencia.

## Program.cs

Arquivo: `src/SampleOrders.Api/Program.cs`

Este arquivo configura e expoe a API usando Minimal APIs do ASP.NET Core.

Responsabilidades:

- Configurar enums como string no JSON.
- Registrar o armazenamento em memoria com `AddSingleton`.
- Criar endpoint de health check.
- Criar o grupo de rotas `/orders`.
- Validar pedidos criados via `POST /orders`.
- Converter models internos para responses publicas.

Fluxo simplificado:

```text
Request HTTP
  -> Minimal API endpoint
  -> validacao simples
  -> IOrderStore
  -> memoria da aplicacao
  -> response JSON
```

## Models

### Order

Arquivo: `src/SampleOrders.Api/Models/Order.cs`

Representa um pedido.

Campos:

- `Id`: identificador unico.
- `CustomerName`: nome do cliente.
- `Items`: itens do pedido.
- `Status`: status atual do pedido.
- `TotalAmount`: total calculado a partir dos itens.
- `CreatedAt`: data de criacao.
- `UpdatedAt`: data da ultima atualizacao.

O `TotalAmount` e calculado, nao armazenado manualmente:

```text
quantity * unitPrice de cada item
```

### OrderItem

Arquivo: `src/SampleOrders.Api/Models/OrderItem.cs`

Representa um item dentro do pedido.

Campos:

- `ProductName`
- `Quantity`
- `UnitPrice`

### OrderStatus

Arquivo: `src/SampleOrders.Api/Models/OrderStatus.cs`

Enum com os status possiveis:

- `Pending`
- `Paid`
- `Preparing`
- `Shipped`
- `Delivered`
- `Canceled`

## DTOs

Arquivo: `src/SampleOrders.Api/Dtos/OrderDtos.cs`

DTOs definem o contrato publico da API.

### CreateOrderRequest

Usado no `POST /orders`.

Exemplo:

```json
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

### UpdateOrderStatusRequest

Usado no `PATCH /orders/{id}/status`.

Exemplo:

```json
{
  "status": "Paid"
}
```

### OrderResponse

Resposta padrao dos endpoints que retornam pedidos.

Inclui:

- Dados do pedido.
- Itens.
- Status.
- Valor total.
- Datas de criacao e atualizacao.

## Services

### IOrderStore

Arquivo: `src/SampleOrders.Api/Services/IOrderStore.cs`

Contrato do armazenamento de pedidos.

Operacoes:

- `GetAll`
- `GetById`
- `Create`
- `UpdateStatus`

### InMemoryOrderStore

Arquivo: `src/SampleOrders.Api/Services/InMemoryOrderStore.cs`

Implementacao em memoria do `IOrderStore`.

Ela usa `ConcurrentDictionary<Guid, Order>` para guardar pedidos de forma simples e segura para acessos concorrentes basicos.

Essa camada existe para separar os endpoints da forma como os pedidos sao armazenados. No futuro, seria facil trocar por banco de dados sem reescrever todos os endpoints.

## Endpoints

### GET /health

Verifica se a API esta respondendo.

Resposta esperada:

```json
{
  "status": "Healthy",
  "service": "Sample Orders API",
  "checkedAt": "2026-05-26T18:00:00Z"
}
```

### POST /orders

Cria um pedido.

Validacoes:

- `customerName` e obrigatorio.
- Deve existir pelo menos um item.
- `productName` e obrigatorio em todos os itens.
- `quantity` deve ser maior que zero.
- `unitPrice` nao pode ser negativo.

Resposta:

- `201 Created` quando cria.
- `400 Bad Request` quando a entrada e invalida.

### GET /orders

Lista todos os pedidos em memoria, ordenando os mais recentes primeiro.

### GET /orders/{id}

Busca um pedido por id.

Resposta:

- `200 OK` quando encontra.
- `404 Not Found` quando nao encontra.

### PATCH /orders/{id}/status

Atualiza apenas o status do pedido.

Exemplo:

```json
{
  "status": "Shipped"
}
```

Resposta:

- `200 OK` quando atualiza.
- `404 Not Found` quando o pedido nao existe.

## Como rodar localmente

Rodando direto pelo .NET:

```bash
dotnet run --project src/SampleOrders.Api
```

URL local:

```text
http://localhost:5030
```

Rodando com Docker Compose junto com a DeployTrack:

```bash
docker compose up --build
```

URLs:

```text
DeployTrack API:      http://localhost:8080
Sample Orders API:   http://localhost:8081
```

## Arquivo .http

O arquivo `src/SampleOrders.Api/SampleOrders.Api.http` contem exemplos prontos para testar:

- Health check.
- Criacao de pedido.
- Listagem.
- Consulta por id.
- Atualizacao de status.

## Relacao com a DeployTrack

Na proxima etapa, a pipeline da `Sample Orders API` podera chamar a DeployTrack depois do deploy:

```http
POST /api/deployments
Authorization: Bearer <token>
Content-Type: application/json

{
  "applicationName": "orders-api",
  "environment": "production",
  "version": "1.0.0",
  "status": "Success",
  "deployedBy": "github-actions",
  "commitSha": "<sha-do-commit>",
  "pipelineUrl": "<url-da-action>",
  "startedAt": "<inicio>",
  "finishedAt": "<fim>"
}
```

Assim, a DeployTrack passa a registrar o historico de publicacoes da Orders API.
