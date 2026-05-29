# Etapa 4 - Infraestrutura AWS com Terraform

Esta etapa cria a infraestrutura AWS que recebe as duas APIs containerizadas.

## Estrutura

```text
terraform/
|-- network/
|-- ecr/
|-- ecs/
|-- alb/
|-- rds/
|-- secrets/
|-- monitoring/
`-- environments/
    `-- dev/
```

## O que cada modulo cria

`network/` cria VPC, subnets publicas, subnets privadas, internet gateway, NAT gateway opcional, route tables e security groups.

`ecr/` cria dois repositorios:

- `deploytrack/deploytrack-api`
- `deploytrack/orders-api`

`alb/` cria o Application Load Balancer, target groups e regras:

- Trafego padrao vai para a DeployTrack API.
- `/orders` e `/orders/*` vao para a Orders API.

`rds/` cria um PostgreSQL privado em RDS. A senha principal e gerenciada pelo proprio RDS via AWS Secrets Manager com `manage_master_user_password = true`.

Em contas com restricoes de free tier, a AWS pode bloquear backup retention maior que zero. Por isso o ambiente `dev` usa `rds_backup_retention_period = 0` por padrao. Se sua conta permitir backups automatizados, aumente esse valor no `terraform.tfvars`.

`secrets/` cria o secret da chave JWT da DeployTrack.

`monitoring/` cria CloudWatch Log Groups para as duas tasks ECS.

`ecs/` cria:

- ECS cluster.
- Task definition da DeployTrack.
- Task definition da Orders API.
- ECS service da DeployTrack.
- ECS service da Orders API.
- IAM roles de task e execucao.

O ambiente `terraform/environments/dev` conecta todos esses modulos e tambem cria uma role IAM para GitHub Actions assumir via OIDC.

## Segredos

A senha do banco nao fica no codigo, no Dockerfile, no Compose ou no pipeline.

O fluxo e:

```text
RDS gera e gerencia senha principal
  -> senha fica no Secrets Manager
  -> ECS task recebe Database__Password como secret
  -> DeployTrack monta a connection string em runtime
```

A chave JWT segue a mesma ideia:

```text
Terraform gera JWT signing key
  -> grava no Secrets Manager
  -> ECS task injeta Jwt__SigningKey como secret
```

## OIDC GitHub Actions

O Terraform reutiliza por padrao o OIDC provider `token.actions.githubusercontent.com` caso ele ja exista na conta AWS, que e comum quando outro projeto ja usou GitHub Actions com OIDC.

Se a conta ainda nao tiver esse provider, defina no `terraform.tfvars`:

```hcl
create_github_oidc_provider = true
```

O Terraform cria ou usa:

- OIDC provider `token.actions.githubusercontent.com`.
- Role `deploytrack-dev-github-actions-deploy`.
- Policy para push no ECR, registrar task definition e atualizar services ECS.

Se aparecer `EntityAlreadyExists: Provider with url https://token.actions.githubusercontent.com already exists`, mantenha:

```hcl
create_github_oidc_provider = false
```

e rode `terraform apply` novamente.

Depois do `terraform apply`, copie o output `github_actions_role_arn` para o secret do GitHub:

```text
AWS_ROLE_TO_ASSUME=<arn-da-role>
```

Isso evita guardar `AWS_ACCESS_KEY_ID` e `AWS_SECRET_ACCESS_KEY` no GitHub.

## Como usar

Copie o arquivo de exemplo:

```bash
cp terraform/environments/dev/terraform.tfvars.example terraform/environments/dev/terraform.tfvars
```

Edite:

```hcl
github_repository = "seu-usuario/DeployTrack-API"
```

Inicialize:

```bash
terraform -chdir=terraform/environments/dev init
```

Planeje:

```bash
terraform -chdir=terraform/environments/dev plan
```

Aplique:

```bash
terraform -chdir=terraform/environments/dev apply
```

## Outputs importantes

Depois do apply, use os outputs para configurar o GitHub:

- `github_actions_role_arn` -> secret `AWS_ROLE_TO_ASSUME`
- `deploytrack_health_url` -> environment variable `DEPLOYTRACK_HEALTH_URL`
- `orders_health_url` -> environment variable `ORDERS_HEALTH_URL`
- `alb_dns_name` -> base URL publica do ALB

Para a pipeline da Orders API, configure tambem:

- `DEPLOYTRACK_API_BASE_URL`
- `DEPLOYTRACK_EMAIL`
- `DEPLOYTRACK_PASSWORD`

## Observacao de custo

Este ambiente cria recursos pagos, incluindo NAT Gateway, ALB, ECS Fargate e RDS.

Para reduzir custo em laboratorio, e possivel definir:

```hcl
enable_nat_gateway = false
```

Mas tasks ECS em subnets privadas precisam de caminho para ECR, CloudWatch e Secrets Manager. Sem NAT, voce precisaria adicionar VPC endpoints.
