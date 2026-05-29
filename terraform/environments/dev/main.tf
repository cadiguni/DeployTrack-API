data "aws_caller_identity" "current" {}

data "aws_iam_openid_connect_provider" "github" {
  count = var.create_github_oidc_provider ? 0 : 1

  url = "https://token.actions.githubusercontent.com"
}

module "network" {
  source = "../../network"

  project_name       = var.project_name
  environment        = var.environment
  vpc_cidr           = var.vpc_cidr
  enable_nat_gateway = var.enable_nat_gateway
}

module "ecr" {
  source = "../../ecr"

  project_name = var.project_name
  environment  = var.environment
}

module "monitoring" {
  source = "../../monitoring"

  project_name = var.project_name
  environment  = var.environment
}

module "secrets" {
  source = "../../secrets"

  project_name = var.project_name
  environment  = var.environment
}

module "rds" {
  source = "../../rds"

  project_name            = var.project_name
  environment             = var.environment
  private_subnet_ids      = module.network.private_subnet_ids
  security_group_id       = module.network.rds_security_group_id
  instance_class          = var.rds_instance_class
  backup_retention_period = var.rds_backup_retention_period
}

module "alb" {
  source = "../../alb"

  project_name      = var.project_name
  environment       = var.environment
  vpc_id            = module.network.vpc_id
  public_subnet_ids = module.network.public_subnet_ids
  security_group_id = module.network.alb_security_group_id
}

module "ecs" {
  source = "../../ecs"

  project_name                 = var.project_name
  environment                  = var.environment
  aws_region                   = var.aws_region
  private_subnet_ids           = module.network.private_subnet_ids
  security_group_id            = module.network.ecs_security_group_id
  deploytrack_target_group_arn = module.alb.deploytrack_target_group_arn
  orders_target_group_arn      = module.alb.orders_target_group_arn

  deploytrack_image = "${module.ecr.deploytrack_repository_url}:${var.deploytrack_image_tag}"
  orders_image      = "${module.ecr.orders_repository_url}:${var.orders_image_tag}"

  deploytrack_log_group_name = module.monitoring.deploytrack_log_group_name
  orders_log_group_name      = module.monitoring.orders_log_group_name

  database_host       = module.rds.database_address
  database_port       = module.rds.database_port
  database_name       = module.rds.database_name
  database_username   = module.rds.master_username
  database_secret_arn = module.rds.master_user_secret_arn

  jwt_issuer                 = module.secrets.jwt_issuer
  jwt_audience               = module.secrets.jwt_audience
  jwt_signing_key_secret_arn = module.secrets.jwt_signing_key_secret_arn

  depends_on = [module.alb]
}

resource "aws_iam_openid_connect_provider" "github" {
  count = var.create_github_oidc_provider ? 1 : 0

  url = "https://token.actions.githubusercontent.com"

  client_id_list = [
    "sts.amazonaws.com"
  ]

  thumbprint_list = [
    var.github_oidc_thumbprint
  ]
}

locals {
  github_oidc_provider_arn = var.create_github_oidc_provider ? aws_iam_openid_connect_provider.github[0].arn : data.aws_iam_openid_connect_provider.github[0].arn
}

resource "aws_iam_role" "github_actions_deploy" {
  name = "${var.project_name}-${var.environment}-github-actions-deploy"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Federated = local.github_oidc_provider_arn
        }
        Action = "sts:AssumeRoleWithWebIdentity"
        Condition = {
          StringEquals = {
            "token.actions.githubusercontent.com:aud" = "sts.amazonaws.com"
          }
          StringLike = {
            "token.actions.githubusercontent.com:sub" = [
              "repo:${var.github_repository}:ref:refs/heads/main",
              "repo:${var.github_repository}:environment:${var.environment}"
            ]
          }
        }
      }
    ]
  })
}

resource "aws_iam_role_policy" "github_actions_deploy" {
  name = "${var.project_name}-${var.environment}-github-actions-deploy"
  role = aws_iam_role.github_actions_deploy.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "ecr:GetAuthorizationToken"
        ]
        Resource = "*"
      },
      {
        Effect = "Allow"
        Action = [
          "ecr:BatchCheckLayerAvailability",
          "ecr:CompleteLayerUpload",
          "ecr:InitiateLayerUpload",
          "ecr:PutImage",
          "ecr:UploadLayerPart"
        ]
        Resource = [
          "arn:aws:ecr:${var.aws_region}:${data.aws_caller_identity.current.account_id}:repository/${module.ecr.deploytrack_repository_name}",
          "arn:aws:ecr:${var.aws_region}:${data.aws_caller_identity.current.account_id}:repository/${module.ecr.orders_repository_name}"
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "ecs:DescribeServices",
          "ecs:DescribeTaskDefinition",
          "ecs:RegisterTaskDefinition",
          "ecs:UpdateService"
        ]
        Resource = "*"
      },
      {
        Effect = "Allow"
        Action = [
          "iam:PassRole"
        ]
        Resource = [
          module.ecs.task_execution_role_arn,
          module.ecs.task_role_arn
        ]
      }
    ]
  })
}
