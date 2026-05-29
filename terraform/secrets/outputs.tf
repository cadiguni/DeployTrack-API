output "jwt_signing_key_secret_arn" {
  value = aws_secretsmanager_secret.jwt_signing_key.arn
}

output "jwt_issuer" {
  value = var.jwt_issuer
}

output "jwt_audience" {
  value = var.jwt_audience
}
