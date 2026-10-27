# Architecture Overview

See [container-runtime.md](container-runtime.md) for the authoritative persistence,
proxy, container and recovery contracts. Legacy Azure resources remain intact
until separately coordinated retirement; no cloud deployment is enabled here.

## Application

Talk With Tiles is an ASP.NET Core 10 MVC web application that provides an online platform for playing tile-based word games (Scrabble-style). The solution is structured as a multi-project .NET solution:

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

Dedicated PostgreSQL 18 stores JSONB game snapshots, including board/bag/player
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

The proposed Production runtime runs `talkwithtiles-web` and
`talkwithtiles-postgres` as distinct non-root services. Only the web service
joins the host's public proxy network; PostgreSQL is private with no published
host port. Forwarded headers are accepted only from configured Caddy IPs.
Cloudflare fronts the canonical `https://talkwithtiles.com` address.

`workload.json` intentionally contains non-deployable zero digest placeholders.
Immutable publication and exact-head quality/Copilot-reviewed digest promotion
must complete before the parent-owned host policy is enabled.

## Retained legacy infrastructure

Infrastructure is defined in Terraform under the `terraform/` directory and includes:

- **Azure App Service** (Linux, .NET 10.0) using a workload-owned B1 plan in development and the shared `platform-hosting` plan in production
- **Azure Storage Account** with the five game tables
- **Microsoft Entra ID** app registration with client credentials
- **Application Insights** instance
- **DNS records** (CNAME and TXT) for custom domain

Terraform state is stored remotely in Azure Storage with per-environment backend configurations (`backends/dev.backend.hcl`, `backends/prd.backend.hcl`).
Development Terraform owns its App Service plan and does not read
`platform-hosting` state. Production continues to select the existing shared
production plan from `platform-hosting` remote state.

## Deployment and retirement

Azure application deployment workflows are explicitly blocked, so the migrated
build cannot overwrite the incompatible legacy runtime. Hosted Development is
not part of the target design. Existing Terraform resources, identities and
destroy workflows remain until a separate coordinated retirement. See
[Development Workflows](development-workflows.md) for CI/CD details.
