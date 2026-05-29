# Etapa 5 - Pipelines CI/CD

Esta etapa cria duas pipelines de deploy com GitHub Actions.

## Autenticacao AWS

As pipelines usam OpenID Connect, OIDC.

Fluxo:

```text
GitHub Actions
  -> solicita token OIDC
  -> assume role IAM na AWS
  -> acessa ECR e ECS temporariamente
```

Nao e necessario salvar credenciais permanentes da AWS no GitHub.

O unico secret AWS necessario e:

```text
AWS_ROLE_TO_ASSUME
```

Esse valor vem do output Terraform `github_actions_role_arn`.

### Erro comum: Credentials could not be loaded

Se o GitHub Actions falhar no passo `aws-actions/configure-aws-credentials@v4` com:

```text
Credentials could not be loaded, please check your action inputs
```

normalmente significa uma destas situacoes:

- O secret `AWS_ROLE_TO_ASSUME` ainda nao foi configurado no GitHub.
- O workflow rodado ainda era uma versao antiga, sem `role-to-assume`.
- A role OIDC ainda nao foi criada com Terraform.
- A trust policy da role nao permite o repositorio, branch ou environment usados no workflow.

Neste projeto, os jobs usam `environment: dev`, entao o secret pode ser configurado em:

```text
GitHub repository -> Settings -> Environments -> dev -> Environment secrets
```

Crie:

```text
AWS_ROLE_TO_ASSUME=<output github_actions_role_arn do Terraform>
```

Tambem e possivel configurar em:

```text
GitHub repository -> Settings -> Secrets and variables -> Actions -> Repository secrets
```

mas, como o workflow usa `environment: dev`, manter o secret no environment `dev` deixa o acesso mais controlado.

## DeployTrack API

Workflow:

```text
.github/workflows/deploy-dev.yml
```

Fluxo:

```text
Push na main
  -> restore
  -> build
  -> test
  -> build Docker image
  -> push para ECR
  -> render ECS task definition
  -> update ECS service
  -> validar health check
```

Variaveis usadas:

- `AWS_ROLE_TO_ASSUME`: secret do GitHub.
- `DEPLOYTRACK_HEALTH_URL`: environment variable do GitHub, com valor do output Terraform `deploytrack_health_url`.

## Orders API

Workflow:

```text
.github/workflows/deploy-orders-dev.yml
```

Fluxo:

```text
Push na main
  -> restore
  -> build
  -> test
  -> build Docker image
  -> push para ECR
  -> login na DeployTrack
  -> garantir cadastro da aplicacao orders-api
  -> criar deployment Running
  -> render ECS task definition
  -> update ECS service
  -> validar health check
  -> marcar deployment Success ou Failed
```

Variaveis e secrets usados:

- `AWS_ROLE_TO_ASSUME`: secret do GitHub.
- `ORDERS_HEALTH_URL`: environment variable do GitHub, com valor do output Terraform `orders_health_url`.
- `DEPLOYTRACK_API_BASE_URL`: environment variable do GitHub, por exemplo `http://<alb-dns>`.
- `DEPLOYTRACK_EMAIL`: secret do GitHub para login na DeployTrack.
- `DEPLOYTRACK_PASSWORD`: secret do GitHub para login na DeployTrack.

## Registro de deploy da Orders API

Antes de atualizar o ECS service da Orders API, a pipeline cria:

```text
status = Running
```

Se tudo passar, atualiza para:

```text
status = Success
```

Se alguma etapa falhar depois do registro, atualiza para:

```text
status = Failed
```

Isso demonstra o principal valor da DeployTrack: rastrear deploys reais de outra aplicacao.
