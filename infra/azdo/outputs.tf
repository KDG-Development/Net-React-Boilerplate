output "acr_service_connection_name" {
  description = "Name of the ACR service connection"
  value       = azuredevops_serviceendpoint_azurecr.acr.service_endpoint_name
}

output "variable_group_name" {
  description = "Name of the application variable group"
  value       = azuredevops_variable_group.app_vars.name
}

output "variable_group_id" {
  description = "ID of the application variable group"
  value       = azuredevops_variable_group.app_vars.id
}

