variable "runtime_host_principal_id" {
  description = "Azure Arc principal allowed to hydrate Production runtime secrets."
  type        = string
  default     = null

  validation {
    condition     = var.environment != "prd" || can(regex("^[0-9a-fA-F-]{36}$", var.runtime_host_principal_id))
    error_message = "Production requires the approved runtime host principal ID."
  }
}

variable "runtime_host_ipv4_cidrs" {
  description = "Approved fixed IPv4 egress ranges for Production vault hydration."
  type        = list(string)
  default     = []

  validation {
    condition = var.environment != "prd" || (
      length(var.runtime_host_ipv4_cidrs) > 0 &&
      alltrue([for cidr in var.runtime_host_ipv4_cidrs : can(cidrnetmask(cidr)) && endswith(cidr, "/32")])
    )
    error_message = "Production requires explicit, individual IPv4 host addresses."
  }
}

resource "azurerm_key_vault" "runtime" {
  count = var.environment == "prd" ? 1 : 0

  name                       = "kv-${random_id.environment_id.hex}"
  location                   = data.azurerm_resource_group.rg.location
  resource_group_name        = data.azurerm_resource_group.rg.name
  tenant_id                  = data.azuread_client_config.current.tenant_id
  sku_name                   = "standard"
  rbac_authorization_enabled = true
  purge_protection_enabled   = true
  soft_delete_retention_days = 90
  tags                       = merge(var.tags, { Purpose = "workload-secrets" })

  # checkov:skip=CKV2_AZURE_32:The off-Azure host uses its fixed public egress allowlist rather than Private Link.
  # checkov:skip=CKV_AZURE_189:The public endpoint is required for off-Azure hydration, restricted by RBAC and host IPv4.
  network_acls {
    bypass         = "AzureServices"
    default_action = "Deny"
    ip_rules       = var.runtime_host_ipv4_cidrs
  }
}

resource "azurerm_role_assignment" "runtime_deploy_secrets_officer" {
  count = var.environment == "prd" ? 1 : 0

  scope                = azurerm_key_vault.runtime[0].id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azuread_client_config.current.object_id
}

resource "azurerm_role_assignment" "runtime_host_secrets_user" {
  count = var.environment == "prd" ? 1 : 0

  scope                = azurerm_key_vault.runtime[0].id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.runtime_host_principal_id
}
