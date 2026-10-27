# Talk With Tiles agent brief

Talk With Tiles is an ASP.NET Core 10 MVC application with PostgreSQL 18,
certificate-backed Microsoft/Entra authentication and constrained container
packaging. Retained Azure Terraform/destroy workflows are retirement context,
not the new runtime. See `docs/container-runtime.md` before runtime changes.

## Repository layout

- `src/MX.TalkWithTiles.slnx` - application and unit-test projects
- `src/MX.TalkWithTiles.Web` - MVC application
- `src/*.Tests` - xUnit test projects
- `e2e`, `fixtures`, `helpers`, `pages` - Playwright tests and support code
- `specs` - Playwright test plans
- `terraform` - Azure infrastructure
- `.github/workflows` - CI and deployment workflows

## Common commands

```pwsh
dotnet build src\MX.TalkWithTiles.slnx
dotnet test src\MX.TalkWithTiles.slnx
dotnet format src\MX.TalkWithTiles.slnx --verify-no-changes

npm ci
npx playwright test --project=chromium

terraform -chdir=terraform fmt -check -recursive
terraform -chdir=terraform init -lockfile=readonly -backend-config=backends/dev.backend.hcl
terraform -chdir=terraform validate
```

Use the narrowest relevant build, test, or format command while iterating. The
Playwright configuration starts the app against a disposable PostgreSQL 18
database. Set `TEST_POSTGRES_PASSWORD` and `TEST_POSTGRES_PASSWORD_FILE`; retain
one worker. Runtime Quality provides real PostgreSQL and constrained image tests.

## Repository constraints

- Use the SDK selected by `global.json`.
- Preserve the `GameEngine` and manager-factory architecture described in the
  repository Copilot instructions.
- Authentication uses Microsoft.Identity.Web, tenant common and the existing
  organisational/personal Microsoft registration, with an ephemeral private PFX
  loaded from a base64 text secret. Do not introduce client secrets.
- Playwright tests share PostgreSQL state and must remain single-worker.
- Never trust arbitrary forwarded headers; Production requires explicit Caddy IPs.
- Keep Azure deploy workflows blocked; do not remove legacy resources, identity
  ownership or destroy workflows without a separately approved retirement.
- Keep infrastructure changes within the existing Terraform environment,
  naming, backend, and variable layout.

## Authoritative guidance

- [Repository Copilot instructions](.github/copilot-instructions.md)
- [Architecture overview](docs/architecture-overview.md)
- [Development workflows](docs/development-workflows.md)
- [Playwright configuration](playwright.config.ts)


Azure application deployments are explicitly blocked during migration, including
scheduled and dispatched deployments. Development is local/CI only. Keep legacy
destroy workflows and Terraform identity ownership until coordinated retirement.
