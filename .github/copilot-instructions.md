# Copilot Instructions

- **Stack & auth**: ASP.NET Core 9 MVC (`src/MX.TalkWithTiles.Web`) with Microsoft.Identity.Web for Entra External ID. Game controllers require authentication; Home, About, Health, Error are anonymous.
- **Data**: Azure Table Storage via `TableServiceClient` + `DefaultAzureCredential`. Game state, tiles, invites, and contacts stored in separate tables. Configuration in `appsettings.json` `Storage` section.
- **Architecture**: Multi-project .NET solution with separation of concerns:
  - `MX.TalkWithTiles.Contracts` - DTOs, interfaces, models
  - `MX.TalkWithTiles.Common` - Shared utilities
  - `MX.TalkWithTiles.CoreEngine` - Generic game engine (board, players, moves, scoring)
  - `MX.TalkWithTiles.Scrabble` - Scrabble-specific game logic
  - `MX.TalkWithTiles.Repository` - Azure Table Storage data access
  - `MX.TalkWithTiles.Web` - ASP.NET Core MVC web application
- **Local dev loop**: `dotnet build src/MX.TalkWithTiles.sln` then `dotnet run --project src/MX.TalkWithTiles.Web/MX.TalkWithTiles.Web.csproj`. Ensure `Storage:TableEndpoint` and Entra settings are configured.
- **Testing**: `dotnet test src/MX.TalkWithTiles.sln` runs CoreEngine and Scrabble unit tests (NUnit + FakeItEasy + FluentAssertions).
- **Infra**: Terraform under `terraform/` builds App Service (on shared platform-hosting plan), Storage, DNS, Entra ID app, Application Insights (per-environment tfvars/backends). GitHub Actions workflows cover build/test, codequality, PR verify, deploy-dev/prd, destroy-development/environment, dependabot-automerge, and copilot-setup-steps.
- **Configuration**: `appsettings.json` holds `AzureAd`, `Storage`, `ApplicationInsights`. Keep secrets out of source; use user-secrets locally.
