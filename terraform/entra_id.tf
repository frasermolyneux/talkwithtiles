resource "azuread_application" "web" {
  display_name     = local.entra_app_display_name
  description      = "Talk With Tiles web front-end"
  sign_in_audience = "AzureADandPersonalMicrosoftAccount"

  app_role {
    allowed_member_types = ["User"]
    description          = "Administrators can manage games and view analytics."
    display_name         = "Admin"
    enabled              = true
    id                   = "e1b2c3d4-5678-9abc-def0-123456789abc"
    value                = "Admin"
  }

  web {
    homepage_url  = "https://${local.public_hostname}/"
    logout_url    = local.entra_logout_url
    redirect_uris = local.entra_redirect_uris

    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = true
    }
  }

  prevent_duplicate_names = true
}

resource "azuread_service_principal" "web" {
  client_id                    = azuread_application.web.client_id
  app_role_assignment_required = false

  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_application_password" "web" {
  application_id = azuread_application.web.id

  rotate_when_changed = {
    rotation = time_rotating.thirty_days.id
  }
}
