# Talk With Tiles

An online Scrabble-like word game platform built with ASP.NET Core 9 and deployed to Azure.

## Architecture

- **Web Application**: ASP.NET Core 9 MVC with Entra External ID authentication
- **Data Storage**: Azure Table Storage (game state, tiles, invites, contacts)
- **Infrastructure**: Terraform on shared Azure App Service Plan
- **CI/CD**: GitHub Actions with automated dev/prd deployment pipeline

## Project Structure

```
src/
├── MX.TalkWithTiles.Common/          # Shared utilities
├── MX.TalkWithTiles.Contracts/       # DTOs, interfaces, models
├── MX.TalkWithTiles.CoreEngine/      # Generic game engine
├── MX.TalkWithTiles.CoreEngine.Tests/ # Game engine unit tests
├── MX.TalkWithTiles.Repository/      # Azure Table Storage data access
├── MX.TalkWithTiles.Scrabble/        # Scrabble game logic
├── MX.TalkWithTiles.Scrabble.Tests/  # Scrabble unit tests
├── MX.TalkWithTiles.Web/             # ASP.NET Core MVC web app
└── MX.TalkWithTiles.sln
terraform/                            # Azure infrastructure (Terraform)
```

## Local Development

1. Install [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Configure user secrets for Entra ID and Storage settings
3. Build: `dotnet build src/MX.TalkWithTiles.sln`
4. Run: `dotnet run --project src/MX.TalkWithTiles.Web/MX.TalkWithTiles.Web.csproj`
5. Test: `dotnet test src/MX.TalkWithTiles.sln`

## Contributing

Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## Security

Please see [SECURITY.md](SECURITY.md) for reporting vulnerabilities.
