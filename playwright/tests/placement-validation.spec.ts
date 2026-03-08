import { test, expect } from '../fixtures/test-fixtures';
import { CreateGamePage } from '../pages/create-game.page';
import { boardCenter } from '../helpers/test-data';
import type { PlayerSession } from '../fixtures/test-fixtures';

/**
 * Placement Validation Tests (TDD — RED phase)
 *
 * These tests define the Scrabble placement rules that SHOULD be enforced
 * by the game engine but currently are NOT. The negative tests (those
 * expecting rejection) will fail until server-side validation is added
 * to ScrabbleBoardManager.MakeMove().
 *
 * Rules under test:
 *   1. First word must cross the center star
 *   2. Tiles must be placed in a single straight line (horizontal or vertical)
 *   3. Subsequent words must connect to existing tiles on the board
 *   4. Cannot place a tile on an already-occupied cell
 *
 * Positive tests (those expecting acceptance) verify that valid placements
 * continue to work and serve as regression guards during implementation.
 */

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

interface MoveResult {
  isValid: boolean;
  invalidMessage?: string;
  points: number;
  wordsAndPoints: { word: string; score: number }[];
}

/**
 * Submit the current move and capture the server's JSON response.
 *
 * Unlike PlayGamePage.submitMove(), this does NOT wait for the client-side
 * page reload. We intercept the network response to assert on the server's
 * validation result directly.
 */
async function submitAndCapture(session: PlayerSession): Promise<MoveResult> {
  const responsePromise = session.page.waitForResponse(
    (resp) =>
      resp.url().includes('/Scrabble/SubmitPlayerMove/') &&
      resp.request().method() === 'POST',
  );
  await session.playPage.submitTurnButton.click();
  const response = await responsePromise;
  return (await response.json()) as MoveResult;
}

/**
 * Create a 2-player StandardBoard game and navigate both players to it.
 * Returns the game ID and identifies which session is the current player.
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
 * Play a minimal valid first move — two tiles placed horizontally
 * crossing the center star — then wait for the other player's page
 * to detect the state change via etag polling and reload.
 */
async function playValidFirstMove(
  current: PlayerSession,
  waiting: PlayerSession,
) {
  const rack = await current.playPage.getRackLetters();
  expect(rack.length).toBe(7);

  const center = boardCenter.StandardBoard;
  await current.playPage.clickToPlace(rack[0], center.x, center.y);
  await current.playPage.clickToPlace(rack[1], center.x + 1, center.y);
  await current.playPage.submitMove();

  await waiting.playPage.waitForOpponentMove();
  await waiting.playPage.waitForReady();
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

test.describe('Placement Validation', () => {
  // -----------------------------------------------------------------------
  // Rule 1: First word must cross the center star (7,7 on StandardBoard)
  // -----------------------------------------------------------------------

  test.describe('First Move — Center Star', () => {
    test('rejects first move that does not cross center star', async ({
      twoPlayers,
    }) => {
      const [p1, p2] = twoPlayers;
      const { current } = await setupGame(p1, p2);

      const rack = await current.playPage.getRackLetters();
      expect(rack.length).toBe(7);

      // Place a single tile at the top-left corner — far from center
      await current.playPage.clickToPlace(rack[0], 0, 0);

      const result = await submitAndCapture(current);

      expect(result.isValid).toBe(false);
      expect(result.invalidMessage).toBeTruthy();
      expect(result.invalidMessage!.toLowerCase()).toContain('center');
    });

    test('accepts first move that crosses center star', async ({
      twoPlayers,
    }) => {
      const [p1, p2] = twoPlayers;
      const { current } = await setupGame(p1, p2);

      const rack = await current.playPage.getRackLetters();
      expect(rack.length).toBe(7);

      const center = boardCenter.StandardBoard;
      await current.playPage.clickToPlace(rack[0], center.x, center.y);

      const result = await submitAndCapture(current);

      expect(result.isValid).toBe(true);
      expect(result.points).toBeGreaterThanOrEqual(0);
    });
  });

  // -----------------------------------------------------------------------
  // Rule 2: Tiles must form a single straight line (horizontal or vertical)
  // -----------------------------------------------------------------------

  test.describe('Linear Placement', () => {
    test('rejects tiles placed diagonally', async ({ twoPlayers }) => {
      const [p1, p2] = twoPlayers;
      const { current } = await setupGame(p1, p2);

      const rack = await current.playPage.getRackLetters();
      expect(rack.length).toBe(7);

      const center = boardCenter.StandardBoard;
      // Place one tile on center and another diagonally
      await current.playPage.clickToPlace(rack[0], center.x, center.y);
      await current.playPage.clickToPlace(
        rack[1],
        center.x + 1,
        center.y + 1,
      );

      const result = await submitAndCapture(current);

      expect(result.isValid).toBe(false);
      expect(result.invalidMessage).toBeTruthy();
      expect(result.invalidMessage!.toLowerCase()).toMatch(
        /line|row|column|straight/,
      );
    });

    test('accepts tiles in a horizontal line', async ({ twoPlayers }) => {
      const [p1, p2] = twoPlayers;
      const { current } = await setupGame(p1, p2);

      const rack = await current.playPage.getRackLetters();
      expect(rack.length).toBe(7);

      const center = boardCenter.StandardBoard;
      await current.playPage.clickToPlace(rack[0], center.x, center.y);
      await current.playPage.clickToPlace(rack[1], center.x + 1, center.y);

      const result = await submitAndCapture(current);

      expect(result.isValid).toBe(true);
      expect(result.points).toBeGreaterThan(0);
    });

    test('accepts tiles in a vertical line', async ({ twoPlayers }) => {
      const [p1, p2] = twoPlayers;
      const { current } = await setupGame(p1, p2);

      const rack = await current.playPage.getRackLetters();
      expect(rack.length).toBe(7);

      const center = boardCenter.StandardBoard;
      await current.playPage.clickToPlace(rack[0], center.x, center.y);
      await current.playPage.clickToPlace(rack[1], center.x, center.y + 1);

      const result = await submitAndCapture(current);

      expect(result.isValid).toBe(true);
      expect(result.points).toBeGreaterThan(0);
    });
  });

  // -----------------------------------------------------------------------
  // Rule 3: Subsequent words must connect to existing tiles on the board
  // -----------------------------------------------------------------------

  test.describe('Connectivity', () => {
    test('rejects second move that is disconnected from existing tiles', async ({
      twoPlayers,
    }) => {
      const [p1, p2] = twoPlayers;
      const { current, waiting } = await setupGame(p1, p2);

      // Play a valid first move at center
      await playValidFirstMove(current, waiting);

      const rack = await waiting.playPage.getRackLetters();
      expect(rack.length).toBe(7);

      // Place a tile at the far corner — completely disconnected
      await waiting.playPage.clickToPlace(rack[0], 0, 0);

      const result = await submitAndCapture(waiting);

      expect(result.isValid).toBe(false);
      expect(result.invalidMessage).toBeTruthy();
      expect(result.invalidMessage!.toLowerCase()).toMatch(
        /connect|adjacent|attach/,
      );
    });

    test('accepts second move that is adjacent to existing tiles', async ({
      twoPlayers,
    }) => {
      const [p1, p2] = twoPlayers;
      const { current, waiting } = await setupGame(p1, p2);

      // First move places tiles at (7,7) and (8,7)
      await playValidFirstMove(current, waiting);

      const rack = await waiting.playPage.getRackLetters();
      expect(rack.length).toBe(7);

      const center = boardCenter.StandardBoard;
      // Place tile one row below center — directly adjacent to existing tile
      await waiting.playPage.clickToPlace(rack[0], center.x, center.y + 1);

      const result = await submitAndCapture(waiting);

      expect(result.isValid).toBe(true);
      expect(result.points).toBeGreaterThan(0);
    });
  });

  // -----------------------------------------------------------------------
  // Rule 4: Cannot place a tile on an already-occupied cell
  // -----------------------------------------------------------------------

  test.describe('Overlap Prevention', () => {
    test('rejects placing a tile on an occupied cell', async ({
      twoPlayers,
    }) => {
      const [p1, p2] = twoPlayers;
      const { current, waiting } = await setupGame(p1, p2);

      // First move places tiles at (7,7) and (8,7)
      await playValidFirstMove(current, waiting);

      const center = boardCenter.StandardBoard;

      // The client UI prevents clicking on occupied cells — they lose the
      // .availableBoardCell class after being filled. We bypass the UI by
      // directly modifying the JS tile state in the page context. This
      // simulates a malicious client and validates server-side protection.
      await waiting.page.evaluate(
        `{
          const tile = userTiles.find(t => t.rackPosition >= 0);
          if (tile) {
            tile.posX = ${center.x};
            tile.posY = ${center.y};
            tile.rackPosition = -1;
          }
        }`,
      );

      const result = await submitAndCapture(waiting);

      expect(result.isValid).toBe(false);
      expect(result.invalidMessage).toBeTruthy();
      expect(result.invalidMessage!.toLowerCase()).toMatch(
        /occupied|already/,
      );
    });
  });
});
