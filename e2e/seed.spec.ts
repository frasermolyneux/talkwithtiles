import { test, expect } from '@playwright/test';

/**
 * Seed test for Playwright Test Agents.
 *
 * This test bootstraps the app environment so that agents (planner,
 * generator, healer) have an authenticated session to explore and
 * interact with the application.
 * Its purpose is to verify that the seeded session can reach authenticated
 * game creation, providing a stable starting point for agent-generated tests.
 *
 * The app runs on https://localhost:5001 with:
 * - Azurite for Azure Table Storage emulation
 * - Cookie-based test auth (Testing__Enabled=true)
 * - webServer auto-start via playwright.config.ts
 */
test('seed', async ({ page }) => {
  // Sign in via the TestAuth API
  await page.request.post('/api/test/signin', {
    data: {
      userId: '11111111-1111-1111-1111-111111111111',
      userName: 'Alice',
      email: 'alice@test.local',
      role: null,
    },
  });

  // Navigate to the home page and verify we are signed in
  await page.goto('/');
  await expect(page.getByTestId('welcome-banner')).toBeVisible();
  await expect(page.getByText('Welcome Alice')).toBeVisible();

  // Verify navigation is available for authenticated users
  await expect(page.getByTestId('nav-scrabble')).toBeVisible();
  await expect(page.getByTestId('nav-user-menu')).toBeVisible();

  // Navigate to game creation to confirm full app functionality
  await page.getByTestId('nav-scrabble').click();
  await page.getByTestId('nav-create-game').click();
  await expect(page.getByTestId('create-game-form')).toBeVisible();
});
