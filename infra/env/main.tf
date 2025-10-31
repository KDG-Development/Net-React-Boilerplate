locals {
  name = "${var.project_name}-${var.environment}"
  tags = {
    project = var.project_name
    env     = var.environment
    managed = "terraform"
  }
}

resource "random_string" "suffix" {
  length  = 5
  upper   = false
  lower   = true
  numeric = true
  special = false
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${local.name}"
  location = var.location
  tags     = local.tags
}

resource "azurerm_key_vault" "this" {
  name                        = substr(replace("kv-${local.name}-${random_string.suffix.result}", "_", "-"), 0, 24)
  resource_group_name         = azurerm_resource_group.this.name
  location                    = azurerm_resource_group.this.location
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  purge_protection_enabled    = true
  soft_delete_retention_days  = 7
  rbac_authorization_enabled  = true
  tags                        = local.tags
}

data "azurerm_client_config" "current" {}

# Grant the service principal (running Terraform) permission to manage secrets
resource "azurerm_role_assignment" "terraform_kv_admin" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azurerm_client_config.current.object_id
}

resource "azurerm_container_registry" "this" {
  name                = replace("acr${local.name}${random_string.suffix.result}", "-", "")
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = "Basic"
  admin_enabled       = false
  tags                = local.tags
}

resource "azurerm_postgresql_flexible_server" "this" {
  name                   = "pg-${local.name}-${random_string.suffix.result}"
  resource_group_name    = azurerm_resource_group.this.name
  location               = azurerm_resource_group.this.location
  administrator_login    = var.postgres_admin_username
  administrator_password = var.postgres_admin_password
  version                = "16"
  sku_name               = "B_Standard_B1ms"
  storage_mb             = 32768
  backup_retention_days  = 7
  
  tags = local.tags
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure" {
  name             = "allow-azure-services"
  server_id        = azurerm_postgresql_flexible_server.this.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_postgresql_flexible_server_database" "app" {
  name      = "${var.project_name}_db"
  server_id = azurerm_postgresql_flexible_server.this.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

resource "azurerm_service_plan" "this" {
  name                = "asp-${local.name}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_linux_web_app" "this" {
  name                = "app-${local.name}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  service_plan_id     = azurerm_service_plan.this.id
  https_only          = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on = true
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
  }
}

resource "azurerm_role_assignment" "webapp_kv_reader" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_web_app.this.identity[0].principal_id
}

resource "azurerm_key_vault_secret" "jwt_key" {
  name         = "jwt-key-${var.environment}"
  value        = random_string.suffix.result
  key_vault_id = azurerm_key_vault.this.id
  
  depends_on = [azurerm_role_assignment.terraform_kv_admin]
}

resource "azurerm_key_vault_secret" "pg_password" {
  name         = "postgres-admin-password-${var.environment}"
  value        = var.postgres_admin_password
  key_vault_id = azurerm_key_vault.this.id
}

resource "azurerm_key_vault_secret" "pg_connstr" {
  name         = "postgres-connection-string-${var.environment}"
  value        = "Host=${azurerm_postgresql_flexible_server.this.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.app.name};Username=${var.postgres_admin_username};Password=${var.postgres_admin_password}"
  key_vault_id = azurerm_key_vault.this.id
}

resource "azurerm_key_vault_secret" "jwt_issuer" {
  name         = "jwt-issuer-${var.environment}"
  value        = var.project_name
  key_vault_id = azurerm_key_vault.this.id
}

resource "azurerm_key_vault_secret" "jwt_audience" {
  name         = "jwt-audience-${var.environment}"
  value        = "${var.project_name}-client"
  key_vault_id = azurerm_key_vault.this.id
}

resource "azurerm_key_vault_secret" "app_base_url" {
  name         = "app-base-url-${var.environment}"
  value        = "https://${azurerm_linux_web_app.this.default_hostname}"
  key_vault_id = azurerm_key_vault.this.id
}

output "kv" {
  value = {
    name = azurerm_key_vault.this.name
    id   = azurerm_key_vault.this.id
  }
}

output "webapp" {
  value = {
    name     = azurerm_linux_web_app.this.name
    hostname = azurerm_linux_web_app.this.default_hostname
    rg       = azurerm_resource_group.this.name
  }
}


