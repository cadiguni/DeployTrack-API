variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "project_name" {
  type    = string
  default = "deploytrack"
}

variable "environment" {
  type    = string
  default = "dev"
}

variable "github_repository" {
  type        = string
  description = "GitHub repository in owner/name format, for example ronaldo/deploytrack-api."
}

variable "vpc_cidr" {
  type    = string
  default = "10.20.0.0/16"
}

variable "deploytrack_image_tag" {
  type    = string
  default = "latest"
}

variable "orders_image_tag" {
  type    = string
  default = "latest"
}

variable "enable_nat_gateway" {
  type    = bool
  default = true
}

variable "rds_instance_class" {
  type    = string
  default = "db.t4g.micro"
}

variable "rds_backup_retention_period" {
  type        = number
  default     = 0
  description = "Number of days to retain RDS automated backups. Keep 0 for free-tier restricted accounts."
}

variable "github_oidc_thumbprint" {
  type    = string
  default = "6938fd4d98bab03faadb97b34396831e3780aea1"
}

variable "create_github_oidc_provider" {
  type        = bool
  default     = false
  description = "Set to true only when the AWS account does not already have the GitHub Actions OIDC provider."
}
