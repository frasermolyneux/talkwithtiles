---
applyTo: "e2e/**,fixtures/**,helpers/**,pages/**,specs/**,playwright.config.ts,global-setup.ts,test-ids.ts,**/*.cshtml"
---

# Playwright E2E Testing Rules

## MANDATORY: URL Navigation Rules

**ALWAYS use relative URLs. NEVER use absolute URLs.**

```typescript
// REQUIRED pattern:
await page.goto('/');                    // ✅ Correct
await page.goto('/profile');             // ✅ Correct
await expect(page).toHaveURL('/profile'); // ✅ Correct

// FORBIDDEN pattern:
await page.goto('https://localhost:5001/');       // ❌ Never do this
await expect(page).toHaveURL('https://localhost:5001/profile'); // ❌ Never do this
```

## Test Architecture Requirements

- **Single worker execution**: Set `workers: 1` - game state is shared in Azurite
- **Cookie authentication**: Use `signIn(page, players.player1)` from `helpers/auth.helper.ts`
- **Multi-player contexts**: Use fixtures from `fixtures/test-fixtures.ts` (twoPlayers, threePlayers, fourPlayers)
- **Azurite dependency**: Tests require Azurite table emulator (auto-started via webServer config)

## Locator Selection Priority

1. `page.getByTestId('element-name')` - Primary choice for all testable elements
2. `page.getByRole('button', { name: 'Text' })` - Accessible controls
3. `page.locator('#specificId')` - ONLY for: JS-bound IDs (`#cell_X-Y`, `#rack_N`), ASP.NET form IDs, Bootstrap targets
4. **NEVER use CSS class selectors** (`.btn-primary`, `.navbar-nav`, etc.)

## Page Object Model Rules

- Extend `BasePage` for all page classes
- Define all locators as `readonly` properties in constructor
- Expose semantic methods (`submitMove()`, `selectTile(letter)`) not raw locators
- POMs perform actions and queries, tests perform assertions
- Use relative URLs in `goto()` methods:

```typescript
async goto(): Promise<void> {
  await this.page.goto('/profile');
}
```

## Test Writing Requirements

```typescript
// Required imports:
import { test, expect } from '@playwright/test';
import { signIn } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';

// Test structure:
test.describe('Feature Name', () => {
  test('specific behavior', async ({ page }) => {
    await signIn(page, players.player1);
    await page.goto('/relative-path');
    // Test implementation
  });
});
```

## Async Interaction Patterns

- **Board stability**: Use `waitForStableBoard()` after navigation to game pages
- **Auto-retrying assertions**: Use `toBeVisible()`, `toHaveText()` for async-populated elements
- **Mobile navigation**: Call `page.getByTestId('nav-toggler').click()` before nav items on mobile
- **Form submissions**: Wait for page events after submit: `await page.waitForEvent('load')`
- **No fixed delays**: Use event-driven waits, never `waitForTimeout()`

## File Organization

- Tests: `e2e/*.spec.ts`
- Page Objects: `pages/*.page.ts`, `pages/modals/*.modal.ts`
- Helpers: `helpers/*.helper.ts`
- Test data: `helpers/test-data.ts`
- Test IDs: `test-ids.ts` (register all new data-testid values)

## CI/CD Integration

- Tests auto-run on: feature/bugfix/hotfix pushes, pull requests, deployments
- Environment variables: `process.env.CI` controls retry count and reporter
- Build requirements: `dotnet build` before test execution
- Azurite auto-start: Background process in CI workflows
