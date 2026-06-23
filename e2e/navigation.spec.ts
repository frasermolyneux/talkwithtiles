import { test, expect } from '@playwright/test';
import { HomePage } from '../pages/home.page';
import { signIn } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';

test.describe('Navigation & Public Pages', () => {
  test('home page loads for unauthenticated users', async ({ page }) => {
    const homePage = new HomePage(page);
    await homePage.goto();

    await expect(page).toHaveTitle(/Talk With Tiles/i);
    // Welcome banner only shows for authenticated users; check marketing heading instead
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
  });

  test('authenticated user sees welcome banner', async ({ page }) => {
    await signIn(page, players.player1);
    const homePage = new HomePage(page);
    await homePage.goto();

    await homePage.expectWelcomeBanner(players.player1.userName);
    await expect(homePage.createGameButton).toBeVisible();
  });

  test('unauthenticated user is redirected when accessing /Scrabble', async ({ page }) => {
    await page.goto('/Scrabble');

    // Should redirect to sign-in
    await expect(page).toHaveURL(/sign-in|login/i);
  });

  test('FAQ page is publicly accessible', async ({ page }) => {
    await page.goto('/faq');
    await expect(page.getByRole('heading', { level: 1 })).toContainText(/Frequently Asked Questions|FAQ/i);
  });

  test('Getting Started page is publicly accessible', async ({ page }) => {
    await page.goto('/getting-started');
    await expect(page).toHaveURL(/getting-started/);
  });

  test('Game Controls page is publicly accessible', async ({ page }) => {
    await page.goto('/game-controls');
    await expect(page).toHaveURL(/game-controls/);
  });

  test('404 page renders for invalid routes', async ({ page }) => {
    const response = await page.goto('/nonexistent-page-xyz');
    expect(response?.status()).toBe(404);
  });

  test('navbar links are functional', async ({ page, isMobile }) => {
    await signIn(page, players.player1);
    await page.goto('/');

    // On mobile, open the hamburger menu first
    if (isMobile) {
      const toggler = page.getByTestId('nav-toggler');
      await toggler.click();
      await page.locator('.navbar-collapse.show, .navbar-collapse.collapsing').waitFor({ state: 'visible' });
    }

    // Test Scrabble dropdown
    const scrabbleDropdown = page.getByTestId('nav-scrabble');
    await scrabbleDropdown.click();
    const myGames = page.getByTestId('nav-my-games');
    await expect(myGames).toBeVisible();
  });

  test('health check endpoint responds', async ({ page }) => {
    const response = await page.request.get('/api/health/live');
    expect(response.ok()).toBeTruthy();
  });
});
