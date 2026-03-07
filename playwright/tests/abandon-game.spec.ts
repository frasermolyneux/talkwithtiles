import { test, expect } from '@playwright/test';
import { signIn } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';
import { CreateGamePage } from '../pages/create-game.page';
import { AbandonGamePage } from '../pages/abandon-game.page';
import { ScrabbleIndexPage } from '../pages/scrabble-index.page';

test.describe('Game Abandonment & Deletion', () => {
  test('player can abandon an active game', async ({ browser }) => {
    const ctx = await browser.newContext({ ignoreHTTPSErrors: true });
    const page = await ctx.newPage();
    await signIn(page, players.player1);

    // Create a game
    const createPage = new CreateGamePage(page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [players.player2]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    // Navigate to abandon page
    const abandonPage = new AbandonGamePage(page);
    await abandonPage.goto(gameId);

    await expect(abandonPage.confirmButton).toBeVisible();
    await abandonPage.confirmAbandon();

    // Should redirect back to game list
    await expect(page).toHaveURL(/\/Scrabble/);

    await ctx.close();
  });

  test('abandon confirmation page has cancel option', async ({ browser }) => {
    const ctx = await browser.newContext({ ignoreHTTPSErrors: true });
    const page = await ctx.newPage();
    await signIn(page, players.player1);

    const createPage = new CreateGamePage(page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [players.player2]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    const abandonPage = new AbandonGamePage(page);
    await abandonPage.goto(gameId);

    await expect(abandonPage.cancelButton).toBeVisible();

    await ctx.close();
  });
});
