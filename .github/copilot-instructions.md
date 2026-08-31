# Talk With Tiles

Talk With Tiles is an online Scrabble-style word-game platform. It is an
ASP.NET Core 9 MVC application with xUnit unit tests, Playwright browser tests,
Azure Table Storage persistence, and Terraform-managed Azure infrastructure.

## Layout

- `src/MX.TalkWithTiles.Web` - MVC application, controllers, views, and DI
- `src/MX.TalkWithTiles.CoreEngine` - game orchestration
- `src/MX.TalkWithTiles.Scrabble` - Scrabble rules and scoring
- `src/MX.TalkWithTiles.Repository` - Azure Table Storage repositories
- `src/MX.TalkWithTiles.Contracts` - interfaces, DTOs, and state models
- `src/MX.TalkWithTiles.Common` - shared utilities
- `src/*.Tests` - xUnit test projects
- `e2e`, `fixtures`, `helpers`, `pages`, `specs` - Playwright tests and plans
- `terraform` - Azure infrastructure for development and production

## Architecture and security

- `GameEngine` receives `IManagerFactory` and coordinates board, bag, player,
  move, challenge, and end-game managers. Keep game-type behavior behind the
  existing factory and manager interfaces.
- `GameStateModel` aggregates manager-owned state models. Preserve the
  repository and contract boundaries when changing persisted state.
- Authentication uses Microsoft Entra ID through `Microsoft.Identity.Web`.
- Azure Table Storage access uses `Azure.Data.Tables` and
  `DefaultAzureCredential`.
- Use OIDC or managed identity for cloud access. Do not add client secrets,
  credentials, connection strings, or hard-coded tenant/subscription IDs.

## Validation

Choose validation proportional to the changed files. The default commands are:

```pwsh
dotnet build src\MX.TalkWithTiles.sln
dotnet test src\MX.TalkWithTiles.sln
dotnet format src\MX.TalkWithTiles.sln --verify-no-changes
npx playwright test --project=chromium
terraform -chdir=terraform fmt -check -recursive
terraform -chdir=terraform validate
```

Use `global.json` for the .NET SDK. Run `npm ci` before Playwright commands when
dependencies are absent. Terraform validation requires initialization with the
relevant backend configuration.

Detailed guidance:

- [Architecture overview](../docs/architecture-overview.md)
- [Development workflows](../docs/development-workflows.md)
- Scoped instructions under `.github/instructions`
