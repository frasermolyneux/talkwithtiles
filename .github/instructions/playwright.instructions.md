---
applyTo: "playwright/**,**/*.cshtml"
---

# Playwright E2E Testing — General Guidance

## Overview

This project uses Playwright for end-to-end testing of an ASP.NET Core 9 MVC Scrabble web app. Tests run against a real app instance backed by Azurite (Azure Table Storage emulator) with a dev-only cookie auth scheme replacing Entra ID.

## Architecture Decisions

| Decision | Rationale |
|----------|-----------|
| Single worker, serial execution | Game state in Azurite is shared; parallel tests cause race conditions |
| Cookie auth via `TestAuthController` | Avoids external Entra ID dependency; supports multi-player with separate browser contexts |
| Azurite table emulator | Provides offline-capable Azure Table Storage; global-setup clears between runs |
| `data-testid` over CSS selectors | Resilient to CSS framework upgrades (Bootstrap); decouples tests from styling |
| `#id` retained for JS-bound elements | Board cells, rack cells, modals use `id` for JavaScript event handlers |
| Chromium + mobile-chrome projects | Desktop and mobile viewport coverage via Pixel 7 device emulation |

## Running Tests

```bash
# Prerequisites: Azurite must be running
npx azurite-table --tableHost 127.0.0.1 --tablePort 10002 --silent

# Run all tests (webServer auto-starts the .NET app)
cd playwright && npx playwright test

# Run specific test file
npx playwright test tests/game-play-2p.spec.ts

# Run specific project
npx playwright test --project=chromium
```

VS Code: Use the Playwright extension. Azurite must be running; the webServer starts automatically via `playwright.config.ts`.

## CI/CD Integration

E2E tests run in three workflows:
- `build-and-test.yml` — on feature/bugfix/hotfix branch pushes (conditioned on src or playwright changes)
- `deploy-prd.yml` — gates `app-service-deploy-dev` (must pass before deployment)
- `pr-verify.yml` — on pull requests

Workflow jobs: checkout with `fetch-depth: 0` (NBGV needs full history), `dotnet build` before test run, Azurite started as background process, `--no-build` passed to webServer in CI.

The `detect-changes` action includes a `playwright:playwright/**` filter. E2E tests execute when either `src` or `playwright` files change.

## Adding New Tests

1. Identify which POM covers the page. Create a new one extending `BasePage` if needed.
2. Add `data-testid` attributes to any new view elements (see `playwright-cshtml.instructions.md`).
3. Register new test IDs in `playwright/test-ids.ts`.
4. Update the relevant POM to expose the new elements via `getByTestId()`.
5. Write the test in `playwright/tests/`. Use fixtures for multi-player scenarios.
6. Run locally to verify, then push — CI runs automatically.

## Adding New Pages/Views

When creating a new Razor view:
1. Add `data-testid` to all interactive elements following the `{area}-{element}[-{qualifier}]` convention.
2. Create a POM in `playwright/pages/` extending `BasePage`.
3. Add corresponding test IDs to `playwright/test-ids.ts`.

## Test Reliability Patterns

- Use `waitForStableBoard()` after navigation to game pages — etag polling may trigger reloads.
- Use auto-retrying assertions (`toBeVisible()`, `toHaveText()`) for elements populated by async JS.
- For mobile tests, open the hamburger menu (`nav-toggler`) before clicking nav items.
- Use `waitUntil: 'domcontentloaded'` for pages with heavy external resource loading.
- Never use fixed delays (`waitForTimeout`) — use event-driven waits.
