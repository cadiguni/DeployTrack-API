# Terraform

Estrutura reservada para provisionar a infraestrutura AWS da DeployTrack API.

## Alvo inicial

- VPC e subnets
- Security groups
- ECR
- ECS Fargate
- Application Load Balancer
- RDS PostgreSQL
- Secrets Manager
- CloudWatch Logs
- IAM role para GitHub Actions via OIDC

## Estrutura sugerida

```text
infra/terraform/
|-- environments/
|   |-- dev/
|   `-- prod/
`-- modules/
    |-- networking/
    |-- ecr/
    |-- ecs/
    |-- rds/
    |-- alb/
    `-- iam-github-oidc/
```
