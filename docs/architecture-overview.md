# Architecture Overview

See [container-runtime.md](container-runtime.md) for persistence, proxy,
container and recovery details. Configuration and workflows are the source of
truth for the deployed contract.

## Application

Talk With Tiles is an ASP.NET Core MVC web application for tile-based word games (Scrabble-style). The solution is structured as a multi-project .NET solution:

| Project | Purpose |
|---|---|
| `MX.TalkWithTiles.Web` | ASP.NET Core MVC web application (controllers, views, DI configuration) |
| `MX.TalkWithTiles.CoreEngine` | Generic game engine orchestration (board, players, moves, scoring) |
| `MX.TalkWithTiles.Scrabble` | Scrabble-specific game logic and rules |
| `MX.TalkWithTiles.Repository` | Transactional Npgsql/PostgreSQL persistence |
| `MX.TalkWithTiles.Contracts` | DTOs, interfaces, state models, and constants |
| `MX.TalkWithTiles.Common` | Shared utility extensions |
| `MX.TalkWithTiles.CoreEngine.Tests` | Unit tests for the core engine |
| `MX.TalkWithTiles.Scrabble.Tests` | Unit tests for Scrabble logic |
| `MX.TalkWithTiles.Web.Tests` | Runtime/authentication tests and real PostgreSQL persistence tests |

## Authentication

The application uses Microsoft Entra ID (via `Microsoft.Identity.Web`) for authentication, configured with `TenantId: common` to support multi-tenant organisations and personal Microsoft accounts. A base64 PFX text secret supplies the confidential-client certificate, loaded with ephemeral key storage; MSAL redeems authorization codes without a client secret. Game-related controllers require authentication while public pages (Home, About, Health, Error) are accessible anonymously. Test authentication requires both Development and explicit opt-in.

## Data Storage

Dedicated PostgreSQL stores JSONB game snapshots, including board/bag/player
tiles, alongside relational player indexes, invitations and contacts. Aggregate
saves update related rows in one transaction, preserve optimistic versions and
reject conflicting writes. The existing game engine and repository interfaces
remain the application boundary; there are no Azure Table clients.

Startup serializes fresh-schema initialization and rejects unsupported versions.
Readiness requires a reachable writable database and the supported schema.
This migration starts with a fresh database; it does not transfer Azure data.
Persist the database and Data Protection keyring on separate host mounts.
Sessions and token caches are in-memory and reset on process restart.

## Telemetry

Application Insights instrumentation remains in the application. Legacy Azure
telemetry configuration is retained in Terraform; the new runtime must not
assume those App Service settings are present.

## Container runtime

The Production runtime contract runs `talkwithtiles-web` and
`talkwithtiles-postgres` as distinct non-root services. Only the web service
joins the host's public proxy network; PostgreSQL is private with no published
host port. Forwarded headers are accepted only from configured Caddy IPs.
Cloudflare fronts the canonical `https://talkwithtiles.com` address.

[`workload.json`](../workload.json) specifies image digests and resource
constraints. Image publication and promotion do not activate the host.

## Azure infrastructure

The [Terraform root](../terraform/) defines the Entra application and service
principal, and the Production runtime vault and host access. Its backends are
configured per environment. This configuration does not establish the current
state of live or legacy resources; review the actual state and plan before any
operator action.

## Deployment and retirement

Azure application deployment workflows are explicitly blocked, so the migrated
build cannot overwrite the incompatible legacy runtime. Hosted Development is
not part of the target design. Destroy workflows remain operator-only; identity,
infrastructure and retirement changes require separate authorization. See
[Development Workflows](development-workflows.md) for CI/CD details.
