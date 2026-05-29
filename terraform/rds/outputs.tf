output "database_name" {
  value = aws_db_instance.postgres.db_name
}

output "database_address" {
  value = aws_db_instance.postgres.address
}

output "database_port" {
  value = aws_db_instance.postgres.port
}

output "master_username" {
  value = aws_db_instance.postgres.username
}

output "master_user_secret_arn" {
  value = aws_db_instance.postgres.master_user_secret[0].secret_arn
}
