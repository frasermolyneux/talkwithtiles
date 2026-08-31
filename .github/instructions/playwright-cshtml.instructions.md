---
description: "Testability rules for Razor views exercised by the Playwright suite."
applyTo: "src/MX.TalkWithTiles.Web/Views/**/*.cshtml"
---

# Razor view testability

- Preserve existing `data-testid` attributes used by Playwright tests.
- Add a stable kebab-case `data-testid` when a new interactive or significant
  element needs a browser-test locator.
- Keep existing `id` attributes used by JavaScript, Bootstrap, or ASP.NET model
  binding.
- In Razor HTML-helper attribute objects, use `data_testid`; Razor renders it
  as `data-testid`.
- Add shared identifiers to `test-ids.ts` when the TypeScript suite should
  reference them as constants.

Do not add test IDs to parameterized board cells (`cell_X-Y`), rack cells
(`rack_N`), generated form IDs, Bootstrap targets, or layout-only wrappers.
