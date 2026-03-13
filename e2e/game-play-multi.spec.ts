import { test, expect } from '../fixtures/test-fixtures';
import { CreateGamePage } from '../pages/create-game.page';
import { boardCenter } from '../helpers/test-data';

test.describe('3-Player Game', () => {
  test('three players can all see the game board', async ({ threePlayers }) => {
    const [p1, p2, p3] = threePlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user, p3.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    // All players navigate to the game
    await p2.playPage.goto(gameId);
    await p3.playPage.goto(gameId);

    // Verify all three players can see the board
    await p1.playPage.waitForReady();
    await p2.playPage.waitForReady();
    await p3.playPage.waitForReady();

    // Each player in the game should have 7 tiles (only current player has rack visible)
    // Check that at least the board is visible for all
    for (const p of [p1, p2, p3]) {
      await expect(p.playPage.board).toBeVisible();
    }
  });
});

test.describe('4-Player Game', () => {
  test('four players can all see the game board', async ({ fourPlayers }) => {
    const [p1, p2, p3, p4] = fourPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user, p3.user, p4.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    for (const p of [p2, p3, p4]) {
      await p.playPage.goto(gameId);
      await p.playPage.waitForReady();
    }

    await p1.playPage.waitForReady();

    // All players should see the board
    for (const p of [p1, p2, p3, p4]) {
      await expect(p.playPage.board).toBeVisible();
    }
  });
});
