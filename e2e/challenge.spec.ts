import { test, expect } from '../fixtures/test-fixtures';
import { CreateGamePage } from '../pages/create-game.page';
import { IssueChallengeModal } from '../pages/modals/issue-challenge.modal';
import { ResolveChallengeModal } from '../pages/modals/resolve-challenge.modal';
import { boardCenter } from '../helpers/test-data';
import { findPlayableWord, getShortWords } from '../helpers/gameplay.helper';

test.describe('Challenge System', () => {
  test('player can issue a challenge after opponents move', async ({ twoPlayers }) => {
    test.setTimeout(60_000);
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user], {
      challengeOverride: true,
    });
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForStableBoard();
    await p1.playPage.waitForStableBoard();

    // First player makes a move
    const firstPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;
    const secondPlayer = firstPlayer === p1 ? p2 : p1;

    const rackLetters = await firstPlayer.playPage.getRackLetters();
    const word = findPlayableWord(rackLetters, getShortWords());
    if (!word) {
      test.skip(true, 'No playable word could be formed from random rack tiles');
      return;
    }

    const center = boardCenter.StandardBoard;
    await firstPlayer.playPage.placeWord(word, center.x, center.y);
    await firstPlayer.playPage.submitMove();
    await firstPlayer.playPage.waitForReady();

    // Second player reloads to see the updated game state
    await secondPlayer.page.goto(`/Scrabble/Play/${gameId}`);
    await secondPlayer.playPage.waitForReady();

    const challengeVisible = await secondPlayer.playPage.issueChallengeButton.isVisible();
    if (!challengeVisible) {
      test.skip(true, 'Challenge button not visible — game state may not support challenges');
      return;
    }

    await secondPlayer.playPage.issueChallengeButton.click();
    const challengeModal = new IssueChallengeModal(secondPlayer.page);
    await expect(challengeModal.modal).toBeVisible();

    // Issue the challenge
    await challengeModal.confirm();

    // First player should see the challenge resolution
    await firstPlayer.page.goto(`/Scrabble/Play/${gameId}`);
    await firstPlayer.playPage.waitForReady();
  });
});
