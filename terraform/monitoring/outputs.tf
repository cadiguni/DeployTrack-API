output "deploytrack_log_group_name" {
  value = aws_cloudwatch_log_group.deploytrack.name
}

output "orders_log_group_name" {
  value = aws_cloudwatch_log_group.orders.name
}
