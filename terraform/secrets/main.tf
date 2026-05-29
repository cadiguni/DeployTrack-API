resource "random_password" "jwt_signing_key" {
  length  = 48
  special = true
}

locals {
  tags = {
    Project     = var.project_name
    Environment = var.environment
    ManagedBy   = "terraform"
  }
}

resource "aws_secretsmanager_secret" "jwt_signing_key" {
  name        = "${var.project_name}/${var.environment}/deploytrack/jwt-signing-key"
  description = "JWT signing key used by the DeployTrack API."

  tags = local.tags
}

resource "aws_secretsmanager_secret_version" "jwt_signing_key" {
  secret_id     = aws_secretsmanager_secret.jwt_signing_key.id
  secret_string = random_password.jwt_signing_key.result
}
