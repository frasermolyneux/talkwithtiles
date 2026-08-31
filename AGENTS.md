# Talk With Tiles agent brief

Talk With Tiles is an ASP.NET Core 9 MVC application for playing Scrabble-style
word games. The repository also contains xUnit tests, Playwright browser tests,
and Terraform for Azure App Service and Azure Table Storage.

## Repository layout

- `src/MX.TalkWithTiles.sln` - application and unit-test projects
- `src/MX.TalkWithTiles.Web` - MVC application
- `src/*.Tests` - xUnit test projects
- `e2e`, `fixtures`, `helpers`, `pages` - Playwright tests and support code
- `specs` - Playwright test plans
- `terraform` - Azure infrastructure
- `.github/workflows` - CI and deployment workflows

## Common commands

```pwsh
dotnet build src\MX.TalkWithTiles.sln
dotnet test src\MX.TalkWithTiles.sln
dotnet format src\MX.TalkWithTiles.sln --verify-no-changes

npm ci
npx playwright test --project=chromium

terraform -chdir=terraform fmt -check -recursive
terraform -chdir=terraform init -backend-config=backends/dev.backend.hcl
terraform -chdir=terraform validate
```

Use the narrowest relevant build, test, or format command while iterating. The
Playwright configuration starts Azurite and the web application for browser
tests.

## Repository constraints

- Use the SDK selected by `global.json`.
- Preserve the `GameEngine` and manager-factory architecture described in the
  repository Copilot instructions.
- Authentication and Azure access use Microsoft Entra ID, OIDC, managed
  identities, and `DefaultAzureCredential`; do not introduce client secrets.
- Playwright tests share Azurite state and must remain single-worker.
- Keep infrastructure changes within the existing Terraform environment,
  naming, backend, and variable layout.

## Authoritative guidance

- [Repository Copilot instructions](.github/copilot-instructions.md)
- [Architecture overview](docs/architecture-overview.md)
- [Development workflows](docs/development-workflows.md)
- [Playwright configuration](playwright.config.ts)
