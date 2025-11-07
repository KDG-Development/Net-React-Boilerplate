variable "azdo_org_service_url" {
  description = "Azure DevOps organization URL"
  type        = string
}

variable "azdo_project_name" {
  description = "Azure DevOps project name"
  type        = string
}

variable "environment" {
  description = "Environment name (qa, prod)"
  type        = string
  validation {
    condition     = contains(["qa", "prod"], var.environment)
    error_message = "Environment must be either 'qa' or 'prod'."
  }
}

variable "project_name" {
  description = "Project name used in resource naming"
  type        = string
}

variable "tfstate_rg" {
  description = "Resource group name for Terraform state storage"
  type        = string
}

variable "tfstate_account" {
  description = "Storage account name for Terraform state"
  type        = string
}

variable "tfstate_container" {
  description = "Container name for Terraform state"
  type        = string
  default     = "tfstate"
}

variable "subscription_id" {
  description = "Azure subscription ID"
  type        = string
}

variable "tenant_id" {
  description = "Azure AD tenant ID"
  type        = string
}

