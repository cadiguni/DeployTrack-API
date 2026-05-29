variable "project_name" {
  type = string
}

variable "environment" {
  type = string
}

variable "private_subnet_ids" {
  type = list(string)
}

variable "security_group_id" {
  type = string
}

variable "database_name" {
  type    = string
  default = "deploytrack"
}

variable "master_username" {
  type    = string
  default = "deploytrack"
}

variable "instance_class" {
  type    = string
  default = "db.t4g.micro"
}

variable "allocated_storage" {
  type    = number
  default = 20
}

variable "backup_retention_period" {
  type        = number
  default     = 0
  description = "Number of days to retain automated RDS backups. Use 0 for free-tier restricted accounts."
}
