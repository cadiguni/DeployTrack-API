locals {
  tags = {
    Project     = var.project_name
    Environment = var.environment
    ManagedBy   = "terraform"
  }
}

resource "aws_cloudwatch_log_group" "deploytrack" {
  name              = "/ecs/${var.project_name}-${var.environment}/deploytrack-api"
  retention_in_days = var.retention_in_days

  tags = local.tags
}

resource "aws_cloudwatch_log_group" "orders" {
  name              = "/ecs/${var.project_name}-${var.environment}/orders-api"
  retention_in_days = var.retention_in_days

  tags = local.tags
}
