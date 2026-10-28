locals {
  resource_group_name    = "rg-${var.workload}-${var.environment}-${var.location}"
  public_hostname        = "talkwithtiles.com"
  entra_app_display_name = "${var.workload}-${var.environment}-web"
  entra_redirect_uris = [
    "https://${local.public_hostname}/signin-oidc",
    "https://www.${local.public_hostname}/signin-oidc",
    "https://localhost:5001/signin-oidc",
  ]
  entra_logout_url = "https://${local.public_hostname}/signout-callback-oidc"
}
