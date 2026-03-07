import { test, expect } from '@playwright/test';
import { signIn } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';
import { CreateGamePage } from '../pages/create-game.page';
import { PlayGamePage } from '../pages/play-game.page';

test.describe('Mobile Viewport Tests', () => {
  // These tests use the mobile-chrome project from playwright.config.ts
  // but can also be explicitly configured here

  test('game board renders on mobile viewport', async ({ browser }) => {
    const context = await browser.newContext({
      ignoreHTTPSErrors: true,
      viewport: { width: 393, height: 851 }, // Pixel 7
      isMobile: true,
      hasTouch: true,
    });

    const page = await context.newPage();
    await signIn(page, players.player1);

    const createPage = new CreateGamePage(page);
    await createPage.goto();
    await createPage.createGame('StandardBoard', [players.player2]);

    const playPage = new PlayGamePage(page);
    await playPage.waitForReady();

    // Board should be visible on mobile
    await expect(playPage.board).toBeVisible();
    await expect(playPage.rack).toBeVisible();

    // Board should fit within viewport width
    const boardBox = await playPage.board.boundingBox();
    expect(boardBox).toBeTruthy();
    if (boardBox) {
      expect(boardBox.width).toBeLessThanOrEqual(393);
    }

    await context.close();
  });

  test('game creation form works on mobile', async ({ browser }) => {
    const context = await browser.newContext({
      ignoreHTTPSErrors: true,
      viewport: { width: 393, height: 851 },
      isMobile: true,
      hasTouch: true,
    });

    const page = await context.newPage();
    await signIn(page, players.player1);

    const createPage = new CreateGamePage(page);
    await createPage.goto();

    // Form should be usable on mobile
    await expect(createPage.gameTypeSelect).toBeVisible();
    await expect(createPage.submitButton).toBeVisible();

    await context.close();
  });

  test('navigation menu works on mobile', async ({ browser }) => {
    const context = await browser.newContext({
      ignoreHTTPSErrors: true,
      viewport: { width: 393, height: 851 },
      isMobile: true,
      hasTouch: true,
    });

    const page = await context.newPage();
    await signIn(page, players.player1);
    await page.goto('/');

    // On mobile, navbar should have a hamburger toggle
    const toggler = page.locator('.navbar-toggler');
    if (await toggler.isVisible()) {
      await toggler.click();
      // Menu items should become visible
      const navItems = page.locator('.navbar-nav .nav-link');
      await expect(navItems.first()).toBeVisible();
    }

    await context.close();
  });

  test('tile rack is accessible on mobile', async ({ browser }) => {
    const context = await browser.newContext({
      ignoreHTTPSErrors: true,
      viewport: { width: 393, height: 851 },
      isMobile: true,
      hasTouch: true,
    });

    const page = await context.newPage();
    await signIn(page, players.player1);

    const createPage = new CreateGamePage(page);
    await createPage.goto();
    await createPage.createGame('MiniBoard', [players.player2]);

    const playPage = new PlayGamePage(page);
    await playPage.waitForReady();

    // Rack tiles should be visible and tappable
    const rackTiles = playPage.getRackTiles();
    const count = await rackTiles.count();
    expect(count).toBeGreaterThan(0);

    await context.close();
  });
});
