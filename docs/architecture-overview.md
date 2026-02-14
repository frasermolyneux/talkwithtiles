# Architecture Overview

## Application

Talk With Tiles is an ASP.NET Core 9 MVC web application that provides an online platform for playing tile-based word games (Scrabble-style). The solution is structured as a multi-project .NET solution:

| Project | Purpose |
|---|---|
| `MX.TalkWithTiles.Web` | ASP.NET Core MVC web application (controllers, views, DI configuration) |
| `MX.TalkWithTiles.CoreEngine` | Generic game engine orchestration (board, players, moves, scoring) |
| `MX.TalkWithTiles.Scrabble` | Scrabble-specific game logic and rules |
| `MX.TalkWithTiles.Repository` | Azure Table Storage data access layer |
| `MX.TalkWithTiles.Contracts` | DTOs, interfaces, state models, and constants |
| `MX.TalkWithTiles.Common` | Shared utility extensions |
| `MX.TalkWithTiles.CoreEngine.Tests` | Unit tests for the core engine |
| `MX.TalkWithTiles.Scrabble.Tests` | Unit tests for Scrabble logic |

## Authentication

The application uses Microsoft Entra ID (via `Microsoft.Identity.Web`) for authentication, configured with `TenantId: common` to support multi-tenant organisations and personal Microsoft accounts. Game-related controllers require authentication while public pages (Home, About, Health, Error) are accessible anonymously.

## Data Storage

Game state is persisted in Azure Table Storage using the `Azure.Data.Tables` SDK with `DefaultAzureCredential` for authentication. Data is spread across five tables:

- **Scrabble** - Game state records
- **ScrabbleIndex** - Game state index for efficient lookups
- **ScrabbleTiles** - Tile data
- **GameInvites** - Game invitation records
- **Contacts** - Player contact lists

The repository layer uses `ITableEntity` cloud entities that map between domain state models and Azure Table Storage rows.

## Telemetry

Application Insights is integrated for telemetry and monitoring, configured per-environment through Terraform.

## Infrastructure

Infrastructure is defined in Terraform under the `terraform/` directory and includes:

- **Azure App Service** (Linux, .NET 9.0) on a shared hosting plan from `platform-hosting`
- **Azure Storage Account** with the five game tables
- **Microsoft Entra ID** app registration with client credentials
- **Application Insights** instance
- **DNS records** (CNAME and TXT) for custom domain

Terraform state is stored remotely in Azure Storage with per-environment backend configurations (`backends/dev.backend.hcl`, `backends/prd.backend.hcl`).

## Deployment

The application is deployed via GitHub Actions to Azure App Service. The deployment pipeline promotes through dev and prd environments with Terraform plan-and-apply steps for infrastructure and application artifact deployment. See [Development Workflows](development-workflows.md) for CI/CD details.
