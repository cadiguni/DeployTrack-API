output "alb_dns_name" {
  value = module.alb.dns_name
}

output "github_actions_role_arn" {
  value = aws_iam_role.github_actions_deploy.arn
}

output "deploytrack_ecr_repository" {
  value = module.ecr.deploytrack_repository_name
}

output "orders_ecr_repository" {
  value = module.ecr.orders_repository_name
}

output "ecs_cluster_name" {
  value = module.ecs.cluster_name
}

output "deploytrack_ecs_service_name" {
  value = module.ecs.deploytrack_service_name
}

output "orders_ecs_service_name" {
  value = module.ecs.orders_service_name
}

output "deploytrack_task_definition_family" {
  value = module.ecs.deploytrack_task_definition_family
}

output "orders_task_definition_family" {
  value = module.ecs.orders_task_definition_family
}

output "deploytrack_health_url" {
  value = "http://${module.alb.dns_name}/health"
}

output "orders_health_url" {
  value = "http://${module.alb.dns_name}/orders/health"
}
