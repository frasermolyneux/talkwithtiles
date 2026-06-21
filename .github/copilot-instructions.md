# Copilot Instructions

> Shared conventions: see [`.github-copilot/.github/instructions/terraform.instructions.md`](../../.github-copilot/.github/instructions/terraform.instructions.md) for the standard Terraform layout, providers, remote-state pattern, validation commands, and CI/CD workflows.

## Architecture

This is an ASP.NET Core 9 MVC web application (`src/MX.TalkWithTiles.Web`) for playing tile-based word games. The solution is split into multiple projects:

- `MX.TalkWithTiles.Web` - ASP.NET Core MVC app with controllers, views, and DI setup
- `MX.TalkWithTiles.CoreEngine` - Game engine orchestration using `GameEngine` and `IManagerFactory`
- `MX.TalkWithTiles.Scrabble` - Scrabble-specific game rules and logic
- `MX.TalkWithTiles.Repository` - Azure Table Storage data access with `ITableEntity` cloud entities
- `MX.TalkWithTiles.Contracts` - DTOs, interfaces, state models (`GameStateModel`, `BoardStateModel`, etc.), constants
- `MX.TalkWithTiles.Common` - Shared utility extensions

## Key Patterns

- **GameEngine + ManagerFactory**: `GameEngine` receives `IManagerFactory` via primary constructor and lazily creates managers (`BoardManager`, `BagManager`, `PlayerManager`, `EndGameManager`, `ChallengeManager`, `PlayerMoveManager`). Each manager owns a state model. `GameStateModel` aggregates all manager states.
- **Factory hierarchy**: `IGameEngineFactory`, `IManagerFactory`, `ITileFactory`, `IPlayerFactory` — all registered as scoped services.
- **Repository pattern**: `AppDataRepository` base class holds `TableClient` instances for five tables (Scrabble, ScrabbleIndex, ScrabbleTiles, GameInvites, Contacts). Repositories like `GameStateRepository` inherit from it and implement interfaces (e.g. `IGameStateRepository`). Cloud entities implement `ITableEntity` with `PartitionKey`/`RowKey` mapping.
- **State models**: Simple POCOs in `MX.TalkWithTiles.Contracts/StateModels/` — `GameStateModel`, `BoardStateModel`, `BagStateModel`, `PlayersStateModel`, `PlayerStateModel`, `PlayerMoveStateModel`, `ChallengeStateModel`, `EndGameStateModel`.

## Authentication

Microsoft Entra ID via `Microsoft.Identity.Web` with `TenantId: common` (multi-tenant + personal Microsoft accounts). Game controllers require `[Authorize]`; Home, About, Health, Error are anonymous. Configuration in `appsettings.json` under `AzureAd`.

## Data Storage

Azure Table Storage via `Azure.Data.Tables` SDK with `DefaultAzureCredential`. Configuration under `AppData` section in `appsettings.json` (table endpoint and table names). Repositories registered as singletons in DI.

## Build and Test

- Build: `dotnet build src/MX.TalkWithTiles.sln`
- Run: `dotnet run --project src/MX.TalkWithTiles.Web/MX.TalkWithTiles.Web.csproj`
- Test: `dotnet test src/MX.TalkWithTiles.sln`
- E2E: `npx playwright test --project=chromium` (Azurite and .NET app auto-start via webServer config)
- Test projects: `MX.TalkWithTiles.CoreEngine.Tests`, `MX.TalkWithTiles.Scrabble.Tests`, `MX.TalkWithTiles.Web.Tests`
- Test stack: xUnit 2.9.3, Moq 4.20.72, Microsoft.NET.Test.Sdk 17.12.0
- Tests use `[Theory]`/`[InlineData]` with extensive mocking of manager interfaces
- E2E tests live at the repo root: `e2e/` (specs), `pages/` (POMs), `fixtures/`, `helpers/`

## Infrastructure

Terraform under `terraform/` with per-environment configs:
- Backends: `backends/dev.backend.hcl`, `backends/prd.backend.hcl`
- Variables: `tfvars/dev.tfvars`, `tfvars/prd.tfvars`
- Key resources: App Service (Linux .NET 9.0 on shared `platform-hosting` plan), Storage Account with five tables, Entra ID app registration, Application Insights, DNS records
- Providers: AzureRM 4.59.0, AzureAD 2.50.0

## C# Conventions

- All projects target .NET 9, C# latest version (`<LangVersion>latest</LangVersion>`)
- File-scoped namespaces throughout (`namespace MX.TalkWithTiles.X;`)
- Primary constructors used extensively for DI injection
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Implicit usings enabled in the web project

## Key Files

- `src/MX.TalkWithTiles.Web/Program.cs` - DI registration, auth config, middleware pipeline
- `src/MX.TalkWithTiles.CoreEngine/GameEngine.cs` - Core game orchestration
- `src/MX.TalkWithTiles.CoreEngine/ManagerFactory.cs` - Manager creation with game type switching
- `src/MX.TalkWithTiles.Repository/GameStateRepository.cs` - Primary data access
- `src/MX.TalkWithTiles.Contracts/StateModels/` - All game state models
- `terraform/web_app.tf` - App Service definition with app settings
- `terraform/locals.tf` - Computed names and table definitions

## CI/CD

GitHub Actions workflows in `.github/workflows/`:
- `build-and-test.yml` - Runs on feature/bugfix/hotfix branch pushes
- `pr-verify.yml` - Build + Terraform plan on pull requests
- `deploy-prd.yml` - Full pipeline on main push (dev → prd)
- `deploy-dev.yml` - Manual dev deployment
- `codequality.yml` - Weekly code quality analysis
- Uses reusable actions from `frasermolyneux/actions/` with OIDC authentication
