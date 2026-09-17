locals {
  resource_group_name = "rg-${var.workload}-${var.environment}-${var.location}"
  development_service_plan = {
    id                  = one(azurerm_service_plan.development[*].id)
    location            = one(azurerm_service_plan.development[*].location)
    name                = one(azurerm_service_plan.development[*].name)
    resource_group_name = one(azurerm_service_plan.development[*].resource_group_name)
  }
  production_service_plan = var.environment == "prd" ? {
    id                  = data.terraform_remote_state.platform_hosting[0].outputs.app_service_plans["default"].id
    location            = data.terraform_remote_state.platform_hosting[0].outputs.app_service_plans["default"].location
    name                = data.terraform_remote_state.platform_hosting[0].outputs.app_service_plans["default"].name
    resource_group_name = data.terraform_remote_state.platform_hosting[0].outputs.app_service_plans["default"].resource_group_name
  } : null
  selected_service_plan            = var.environment == "dev" ? local.development_service_plan : local.production_service_plan
  platform_monitoring_workspace_id = data.terraform_remote_state.platform_monitoring.outputs.log_analytics.id
  web_app_name                     = "app-${var.workload}-${var.environment}-${var.location}-${random_id.environment_id.hex}"
  app_insights_name                = "ai-${var.workload}-${var.environment}-${var.location}"
  public_hostname                  = "${var.dns.subdomain}.${var.dns.domain}"
  entra_app_display_name           = "${var.workload}-${var.environment}-web"
  entra_redirect_uris = distinct([
    "https://${local.public_hostname}/signin-oidc",
    "https://${local.web_app_name}.azurewebsites.net/signin-oidc",
    "https://localhost:5001/signin-oidc"
  ])
  entra_logout_url       = "https://${local.public_hostname}/signout-callback-oidc"
  storage_account_prefix = substr(replace(var.workload, "-", ""), 0, 8)
  storage_account_name   = lower("st${local.storage_account_prefix}${var.environment}${random_id.storage.hex}")
  storage_table_names = {
    scrabble      = "Scrabble"
    scrabbleindex = "ScrabbleIndex"
    scrabbletiles = "ScrabbleTiles"
    gameinvites   = "GameInvites"
    contacts      = "Contacts"
  }

  app_insights_sampling_percentage = {
    dev = 25
    prd = 75
  }
}
