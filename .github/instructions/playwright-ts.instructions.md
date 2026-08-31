---
description: "Repository-specific Playwright Test conventions for browser tests, fixtures, helpers, and configuration."
applyTo: "e2e/**/*.ts,fixtures/**/*.ts,helpers/**/*.ts,pages/**/*.ts,playwright.config.ts,global-setup.ts,test-ids.ts"
---

# Playwright TypeScript

- Keep `workers: 1`; browser tests share game state in Azurite.
- Use relative navigation and URL assertions. `baseURL` is defined in
  `playwright.config.ts`.
- Authenticate with `signIn` from `helpers/auth.helper.ts`. Use
  `twoPlayers`, `threePlayers`, or `fourPlayers` from
  `fixtures/test-fixtures.ts` for multi-player scenarios.
- Prefer `getByTestId`, then accessible role locators. Use IDs only where the
  application requires them for board cells, rack cells, model binding, or
  Bootstrap targets. Do not couple tests to presentation classes.
- Register shared test IDs in `test-ids.ts`.
- Page objects extend `BasePage`, keep locators readonly, and expose semantic
  actions or queries. Assertions belong in tests.
- Wait on observable state and auto-retrying assertions; do not add fixed
  delays. Use `waitForStableBoard()` after game navigation.
- Use the existing drag-and-drop shim for tile placement rather than replacing
  it with Playwright's generic `dragTo`.

Playwright starts Azurite and the .NET application through `webServer`
configuration. Install dependencies with `npm ci`; install Chromium only when a
browser run requires it.

Run the smallest relevant spec or project while iterating, for example:

```pwsh
npx playwright test e2e\profile-access.spec.ts --project=chromium
```
