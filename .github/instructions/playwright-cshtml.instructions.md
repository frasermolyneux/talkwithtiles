---
applyTo: "**/*.cshtml"
---

# Razor View Test Automation Support

All `.cshtml` views MUST include `data-testid` attributes on interactive and significant elements to support Playwright E2E tests.

## Rules

- Add `data-testid` on: buttons, links, form controls, containers, banners, badges, modals, tables, and any element targeted by tests.
- Do NOT remove existing `id` attributes — add `data-testid` alongside them. JavaScript and Bootstrap rely on `id`.
- Use Razor HTML helper syntax for dynamically rendered attributes: `data_testid = "value"` (underscore, not hyphen) — Razor renders it as `data-testid="value"`.

## Naming Convention

Kebab-case: `{area}-{element}[-{qualifier}]`

| Pattern | Example |
|---------|---------|
| Navigation | `nav-home`, `nav-scrabble`, `nav-toggler` |
| Buttons | `btn-submit-turn`, `btn-confirm-abandon` |
| Containers | `game-controls`, `player-summary`, `additional-players` |
| Modals | `modal-skip-turn`, `modal-skip-turn-confirm`, `modal-skip-turn-cancel` |
| Form controls | `select-game-type`, `checkbox-public-game` |
| Data displays | `turn-score`, `bag-count`, `player-badge` |
| Tables | `active-games`, `completed-games`, `contacts-table` |

## Existing Test IDs

Reference `test-ids.ts` for the complete registry of all `data-testid` values. When adding a new view element that tests will target, add the ID to both the view and `test-ids.ts`.

## What NOT to add data-testid to

- Individual board cells (`id="cell_X-Y"`) — parameterised, used by JS drag/drop.
- Individual rack cells (`id="rack_N"`) — parameterised, used by JS tile management.
- ASP.NET-generated form field IDs (`#PlayerModels_0__Identifier`) — dynamic model binding.
- Bootstrap collapse/accordion targets (`#collapseOne`) — used by Bootstrap JS.
- Pure layout wrappers with no test significance (rows, generic card containers).
