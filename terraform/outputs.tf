output "resource_group_name" {
  value = data.azurerm_resource_group.rg.name
}

output "entra_application_client_id" {
  value = azuread_application.web.client_id
}

output "entra_service_principal_object_id" {
  value = azuread_service_principal.web.object_id
}

output "entra_redirect_uris" {
  value = local.entra_redirect_uris
}
