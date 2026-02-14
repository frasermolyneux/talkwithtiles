resource "azurerm_linux_web_app" "app" {
  name = local.web_app_name
  tags = var.tags

  resource_group_name = local.platform_hosting_app_service_plan.resource_group_name
  location            = local.platform_hosting_app_service_plan.location

  service_plan_id = local.platform_hosting_app_service_plan.id

  https_only = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = "9.0"
    }

    always_on                         = true
    ftps_state                        = "Disabled"
    minimum_tls_version               = "1.2"
    http2_enabled                     = true
    health_check_path                 = "/api/health"
    health_check_eviction_time_in_min = 5
  }

  logs {
    detailed_error_messages = true
    failed_request_tracing  = true

    http_logs {
      file_system {
        retention_in_days = 7
        retention_in_mb   = 35
      }
    }
  }

  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING"       = azurerm_application_insights.ai.connection_string
    "ApplicationInsights__ClientConnectionString" = azurerm_application_insights.ai.connection_string
    "ApplicationInsightsAgent_EXTENSION_VERSION"  = "~3"
    "ASPNETCORE_ENVIRONMENT"                      = var.environment == "prd" ? "Production" : "Development"
    "WEBSITE_RUN_FROM_PACKAGE"                    = "1"
    "AzureAd__Instance"                           = "https://login.microsoftonline.com/"
    "AzureAd__Domain"                             = var.tenant_domain
    "AzureAd__TenantId"                           = "common"
    "AzureAd__ClientId"                           = azuread_application.web.client_id
    "AzureAd__ClientSecret"                       = azuread_application_password.web.value
    "AzureAd__CallbackPath"                       = "/signin-oidc"
    "AppData__StorageAccountUri"                   = azurerm_storage_account.data.primary_table_endpoint
    "AppData__GameStateTableName"                  = azurerm_storage_table.tables["scrabble"].name
    "AppData__GameStateIndexTableName"             = azurerm_storage_table.tables["scrabbleindex"].name
    "AppData__TilesTableName"                      = azurerm_storage_table.tables["scrabbletiles"].name
    "AppData__GameInviteTableName"                 = azurerm_storage_table.tables["gameinvites"].name
    "AppData__ContactsTableName"                   = azurerm_storage_table.tables["contacts"].name
  }
}

resource "azurerm_app_service_custom_hostname_binding" "primary" {
  hostname            = local.public_hostname
  app_service_name    = azurerm_linux_web_app.app.name
  resource_group_name = azurerm_linux_web_app.app.resource_group_name

  depends_on = [
    azurerm_dns_txt_record.app_service_verification,
    azurerm_dns_cname_record.web_app
  ]
}

resource "time_sleep" "wait_for_hostname_binding" {
  create_duration = "60s"

  depends_on = [
    azurerm_app_service_custom_hostname_binding.primary
  ]
}

resource "azurerm_app_service_managed_certificate" "primary" {
  custom_hostname_binding_id = azurerm_app_service_custom_hostname_binding.primary.id

  depends_on = [
    time_sleep.wait_for_hostname_binding,
    azurerm_dns_cname_record.web_app
  ]
}

resource "azurerm_app_service_certificate_binding" "primary" {
  hostname_binding_id = azurerm_app_service_custom_hostname_binding.primary.id
  certificate_id      = azurerm_app_service_managed_certificate.primary.id
  ssl_state           = "SniEnabled"
}
