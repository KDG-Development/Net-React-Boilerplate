variable "project_name" {
  description = "Short project name used in resource names"
  type        = string
}

variable "environment" {
  description = "Environment name (qa, prod)"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "eastus"
}

variable "postgres_admin_username" {
  description = "Admin username for PostgreSQL"
  type        = string
  default     = "pgadmin"
}

variable "postgres_admin_password" {
  description = "Admin password for PostgreSQL"
  type        = string
  sensitive   = true
}


