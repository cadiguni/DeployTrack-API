variable "project_name" {
  type = string
}

variable "environment" {
  type = string
}

variable "jwt_issuer" {
  type    = string
  default = "DeployTrack"
}

variable "jwt_audience" {
  type    = string
  default = "DeployTrack"
}
