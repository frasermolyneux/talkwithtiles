import { test, expect } from '../fixtures/test-fixtures';
import { CreateGamePage } from '../pages/create-game.page';
import { SkipTurnModal } from '../pages/modals/skip-turn.modal';
import { players } from '../helpers/test-data';

test.describe('Skip Turn', () => {
  test('current player can skip their turn', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForReady();
    await p1.playPage.waitForReady();

    const currentPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;

    // Click skip turn to open modal
    await currentPlayer.playPage.skipTurnButton.click();
    const skipModal = new SkipTurnModal(currentPlayer.page);
    await expect(skipModal.modal).toBeVisible();

    // Confirm skip
    await skipModal.confirm();

    // Page should reload — current player should have changed
    await currentPlayer.playPage.waitForReady();
    const isStillCurrentPlayer = await currentPlayer.playPage.isCurrentPlayer();
    expect(isStillCurrentPlayer).toBe(false);
  });

  test('skip turn modal can be cancelled', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForReady();
    await p1.playPage.waitForReady();

    const currentPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;

    await currentPlayer.playPage.skipTurnButton.click();
    const skipModal = new SkipTurnModal(currentPlayer.page);
    await expect(skipModal.modal).toBeVisible();

    await skipModal.cancel();
    await expect(skipModal.modal).not.toBeVisible();
  });
});
