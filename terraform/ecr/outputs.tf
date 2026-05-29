output "deploytrack_repository_name" {
  value = aws_ecr_repository.deploytrack.name
}

output "deploytrack_repository_url" {
  value = aws_ecr_repository.deploytrack.repository_url
}

output "orders_repository_name" {
  value = aws_ecr_repository.orders.name
}

output "orders_repository_url" {
  value = aws_ecr_repository.orders.repository_url
}
