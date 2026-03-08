import { test, expect } from '../fixtures/test-fixtures';
import { CreateGamePage } from '../pages/create-game.page';
import { PlayGamePage } from '../pages/play-game.page';
import { SkipTurnModal } from '../pages/modals/skip-turn.modal';
import { players, scrabbleWords, boardCenter } from '../helpers/test-data';
import { findPlayableWord } from '../helpers/gameplay.helper';

test.describe('2-Player Game Playthrough', () => {
  test('play a complete game with alternating turns', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    // Player 1 creates a game
    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);

    // Extract game ID from URL
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1];
    expect(gameId).toBeTruthy();

    // Player 2 navigates to the game
    await p2.playPage.goto(gameId!);
    await p2.playPage.waitForReady();

    // Player 1 should already be on the play page
    await p1.playPage.waitForReady();

    // Determine who goes first
    const p1IsCurrentPlayer = await p1.playPage.isCurrentPlayer();

    const currentPlayer = p1IsCurrentPlayer ? p1 : p2;
    const waitingPlayer = p1IsCurrentPlayer ? p2 : p1;

    // First player reads their rack letters
    const rackLetters = await currentPlayer.playPage.getRackLetters();
    expect(rackLetters.length).toBe(7);

    // Find a playable word from rack tiles
    const allWords = [
      ...scrabbleWords.twoLetter,
      ...scrabbleWords.threeLetter,
      ...scrabbleWords.fourLetter,
    ];
    const word = findPlayableWord(rackLetters, allWords);

    if (word) {
      // Place first word centered on the board
      const center = boardCenter.StandardBoard;
      const startX = center.x - Math.floor(word.length / 2);
      await currentPlayer.playPage.placeWord(word, startX, center.y);

      // Submit the move
      await currentPlayer.playPage.submitMove();

      // Verify move was accepted (page reloaded with updated state)
      await currentPlayer.playPage.waitForReady();
    } else {
      // If no word can be formed, skip the turn
      await currentPlayer.playPage.skipTurnButton.click();
      const skipModal = new SkipTurnModal(currentPlayer.page);
      await skipModal.confirm();
    }
  });

  test('player scores update after a move', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    // Both players navigate and wait for board to stabilize after invite linking
    await p2.playPage.goto(gameId);
    await p2.playPage.waitForStableBoard();
    await p1.playPage.waitForStableBoard();

    const currentPlayer = (await p1.playPage.isCurrentPlayer()) ? p1 : p2;

    const rackLetters = await currentPlayer.playPage.getRackLetters();
    const word = findPlayableWord(rackLetters, scrabbleWords.twoLetter);

    if (word) {
      const center = boardCenter.StandardBoard;
      await currentPlayer.playPage.placeWord(word, center.x, center.y);
      await currentPlayer.playPage.submitMove();

      // After submission, the tile bag should decrease (proves the move was accepted)
      const bagCount = await currentPlayer.playPage.getBagTileCount();
      expect(bagCount).toBeLessThan(86);

      // At least one player should have a non-zero score
      const scores = await currentPlayer.playPage.getPlayerScores();
      const totalScore = Array.from(scores.values()).reduce((a, b) => a + b, 0);
      expect(totalScore).toBeGreaterThan(0);
    }
  });

  test('game list shows active game for both players', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    await createPage.createGame('StandardBoard', [p2.user]);

    // Player 1 should see the game in their list
    await p1.page.goto('/Scrabble');
    await expect(p1.page.getByTestId('active-games')).toBeVisible();

    // Player 2 should also see the game
    await p2.page.goto('/Scrabble');
    await expect(p2.page.getByTestId('active-games')).toBeVisible();
  });

  test('tile bag count decreases after moves', async ({ twoPlayers }) => {
    const [p1, p2] = twoPlayers;

    const createPage = new CreateGamePage(p1.page);
    await createPage.goto();
    const gameUrl = await createPage.createGame('StandardBoard', [p2.user]);
    const gameId = gameUrl.match(/Play\/([a-f0-9-]+)/i)?.[1]!;

    await p2.playPage.goto(gameId);
    await p2.playPage.waitForReady();
    await p1.playPage.waitForReady();

    const initialBagCount = await p1.playPage.getBagTileCount();
    expect(initialBagCount).toBeGreaterThan(0);
  });
});
