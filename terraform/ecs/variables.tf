variable "project_name" {
  type = string
}

variable "environment" {
  type = string
}

variable "aws_region" {
  type = string
}

variable "private_subnet_ids" {
  type = list(string)
}

variable "security_group_id" {
  type = string
}

variable "deploytrack_target_group_arn" {
  type = string
}

variable "orders_target_group_arn" {
  type = string
}

variable "deploytrack_image" {
  type = string
}

variable "orders_image" {
  type = string
}

variable "deploytrack_log_group_name" {
  type = string
}

variable "orders_log_group_name" {
  type = string
}

variable "database_host" {
  type = string
}

variable "database_port" {
  type = number
}

variable "database_name" {
  type = string
}

variable "database_username" {
  type = string
}

variable "database_secret_arn" {
  type = string
}

variable "jwt_issuer" {
  type = string
}

variable "jwt_audience" {
  type = string
}

variable "jwt_signing_key_secret_arn" {
  type = string
}

variable "deploytrack_desired_count" {
  type    = number
  default = 1
}

variable "orders_desired_count" {
  type    = number
  default = 1
}
