output "dns_name" {
  value = aws_lb.this.dns_name
}

output "listener_arn" {
  value = aws_lb_listener.http.arn
}

output "deploytrack_target_group_arn" {
  value = aws_lb_target_group.deploytrack.arn
}

output "orders_target_group_arn" {
  value = aws_lb_target_group.orders.arn
}
