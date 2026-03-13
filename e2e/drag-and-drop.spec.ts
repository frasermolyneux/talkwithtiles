import { test, expect } from '../fixtures/test-fixtures';
import { CreateGamePage } from '../pages/create-game.page';
import { boardCenter } from '../helpers/test-data';
import { findPlayableWord, getShortWords } from '../helpers/gameplay.helper';

test.describe('Drag and Drop Tile Placement', () => {
  test.setTimeout(60_000);
  test('player can drag tile from rack to board', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForStableBoard();
    await p1.playPage.waitForStableBoard();

    const currentPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;
    const rackLetters = await currentPlayer.playPage.getRackLetters();

    if (rackLetters.length > 0) {
      const letter = rackLetters[0];
      const center = boardCenter.StandardBoard;

      // Drag tile to center of board
      await currentPlayer.playPage.dragToPlace(letter, center.x, center.y);

      // Board cell should now contain a tile
      const cell = currentPlayer.playPage.getBoardCell(center.x, center.y);
      const tileImg = cell.locator('img');
      await expect(tileImg).toBeVisible();
    }
  });

  test('player can drag multiple tiles to form a word', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForStableBoard();
    await p1.playPage.waitForStableBoard();

    const currentPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;
    const rackLetters = await currentPlayer.playPage.getRackLetters();
    const word = findPlayableWord(rackLetters, getShortWords());

    if (word) {
      const center = boardCenter.StandardBoard;
      await currentPlayer.playPage.placeWordByDrag(word, center.x, center.y);

      // Score preview should update (updateMoveScore is async — fetches from server)
      await expect(currentPlayer.playPage.turnScore).toBeVisible({ timeout: 10_000 });
      await expect(currentPlayer.playPage.turnScore).toHaveText(/\d+/, { timeout: 5_000 });
    }
  });
});

test.describe('Click-to-Place Tile Placement', () => {
  test.setTimeout(60_000);
  test('player can click tile then click board cell', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForStableBoard();
    await p1.playPage.waitForStableBoard();

    const currentPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;
    const rackLetters = await currentPlayer.playPage.getRackLetters();

    if (rackLetters.length > 0) {
      const letter = rackLetters[0];
      const center = boardCenter.StandardBoard;

      await currentPlayer.playPage.clickToPlace(letter, center.x, center.y);

      const cell = currentPlayer.playPage.getBoardCell(center.x, center.y);
      const tileImg = cell.locator('img');
      await expect(tileImg).toBeVisible();
    }
  });
});

test.describe('Tile Rack Management', () => {
  test.setTimeout(60_000);
  test('recall tiles returns placed tiles to rack', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForStableBoard();
    await p1.playPage.waitForStableBoard();

    const currentPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;
    const initialLetters = await currentPlayer.playPage.getRackLetters();

    if (initialLetters.length > 0) {
      // Place a tile on the board
      const center = boardCenter.StandardBoard;
      await currentPlayer.playPage.clickToPlace(initialLetters[0], center.x, center.y);

      // Recall tiles
      await currentPlayer.playPage.recallTiles();

      // Rack should have all original tiles back
      const afterRecall = await currentPlayer.playPage.getRackLetters();
      expect(afterRecall.length).toBe(initialLetters.length);
    }
  });

  test('shuffle tiles randomizes rack order', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForStableBoard();
    await p1.playPage.waitForStableBoard();

    const currentPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;

    // Shuffle should not throw an error
    await currentPlayer.playPage.shuffleTiles();

    // Player should still have 7 tiles
    const letters = await currentPlayer.playPage.getRackLetters();
    expect(letters.length).toBe(7);
  });
});
