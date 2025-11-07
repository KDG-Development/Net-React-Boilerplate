# Read infrastructure state from infra/env
data "terraform_remote_state" "infra" {
  backend = "azurerm"
  config = {
    resource_group_name  = var.tfstate_rg
    storage_account_name = var.tfstate_account
    container_name       = var.tfstate_container
    key                  = "${var.project_name}-${var.environment}.tfstate"
    use_oidc             = true
    use_azuread_auth     = true
  }
}

# Get Azure DevOps project data
data "azuredevops_project" "project" {
  name = var.azdo_project_name
}

# Get existing Azure Resource Manager service connection
data "azuredevops_serviceendpoint_azurerm" "arm" {
  project_id            = data.azuredevops_project.project.id
  service_endpoint_name = var.azure_service_connection_name
}

# Create Azure Container Registry service connection
resource "azuredevops_serviceendpoint_azurecr" "acr" {
  project_id            = data.azuredevops_project.project.id
  service_endpoint_name = "sc-acr-${var.project_name}-${var.environment}"
  description           = "Service connection to ${var.environment} ACR (managed by Terraform)"
  
  resource_group             = data.terraform_remote_state.infra.outputs.webapp.rg
  azurecr_name               = data.terraform_remote_state.infra.outputs.acr.name
  azurecr_subscription_id    = var.subscription_id
  azurecr_subscription_name  = "Azure Subscription"
  azurecr_spn_tenantid       = var.tenant_id
}

# Create application variable group
resource "azuredevops_variable_group" "app_vars" {
  project_id   = data.azuredevops_project.project.id
  name         = "${var.environment}-app-vars"
  description  = "Deployment variables for ${var.environment} environment (managed by Terraform)"
  allow_access = true

  variable {
    name  = "dockerRegistryServiceConnection"
    value = azuredevops_serviceendpoint_azurecr.acr.service_endpoint_name
  }

  variable {
    name  = "azureResourceServiceConnection"
    value = var.azure_service_connection_name
  }

  variable {
    name  = "resourceGroupName"
    value = data.terraform_remote_state.infra.outputs.webapp.rg
  }

  variable {
    name  = "webAppName"
    value = data.terraform_remote_state.infra.outputs.webapp.name
  }

  variable {
    name  = "postgresServerName"
    value = data.terraform_remote_state.infra.outputs.postgres.name
  }

  variable {
    name  = "imageRepository"
    value = var.project_name
  }

  variable {
    name  = "environment"
    value = var.environment
  }
}

