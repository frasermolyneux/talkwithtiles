# Talk With Tiles

[![Build and Test](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/build-and-test.yml)
[![Code Quality](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/codequality.yml)
[![PR Verify](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/pr-verify.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/pr-verify.yml)
[![Deploy Dev](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/deploy-dev.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/deploy-dev.yml)
[![Deploy Prd](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/deploy-prd.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/deploy-prd.yml)

## Documentation

* [Architecture Overview](docs/architecture-overview.md)
* [Development Workflows](docs/development-workflows.md)

## Overview

Talk With Tiles (MX.TalkWithTiles) is an online tile-based word game platform built with ASP.NET Core 9 MVC and deployed to Azure. Players can create and join Scrabble-style games, manage contacts, and send game invitations. The application uses Microsoft Entra ID for authentication supporting multi-tenant and personal Microsoft accounts, Azure Table Storage for persisting game state, and Application Insights for telemetry. Infrastructure is managed with Terraform and deployed via GitHub Actions to Azure App Service on a shared hosting plan.

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.

## Local dev: MCP wire-up

This repo is wired to load the shared `frasermolyneux-copilot` MCP server (org conventions catalog) at `.github/copilot/mcp_config.json`. The Copilot setup workflow checks out `frasermolyneux/.github-copilot` at tag `v0.1.0` and builds the MCP server (`npm ci && npm run build` under `.github-copilot/mcp-server`). See `.github-copilot/mcp-server/README.md` (in the checked-out content root) for the full tool surface, content-root resolution, and per-client wire-up snippets (VS Code, Claude Desktop, Copilot CLI, GitHub Copilot coding agent).
