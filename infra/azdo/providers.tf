terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azuredevops = {
      source  = "microsoft/azuredevops"
      version = ">= 1.0.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 3.113.0"
    }
  }
}

provider "azuredevops" {
  # Authentication via AZDO_PERSONAL_ACCESS_TOKEN environment variable
  org_service_url = var.azdo_org_service_url
}

provider "azurerm" {
  features {}
  
  # Use OIDC authentication (Workload Identity Federation)
  use_oidc = true
}

