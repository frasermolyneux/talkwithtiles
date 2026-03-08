import { test, expect } from '../fixtures/test-fixtures';
import { CreateGamePage } from '../pages/create-game.page';
import { boardCenter } from '../helpers/test-data';
import type { PlayerSession } from '../fixtures/test-fixtures';

/**
 * Scoring Rules Tests
 *
 * Validates that the Scrabble scoring engine correctly applies:
 *   1. Center star 2× word multiplier on first move
 *   2. 50-point bonus when placing all 7 tiles
 *   3. Cross-word scoring when tiles form perpendicular words
 */

// ---------------------------------------------------------------------------
// Tile value lookup (matches ScrabbleTileScoreHelper.cs)
// ---------------------------------------------------------------------------

const tileValues: Record<string, number> = {
  A: 1, B: 3, C: 3, D: 2, E: 1, F: 4, G: 2, H: 4,
  I: 1, J: 8, K: 5, L: 1, M: 3, N: 1, O: 1, P: 3,
  Q: 10, R: 1, S: 1, T: 1, U: 1, V: 4, W: 4, X: 8,
  Y: 4, Z: 10, _: 0,
};

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
 * Submit move and capture the JSON response via route interception.
 * Avoids the reload race condition for valid moves.
 */
async function submitAndCapture(session: PlayerSession): Promise<MoveResult> {
  let resolveResult!: (result: MoveResult) => void;
  const resultPromise = new Promise<MoveResult>((resolve) => {
    resolveResult = resolve;
  });

  await session.page.route('**/Scrabble/SubmitPlayerMove/**', async (route) => {
    const response = await route.fetch();
    const json = (await response.json()) as MoveResult;
    resolveResult(json);
    await route.fulfill({ response });
  });

  await session.playPage.submitTurnButton.click();
  const result = await resultPromise;

  await session.page.unroute('**/Scrabble/SubmitPlayerMove/**');
  return result;
}

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

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

test.describe('Scoring Rules', () => {
  // -----------------------------------------------------------------------
  // Center Star — 2× Word Multiplier
  // -----------------------------------------------------------------------

  test('first move through center scores with 2× word multiplier', async ({
    twoPlayers,
  }) => {
    const [p1, p2] = twoPlayers;
    const { current } = await setupGame(p1, p2);

    const rack = await current.playPage.getRackLetters();
    expect(rack.length).toBe(7);

    const center = boardCenter.StandardBoard;
    const tile1 = rack[0];
    const tile2 = rack[1];

    // Place two tiles horizontally through center
    await current.playPage.clickToPlace(tile1, center.x, center.y);
    await current.playPage.clickToPlace(tile2, center.x + 1, center.y);

    const result = await submitAndCapture(current);

    expect(result.isValid).toBe(true);

    // Center (7,7) = 2× word multiplier
    // Position (8,7) = standard tile (no multiplier)
    const expectedScore = (tileValues[tile1] + tileValues[tile2]) * 2;
    expect(result.points).toBe(expectedScore);
    expect(result.wordsAndPoints).toHaveLength(1);
    expect(result.wordsAndPoints[0].score).toBe(expectedScore);
  });

  // -----------------------------------------------------------------------
  // 50-Point Bonus — All 7 Tiles Placed
  // -----------------------------------------------------------------------

  test('placing all 7 tiles awards 50-point bonus', async ({
    twoPlayers,
  }) => {
    const [p1, p2] = twoPlayers;
    const { current } = await setupGame(p1, p2);

    const rack = await current.playPage.getRackLetters();
    expect(rack.length).toBe(7);

    // Place all 7 tiles horizontally at positions (4,7)-(10,7)
    // This crosses center at (7,7) — a valid first move
    const startX = 4;
    const y = boardCenter.StandardBoard.y;

    for (let i = 0; i < 7; i++) {
      await current.playPage.clickToPlace(rack[i], startX + i, y);
    }

    const result = await submitAndCapture(current);

    expect(result.isValid).toBe(true);

    // Total should equal word scores + 50 bonus
    const wordScoreTotal = result.wordsAndPoints.reduce(
      (sum, w) => sum + w.score,
      0,
    );
    expect(result.points).toBe(wordScoreTotal + 50);
    expect(result.points).toBeGreaterThanOrEqual(50);
  });

  // -----------------------------------------------------------------------
  // Cross-Word Scoring — Perpendicular Words Both Counted
  // -----------------------------------------------------------------------

  test('cross-word placement scores all formed words', async ({
    twoPlayers,
  }) => {
    const [p1, p2] = twoPlayers;
    const { current, waiting } = await setupGame(p1, p2);

    // First move: place 2 tiles horizontally at center
    const rack1 = await current.playPage.getRackLetters();
    expect(rack1.length).toBe(7);

    const center = boardCenter.StandardBoard;
    await current.playPage.clickToPlace(rack1[0], center.x, center.y);
    await current.playPage.clickToPlace(rack1[1], center.x + 1, center.y);

    await current.playPage.submitMove();
    await waiting.playPage.waitForOpponentMove();
    await waiting.playPage.waitForReady();

    // Second move: place 2 tiles vertically below the first word
    // This creates a cross-word: vertical word + extends/crosses horizontal
    const rack2 = await waiting.playPage.getRackLetters();
    expect(rack2.length).toBe(7);

    // Place at (7,8) and (8,8) — below the existing tiles at (7,7) and (8,7)
    // This forms:
    //   Horizontal at y=8: 2-letter word
    //   Vertical at x=7: 2-letter word (existing + new)
    //   Vertical at x=8: 2-letter word (existing + new)
    await waiting.playPage.clickToPlace(rack2[0], center.x, center.y + 1);
    await waiting.playPage.clickToPlace(rack2[1], center.x + 1, center.y + 1);

    const result = await submitAndCapture(waiting);

    expect(result.isValid).toBe(true);

    // Should have scored at least 2 words (horizontal + at least one vertical cross-word)
    expect(result.wordsAndPoints.length).toBeGreaterThanOrEqual(2);

    // Total points = sum of all word scores
    const totalWordScores = result.wordsAndPoints.reduce(
      (sum, w) => sum + w.score,
      0,
    );
    expect(result.points).toBe(totalWordScores);
  });
});
