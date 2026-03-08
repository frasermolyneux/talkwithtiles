import { test, expect } from '../fixtures/test-fixtures';
import { CreateGamePage } from '../pages/create-game.page';
import { ExchangeTilesModal } from '../pages/modals/exchange-tiles.modal';
import { players } from '../helpers/test-data';

test.describe('Exchange Tiles', () => {
  test('current player can exchange tiles', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForReady();
    await p1.playPage.waitForReady();

    const currentPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;

    // Get initial rack letters
    const initialLetters = await currentPlayer.playPage.getRackLetters();
    expect(initialLetters.length).toBe(7);

    // Open exchange modal
    await currentPlayer.playPage.exchangeTilesButton.click();
    const exchangeModal = new ExchangeTilesModal(currentPlayer.page);
    await expect(exchangeModal.modal).toBeVisible();

    // Select first two tiles for exchange
    await exchangeModal.selectTiles([0, 1]);
    await exchangeModal.confirm();

    // Player should still have 7 tiles after exchange
    await currentPlayer.playPage.waitForReady();
    const newLetters = await currentPlayer.playPage.getRackLetters();
    expect(newLetters.length).toBe(7);

    // Turn should have passed to the other player
    const isStillCurrent = await currentPlayer.playPage.isCurrentPlayer();
    expect(isStillCurrent).toBe(false);
  });
});
