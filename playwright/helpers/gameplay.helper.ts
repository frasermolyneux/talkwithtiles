import type { Page, Locator } from '@playwright/test';
import { boardCenter, type GameType } from './test-data';

export interface TilePlacement {
  letter: string;
  x: number;
  y: number;
}

/**
 * Generates tile placements for common Scrabble words.
 * First word crosses the board center; subsequent words connect to existing tiles.
 */
export function placeWordHorizontally(
  word: string,
  startX: number,
  startY: number,
): TilePlacement[] {
  return word.split('').map((letter, i) => ({
    letter,
    x: startX + i,
    y: startY,
  }));
}

export function placeWordVertically(
  word: string,
  startX: number,
  startY: number,
): TilePlacement[] {
  return word.split('').map((letter, i) => ({
    letter,
    x: startX,
    y: startY + i,
  }));
}

/**
 * Generate the first word placement, centered on the board.
 */
export function firstWordPlacement(
  word: string,
  gameType: GameType,
  direction: 'horizontal' | 'vertical' = 'horizontal',
): TilePlacement[] {
  const center = boardCenter[gameType];
  const offset = Math.floor(word.length / 2);

  if (direction === 'horizontal') {
    return placeWordHorizontally(word, center.x - offset, center.y);
  }
  return placeWordVertically(word, center.x, center.y - offset);
}

/**
 * Place a tile on the board using click-to-place interaction.
 * Clicks the tile in the rack, then clicks the target board cell.
 */
export async function clickToPlaceTile(
  page: Page,
  tileLetter: string,
  targetX: number,
  targetY: number,
): Promise<void> {
  // Find the tile in the rack by letter
  const rackTile = page.getByTestId('scrabble-rack').locator(
    `img.rackScrabbleTile[src*="/${tileLetter.toLowerCase()}.jpg"]`,
  ).first();
  await rackTile.click();

  // Click the target board cell
  const boardCell = page.locator(`#cell_${targetX}-${targetY}`);
  await boardCell.click();
}

/**
 * Place a tile on the board using drag and drop.
 */
export async function dragToPlaceTile(
  page: Page,
  tileLetter: string,
  targetX: number,
  targetY: number,
): Promise<void> {
  const rackTile = page.getByTestId('scrabble-rack').locator(
    `img.rackScrabbleTile[src*="/${tileLetter.toLowerCase()}.jpg"]`,
  ).first();

  const boardCell = page.locator(`#cell_${targetX}-${targetY}`);

  await rackTile.dragTo(boardCell);
}

/**
 * Place a word on the board using click-to-place, one tile at a time.
 * Only places tiles that the player has in their rack.
 */
export async function placeWordByClicking(
  page: Page,
  placements: TilePlacement[],
): Promise<void> {
  for (const placement of placements) {
    await clickToPlaceTile(page, placement.letter, placement.x, placement.y);
  }
}

/**
 * Place a word on the board using drag and drop, one tile at a time.
 */
export async function placeWordByDragging(
  page: Page,
  placements: TilePlacement[],
): Promise<void> {
  for (const placement of placements) {
    await dragToPlaceTile(page, placement.letter, placement.x, placement.y);
  }
}

/**
 * Get the letters currently in the player's rack.
 */
export async function getRackLetters(page: Page): Promise<string[]> {
  const tiles = page.getByTestId('scrabble-rack').locator('img.rackScrabbleTile');
  const count = await tiles.count();
  const letters: string[] = [];

  for (let i = 0; i < count; i++) {
    const alt = await tiles.nth(i).getAttribute('alt');
    if (alt) {
      letters.push(alt.toUpperCase());
    }
  }

  return letters;
}

/**
 * Find the best word to play from available rack letters.
 * Returns the word and its placements, or null if no valid word can be formed.
 */
export function findPlayableWord(
  rackLetters: string[],
  wordList: string[],
): string | null {
  const available = [...rackLetters];

  for (const word of wordList) {
    const needed = word.split('');
    const tempAvailable = [...available];
    let canPlay = true;

    for (const letter of needed) {
      const idx = tempAvailable.indexOf(letter);
      if (idx === -1) {
        canPlay = false;
        break;
      }
      tempAvailable.splice(idx, 1);
    }

    if (canPlay) {
      return word;
    }
  }

  return null;
}

/**
 * Wait for the game page to fully load (board and rack rendered).
 */
export async function waitForGameReady(page: Page): Promise<void> {
  await page.getByTestId('scrabble-board').waitFor({ state: 'visible' });
  await page.getByTestId('scrabble-rack').waitFor({ state: 'visible' });
}

/**
 * Wait for an opponent's turn to complete (page reload triggered by etag polling).
 */
export async function waitForOpponentMove(page: Page, timeout = 30_000): Promise<void> {
  await page.waitForEvent('load', { timeout });
}
