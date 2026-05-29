output "cluster_name" {
  value = aws_ecs_cluster.this.name
}

output "deploytrack_service_name" {
  value = aws_ecs_service.deploytrack.name
}

output "orders_service_name" {
  value = aws_ecs_service.orders.name
}

output "deploytrack_container_name" {
  value = "deploytrack-api"
}

output "orders_container_name" {
  value = "orders-api"
}

output "task_execution_role_arn" {
  value = aws_iam_role.execution.arn
}

output "task_role_arn" {
  value = aws_iam_role.task.arn
}

output "deploytrack_task_definition_family" {
  value = aws_ecs_task_definition.deploytrack.family
}

output "orders_task_definition_family" {
  value = aws_ecs_task_definition.orders.family
}
