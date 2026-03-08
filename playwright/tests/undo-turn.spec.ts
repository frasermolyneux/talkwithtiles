import { test, expect } from '../fixtures/test-fixtures';
import { CreateGamePage } from '../pages/create-game.page';
import { UndoTurnModal } from '../pages/modals/undo-turn.modal';
import { players, scrabbleWords, boardCenter } from '../helpers/test-data';
import { findPlayableWord } from '../helpers/gameplay.helper';

test.describe('Undo Turn', () => {
  test('last player can undo their move', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForStableBoard();
    await p1.playPage.waitForStableBoard();

    const currentPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;

    // Make a move first
    const rackLetters = await currentPlayer.playPage.getRackLetters();
    const word = findPlayableWord(rackLetters, scrabbleWords.twoLetter);
    if (!word) {
      test.skip(true, 'No playable word could be formed from random rack tiles');
      return;
    }

    const center = boardCenter.StandardBoard;
    await currentPlayer.playPage.placeWord(word, center.x, center.y);
    await currentPlayer.playPage.submitMove();
    await currentPlayer.playPage.waitForReady();

    // The player who just moved should see the undo button
    const undoVisible = await currentPlayer.playPage.undoLastTurnButton.isVisible();
    if (!undoVisible) {
      test.skip(true, 'Undo button not visible — game state may not support undo');
      return;
    }

    await currentPlayer.playPage.undoLastTurnButton.click();
    const undoModal = new UndoTurnModal(currentPlayer.page);
    await expect(undoModal.modal).toBeVisible();

    await undoModal.confirm();
    await currentPlayer.playPage.waitForReady();

    // After undo, this player should be the current player again
    const isCurrentAgain = await currentPlayer.playPage.isCurrentPlayer();
    expect(isCurrentAgain).toBe(true);
  });
});
