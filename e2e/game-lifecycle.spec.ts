import { test, expect } from '../fixtures/test-fixtures';
import { CreateGamePage } from '../pages/create-game.page';
import { SkipTurnModal } from '../pages/modals/skip-turn.modal';
import type { PlayerSession } from '../fixtures/test-fixtures';

/**
 * Game Lifecycle Tests
 *
 * Validates game end conditions per Scrabble rules:
 *   1. Consecutive passes by all players should end the game
 *
 * BUG: The game engine currently does NOT track consecutive passes
 * or trigger game end when all players pass. The SkippedTurns dictionary
 * in EndGameManager is never written to. These tests define the expected
 * behaviour and should FAIL until the bug is fixed.
 */

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/**
 * Create a 2-player StandardBoard game and navigate both players.
 */
async function setupGame(p1: PlayerSession, p2: PlayerSession) {
  const createPage = new CreateGamePage(p1.page);
  await createPage.goto();
  const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
  const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

  await p2.playPage.goto(gameId);
  await p2.playPage.waitForStableBoard();
  await p1.playPage.waitForStableBoard();

  const p1IsFirst = await p1.playPage.isCurrentPlayer();
  return {
    gameId,
    current: p1IsFirst ? p1 : p2,
    waiting: p1IsFirst ? p2 : p1,
  };
}

/**
 * Skip the current player's turn via the skip turn modal.
 * After confirming, waits for the page to reload.
 */
async function skipTurn(session: PlayerSession): Promise<void> {
  await session.playPage.skipTurnButton.click();
  const skipModal = new SkipTurnModal(session.page);
  await expect(skipModal.modal).toBeVisible();
  await skipModal.confirm();
  await session.playPage.waitForReady();
}

/**
 * Skip the current player's turn when the game may end as a result.
 * After confirming, waits for page reload but does NOT expect the rack
 * (completed games hide the rack and show a game state banner instead).
 */
async function skipTurnMayEndGame(session: PlayerSession): Promise<void> {
  await session.playPage.skipTurnButton.click();
  const skipModal = new SkipTurnModal(session.page);
  await expect(skipModal.modal).toBeVisible();
  await skipModal.confirm();
  // confirm() already waits for URL redirect; just wait for the board
  // (present in both active and completed games — but rack is hidden on completed)
  await session.playPage.board.waitFor({ state: 'visible', timeout: 15_000 });
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

test.describe('Game Lifecycle', () => {
  test('consecutive passes by all players ends the game', async ({
    twoPlayers,
  }) => {
    const [p1, p2] = twoPlayers;
    const { current, waiting } = await setupGame(p1, p2);

    // Player 1 (current) skips — game should NOT end yet
    await skipTurn(current);

    // Wait for opponent's page to detect the skip via etag polling
    await waiting.playPage.waitForOpponentMove();
    await waiting.playPage.waitForReady();

    // Player 2 (waiting, now current) skips — this should trigger game end
    // Use skipTurnMayEndGame because the rack won't be shown on a completed game
    await skipTurnMayEndGame(waiting);

    // The game state banner should now be visible on the skipping player's page
    const isCompleteForWaiting = await waiting.playPage.isGameComplete();
    expect(isCompleteForWaiting).toBe(true);
  });

  test('skip by one player does not end the game', async ({
    twoPlayers,
  }) => {
    const [p1, p2] = twoPlayers;
    const { current, waiting } = await setupGame(p1, p2);

    // Only one player skips
    await skipTurn(current);

    // Wait for waiting player's page to detect the skip
    await waiting.playPage.waitForOpponentMove();
    await waiting.playPage.waitForReady();

    // Game should still be in progress
    const isComplete = await waiting.playPage.isGameComplete();
    expect(isComplete).toBe(false);

    // The waiting player (now current) should be able to play
    const isCurrentPlayer = await waiting.playPage.isCurrentPlayer();
    expect(isCurrentPlayer).toBe(true);
  });
});
