import { test, expect } from '@playwright/test';
import { signIn } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';
import { CreateGamePage } from '../pages/create-game.page';
import { ScrabbleIndexPage } from '../pages/scrabble-index.page';

test.describe('Game Creation', () => {
  test.beforeEach(async ({ page }) => {
    await signIn(page, players.player1);
  });

  test('create a standard 2-player game', async ({ page }) => {
    const createPage = new CreateGamePage(page);
    await createPage.goto();

    await createPage.createGame('StandardBoard', [players.player2]);

    // Should redirect to the Play page
    await expect(page).toHaveURL(/\/Scrabble\/Play\//);
  });

  test('create a mini board game', async ({ page }) => {
    const createPage = new CreateGamePage(page);
    await createPage.goto();

    await createPage.createGame('MiniBoard', [players.player2]);

    await expect(page).toHaveURL(/\/Scrabble\/Play\//);
  });

  test('create a super size board game', async ({ page }) => {
    const createPage = new CreateGamePage(page);
    await createPage.goto();

    await createPage.createGame('SuperSizeBoard', [players.player2]);

    await expect(page).toHaveURL(/\/Scrabble\/Play\//);
  });

  test('create a 3-player game', async ({ page }) => {
    const createPage = new CreateGamePage(page);
    await createPage.goto();

    await createPage.createGame('StandardBoard', [players.player2, players.player3]);

    await expect(page).toHaveURL(/\/Scrabble\/Play\//);
  });

  test('create a 4-player game', async ({ page }) => {
    const createPage = new CreateGamePage(page);
    await createPage.goto();

    await createPage.createGame('StandardBoard', [
      players.player2,
      players.player3,
      players.player4,
    ]);

    await expect(page).toHaveURL(/\/Scrabble\/Play\//);
  });

  test('created game appears in games index', async ({ page }) => {
    const createPage = new CreateGamePage(page);
    await createPage.goto();
    await createPage.createGame('StandardBoard', [players.player2]);

    const indexPage = new ScrabbleIndexPage(page);
    await indexPage.goto();

    await indexPage.expectGameInList();
  });

  test('game creation with challenge override enabled', async ({ page }) => {
    const createPage = new CreateGamePage(page);
    await createPage.goto();

    await createPage.createGame('StandardBoard', [players.player2], {
      challengeOverride: true,
    });

    await expect(page).toHaveURL(/\/Scrabble\/Play\//);
  });

  test('game creation with public game option', async ({ page }) => {
    const createPage = new CreateGamePage(page);
    await createPage.goto();

    await createPage.createGame('StandardBoard', [players.player2], {
      publicGame: true,
    });

    await expect(page).toHaveURL(/\/Scrabble\/Play\//);
  });
});
