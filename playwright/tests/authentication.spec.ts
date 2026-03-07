import { test, expect } from '@playwright/test';
import { signIn, signOut } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';
import { HomePage } from '../pages/home.page';

test.describe('Authentication', () => {
  test('sign in via TestAuth creates authenticated session', async ({ page }) => {
    await signIn(page, players.player1);

    const homePage = new HomePage(page);
    await homePage.goto();

    await homePage.expectWelcomeBanner(players.player1.userName);
  });

  test('sign out clears the session', async ({ page }) => {
    await signIn(page, players.player1);

    const homePage = new HomePage(page);
    await homePage.goto();
    await homePage.expectWelcomeBanner(players.player1.userName);

    await signOut(page);
    await homePage.goto();

    // Should no longer see welcome banner
    await expect(page.locator('.bg-light.p-5')).not.toBeVisible();
  });

  test('different users have separate identities', async ({ browser }) => {
    // Player 1 context
    const ctx1 = await browser.newContext({ ignoreHTTPSErrors: true });
    const page1 = await ctx1.newPage();
    await signIn(page1, players.player1);
    await page1.goto('/');
    await expect(page1.locator('.bg-light.p-5')).toContainText(players.player1.userName);

    // Player 2 context
    const ctx2 = await browser.newContext({ ignoreHTTPSErrors: true });
    const page2 = await ctx2.newPage();
    await signIn(page2, players.player2);
    await page2.goto('/');
    await expect(page2.locator('.bg-light.p-5')).toContainText(players.player2.userName);

    await ctx1.close();
    await ctx2.close();
  });

  test('protected routes redirect unauthenticated users', async ({ page }) => {
    // Try to access game creation without auth
    await page.goto('/Scrabble/Create');
    // Should redirect to sign-in page
    await expect(page).toHaveURL(/sign-in/);
  });

  test('admin role grants admin access', async ({ page }) => {
    await signIn(page, players.admin);
    const response = await page.goto('/Analytics');
    // Admin should be able to access analytics
    expect(response?.status()).not.toBe(403);
  });
});
