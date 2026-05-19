# GitHub Actions

Os workflows executaveis ficam em `.github/workflows`, porque esse e o caminho que o GitHub Actions reconhece automaticamente.

Esta pasta guarda a documentacao das pipelines e as decisoes de CI/CD.

## Workflows

- `ci.yml`: restore, build e test da solution.
- `deploy-dev.yml`: build da imagem Docker, push para Amazon ECR e atualizacao do servico no ECS Fargate.

## Autenticacao com AWS

O deploy usa OpenID Connect entre GitHub Actions e AWS IAM. Com isso, o repositorio nao precisa armazenar `AWS_ACCESS_KEY_ID` nem `AWS_SECRET_ACCESS_KEY`.

Configure no ambiente `dev` do GitHub o secret:

```text
AWS_ROLE_TO_ASSUME=arn:aws:iam::<account-id>:role/<github-actions-deploy-role>
```

A role precisa confiar no identity provider OIDC do GitHub e permitir push no ECR, leitura/escrita da task definition e update do ECS service.
