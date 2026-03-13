---
applyTo: "e2e/**/*.ts,fixtures/**/*.ts,helpers/**/*.ts,pages/**/*.ts,playwright.config.ts,global-setup.ts,test-ids.ts"
---

# Playwright TypeScript Code Rules

## MANDATORY: URL Pattern Rules

**ALWAYS use relative URLs. NEVER use absolute URLs.**

```typescript
// REQUIRED patterns:
await page.goto('/');                     // ✅ Homepage
await page.goto('/profile');              // ✅ Specific page
await expect(page).toHaveURL('/profile');  // ✅ URL assertions

// POM goto methods:
async goto(): Promise<void> {
  await this.page.goto('/profile');         // ✅ Relative URL only
}

// FORBIDDEN patterns:
await page.goto('https://localhost:5001/');           // ❌ Never
await expect(page).toHaveURL('https://localhost:5001/profile'); // ❌ Never
```

## File Structure Requirements

```
repo/
├── playwright.config.ts       # webServer config (Azurite + .NET)
├── global-setup.ts            # Table cleanup
├── test-ids.ts                # data-testid constants
├── fixtures/test-fixtures.ts  # Multi-player fixtures
├── helpers/
│   ├── auth.helper.ts         # signIn/signOut functions
│   ├── test-data.ts           # TestUser definitions
│   └── gameplay.helper.ts     # Game interaction utilities
├── pages/
│   ├── base.page.ts           # Extends for all pages
│   └── modals/                # Modal-specific POMs
└── e2e/                       # Test specifications
```

## Locator Selection Rules

```typescript
// Priority order (use first available):
page.getByTestId('element-name')              // 1. Primary choice
page.getByRole('button', { name: 'Text' })    // 2. Accessible controls
page.locator('#specificId')                   // 3. JS-bound/Bootstrap IDs only

// NEVER use:
page.locator('.btn-primary')                  // ❌ CSS classes forbidden
page.locator('.navbar-nav')                   // ❌ Bootstrap classes forbidden
```

## Page Object Model Requirements

```typescript
import { type Page, type Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class ExamplePage extends BasePage {  // Must extend BasePage
  // All locators readonly, defined in constructor
  readonly submitButton: Locator;
  readonly emailInput: Locator;

  constructor(page: Page) {
    super(page);
    this.submitButton = page.getByTestId('submit-btn');
    this.emailInput = page.getByTestId('email-input');
  }

  // Semantic methods, no raw locator exposure
  async submitForm(email: string): Promise<void> {
    await this.emailInput.fill(email);
    await this.submitButton.click();
  }

  // Relative URLs in navigation
  async goto(): Promise<void> {
    await this.page.goto('/example');
  }
}
```

## Test Structure Requirements

```typescript
// Required imports:
import { test, expect } from '@playwright/test';
// OR for multi-player:
import { test, expect } from '../fixtures/test-fixtures';

import { signIn, signOut } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';
import { ExamplePage } from '../pages/example.page';

// Single player test:
test.describe('Feature Name', () => {
  test('specific behavior', async ({ page }) => {
    await signIn(page, players.player1);
    
    const examplePage = new ExamplePage(page);
    await examplePage.goto();
    
    // Test implementation with assertions
    await expect(page).toHaveURL('/example');
  });
});

// Multi-player test:
test('multi-player behavior', async ({ twoPlayers }) => {
  const [p1, p2] = twoPlayers;
  // Each has: context, page, user, playPage
});
```

## Authentication Pattern

```typescript
// Single player:
await signIn(page, players.player1);

// Multi-player (automatic via fixtures):
const { twoPlayers, threePlayers, fourPlayers } = fixtures;
```

## Async Game Interaction Rules

```typescript
// Board stability after navigation:
await playPage.waitForStableBoard();

// Auto-retrying assertions for async content:
await expect(element).toBeVisible();
await expect(element).toHaveText(/pattern/);

// Form submissions with page reload:
await submitButton.click();
await page.waitForEvent('load');

// Mobile navigation:
if (isMobile) {
  await page.getByTestId('nav-toggler').click();
}

// NEVER use fixed delays:
// await page.waitForTimeout(1000);  // ❌ Forbidden
```

## Drag and Drop Pattern

```typescript
// Use custom shim for tile placement (not Playwright dragTo):
await this.dragToPlace(tileLocator, targetCell);

// Implementation includes DataTransfer shim for Chromium
```

## Test ID Management

```typescript
// Import from test-ids.ts:
import { TestIds } from '../test-ids';

// Use constants instead of strings:
page.getByTestId(TestIds.SUBMIT_BUTTON);

// Add new IDs to test-ids.ts when creating elements
```

## CI Environment Handling

```typescript
// Environment detection:
const isCI = process.env.CI;

// CI-specific configurations:
if (isCI) {
  // Different timeouts, retry logic
}
```
