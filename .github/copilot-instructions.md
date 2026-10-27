# Talk With Tiles

Talk With Tiles is an online Scrabble-style word-game platform. It is an
ASP.NET Core 10 MVC application with xUnit/Playwright tests and transactional
PostgreSQL 18 persistence. Constrained runtime/identity/recovery contracts are in
`docs/container-runtime.md`. Legacy Terraform resources and destroy workflows
remain intact until separately coordinated retirement; Azure deploys are blocked.

## Layout

- `src/MX.TalkWithTiles.Web` - MVC application, controllers, views, and DI
- `src/MX.TalkWithTiles.CoreEngine` - game orchestration
- `src/MX.TalkWithTiles.Scrabble` - Scrabble rules and scoring
- `src/MX.TalkWithTiles.Repository` - Npgsql repositories and versioned schema initialization
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
- PostgreSQL stores a whole game snapshot, tiles, indexes, invites and contacts
  transactionally. Preserve optimistic version metadata through the GameEngine.
- Personal Microsoft and organisational Entra auth use the existing common-tenant
  registration and a certificate loaded ephemerally from a base64 PFX text file.
- Persist the Data Protection keyring; in-memory sessions reset on restart.
- Forwarded headers require explicit trusted proxy IPs; never trust all proxies.
- Use OIDC or managed identity for cloud access. Do not add client secrets,
  credentials, connection strings, or hard-coded tenant/subscription IDs.

## Validation

Choose validation proportional to the changed files. The default commands are:

```pwsh
dotnet build src\MX.TalkWithTiles.slnx
dotnet test src\MX.TalkWithTiles.slnx
dotnet format src\MX.TalkWithTiles.slnx --verify-no-changes
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

## Terraform toolchain

Commit each root's `.terraform-version` and `.terraform.lock.hcl`. Select the exact root CLI pin before invoking Terraform; normal initialization uses `-lockfile=readonly`. Regenerate provider selections only in a reviewed dependency-update PR; never commit state, plans, or `.terraform/`.
