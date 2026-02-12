# Copilot Instructions for talkwithtiles

## Project Overview

TalkWithTiles is an online Scrabble-like game platform. The infrastructure is provisioned using Terraform and deployed to Azure through GitHub Actions workflows.

## Repository Structure

- `terraform/` - Terraform configuration files for Azure infrastructure provisioning
- `.github/workflows/` - GitHub Actions workflow definitions for CI/CD
- `docs/` - Additional project documentation

## Technology Stack

- **Infrastructure as Code**: Terraform
- **Cloud Provider**: Azure (using OIDC-based authentication)
- **CI/CD**: GitHub Actions
- **Dependency Management**: Dependabot with auto-merge support

## Development Guidelines

- Follow existing code patterns and conventions in the repository.
- Terraform code should use variable files (`tfvars/`) and backend configurations (`backends/`).
- All infrastructure changes should be validated with a `terraform plan` before applying.
- Keep GitHub Actions workflows minimal and leverage reusable actions from `frasermolyneux/actions`.

## Workflow Overview

- **Build and Test**: Runs Terraform plan on feature, bugfix, and hotfix branches against the development environment.
- **Code Quality**: Runs security scanning and dependency review on pushes to `main` and pull requests.
- **Copilot Setup Steps**: Configures the environment for GitHub Copilot coding agent.
- **Dependabot Auto-Merge**: Automatically merges Dependabot PRs via squash merge.

## Security and Permissions

- Workflows follow the principle of least privilege with explicitly scoped permissions.
- Azure authentication uses OpenID Connect (OIDC) with `id-token: write` permission.
- Secrets and credentials must never be hardcoded; use GitHub Actions secrets and variables.

## Contributing

This is a personal learning project. Contributions are not actively sought, but constructive feedback and issue reports are welcome.
