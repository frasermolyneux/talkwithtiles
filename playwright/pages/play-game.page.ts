import { type Page, type Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class PlayGamePage extends BasePage {
  // Board
  readonly board: Locator;

  // Rack
  readonly rack: Locator;
  readonly recallButton: Locator;
  readonly shuffleButton: Locator;
  readonly selectedTileMessage: Locator;

  // Score
  readonly turnScore: Locator;
  readonly moveErrorMessage: Locator;

  // Game controls
  readonly submitTurnButton: Locator;
  readonly skipTurnButton: Locator;
  readonly exchangeTilesButton: Locator;
  readonly undoLastTurnButton: Locator;
  readonly issueChallengeButton: Locator;

  // Player summary
  readonly playerSummary: Locator;

  // Game state
  readonly gameStateBanner: Locator;

  // Tile bag
  readonly bagButton: Locator;
  readonly bagCount: Locator;

  constructor(page: Page) {
    super(page);

    // Board — keep #scrabbleBoard as it's used by JavaScript
    this.board = page.getByTestId('scrabble-board');

    // Rack — keep #scrabbleRack as it's used by JavaScript
    this.rack = page.getByTestId('scrabble-rack');
    this.recallButton = page.getByTestId('btn-recall-tiles');
    this.shuffleButton = page.getByTestId('btn-shuffle-tiles');
    this.selectedTileMessage = page.getByTestId('selected-tile-message');

    // Score
    this.turnScore = page.getByTestId('turn-score');
    this.moveErrorMessage = page.getByTestId('move-error-message');

    // Controls
    this.submitTurnButton = page.getByTestId('btn-submit-turn');
    this.skipTurnButton = page.getByTestId('btn-skip-turn');
    this.exchangeTilesButton = page.getByTestId('btn-exchange-tiles');
    this.undoLastTurnButton = page.getByTestId('btn-undo-turn');
    this.issueChallengeButton = page.getByTestId('btn-issue-challenge');

    // Player info
    this.playerSummary = page.getByTestId('player-summary');
    this.gameStateBanner = page.getByTestId('game-state-banner');
    this.bagButton = page.getByTestId('bag-button').first();
    this.bagCount = page.getByTestId('bag-count').first();
  }

  async goto(gameId: string): Promise<void> {
    await this.page.goto(`/Scrabble/Play/${gameId}`);
  }

  async waitForReady(): Promise<void> {
    await this.board.waitFor({ state: 'visible' });
    await this.rack.waitFor({ state: 'visible' });
    // Wait for JS to populate rack tiles (dynamically added by InitTileRack)
    await this.rack.locator('img.rackScrabbleTile').first().waitFor({
      state: 'visible',
      timeout: 10_000,
    });
  }

  /**
   * Wait for the board to be fully stable after navigation.
   * After invite auto-linking, the etag polling (5s interval) may trigger
   * a page reload. This method absorbs any such reload before interaction.
   */
  async waitForStableBoard(): Promise<void> {
    await this.waitForReady();
    try {
      // Wait up to 7s for a potential etag-triggered reload
      await this.page.waitForEvent('load', { timeout: 7_000 });
      // A reload occurred — wait for the board to be ready again
      await this.waitForReady();
    } catch {
      // No reload within 7 seconds — board is already stable
    }
  }

  // --- Board interaction ---

  getBoardCell(x: number, y: number): Locator {
    return this.page.locator(`#cell_${x}-${y}`);
  }

  getAvailableBoardCell(x: number, y: number): Locator {
    return this.page.locator(`#cell_${x}-${y}.availableBoardCell`);
  }

  // --- Rack interaction ---

  getRackCell(position: number): Locator {
    return this.page.locator(`#rack_${position}`);
  }

  getRackTiles(): Locator {
    return this.rack.locator('img.rackScrabbleTile');
  }

  async getRackLetters(): Promise<string[]> {
    const tiles = this.getRackTiles();
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
   * Find a rack tile by letter and click it to select.
   */
  async selectRackTile(letter: string): Promise<void> {
    const tile = this.rack.locator(
      `img.rackScrabbleTile[src*="/${letter.toLowerCase()}.jpg"]`,
    ).first();
    await tile.click();
  }

  /**
   * Click-to-place: select tile from rack, then click board cell.
   */
  async clickToPlace(letter: string, x: number, y: number): Promise<void> {
    await this.selectRackTile(letter);
    const cell = this.getBoardCell(x, y);
    await cell.click();
  }

  /**
   * Drag-to-place: dispatch proper HTML5 drag events with DataTransfer.
   * Playwright's built-in dragTo() doesn't populate dataTransfer correctly.
   */
  async dragToPlace(letter: string, x: number, y: number): Promise<void> {
    const rackTile = this.rack.locator(
      `img.rackScrabbleTile[src*="/${letter.toLowerCase()}.jpg"]`,
    ).first();
    const cellId = `cell_${x}-${y}`;

    // Wait for the board to be stable (etag polling may trigger reloads)
    await this.board.waitFor({ state: 'visible', timeout: 10_000 });
    await rackTile.waitFor({ state: 'visible', timeout: 5_000 });

    // Get the tile ID from the rack element
    const tileId = await rackTile.getAttribute('id');
    if (!tileId) throw new Error(`No id attribute on rack tile for letter ${letter}`);

    // Dispatch HTML5 drag events in the page context.
    // Chromium restricts DataTransfer.getData() on synthetic DragEvents,
    // so we override the dataTransfer property with a plain object shim.
    await this.page.evaluate(
      ([tid, cid]) => {
        const tileEl = document.querySelector(`[id="${tid}"]`) as HTMLElement;
        const cellEl = document.getElementById(cid);
        if (!tileEl) throw new Error(`Tile element not found: ${tid}`);
        if (!cellEl) throw new Error(`Cell element not found: ${cid}`);

        const dtShim = {
          data: {} as Record<string, string>,
          setData(fmt: string, val: string) { this.data[fmt] = val; },
          getData(fmt: string) { return this.data[fmt] ?? ''; },
          effectAllowed: 'move',
          dropEffect: 'move',
        };
        dtShim.setData('text/plain', tid);

        function fireDrag(target: HTMLElement, type: string) {
          const evt = new DragEvent(type, { bubbles: true, cancelable: true });
          Object.defineProperty(evt, 'dataTransfer', { get: () => dtShim });
          target.dispatchEvent(evt);
        }

        fireDrag(tileEl, 'dragstart');
        fireDrag(cellEl, 'dragover');
        fireDrag(cellEl, 'drop');
        fireDrag(tileEl, 'dragend');
      },
      [tileId, cellId],
    );
  }

  /**
   * Place a full word on the board using click-to-place.
   */
  async placeWord(
    word: string,
    startX: number,
    startY: number,
    direction: 'horizontal' | 'vertical' = 'horizontal',
  ): Promise<void> {
    for (let i = 0; i < word.length; i++) {
      const x = direction === 'horizontal' ? startX + i : startX;
      const y = direction === 'vertical' ? startY + i : startY;
      await this.clickToPlace(word[i], x, y);
    }
  }

  /**
   * Place a full word using drag-and-drop.
   */
  async placeWordByDrag(
    word: string,
    startX: number,
    startY: number,
    direction: 'horizontal' | 'vertical' = 'horizontal',
  ): Promise<void> {
    for (let i = 0; i < word.length; i++) {
      const x = direction === 'horizontal' ? startX + i : startX;
      const y = direction === 'vertical' ? startY + i : startY;
      await this.dragToPlace(word[i], x, y);
    }
  }

  async recallTiles(): Promise<void> {
    await this.recallButton.click();
  }

  async shuffleTiles(): Promise<void> {
    await this.shuffleButton.click();
  }

  // --- Turn actions ---

  async submitMove(): Promise<void> {
    // submitPlayerMove() does fetch POST then location.reload()
    await this.submitTurnButton.click();
    await this.page.waitForEvent('load', { timeout: 15_000 });
  }

  async getTurnScoreText(): Promise<string> {
    return (await this.turnScore.textContent()) ?? '';
  }

  // --- Game state ---

  async isCurrentPlayer(): Promise<boolean> {
    return this.submitTurnButton.isVisible();
  }

  async getPlayerScores(): Promise<Map<string, number>> {
    const scores = new Map<string, number>();
    const badges = this.playerSummary.locator('[data-testid="player-badge"]');
    const count = await badges.count();

    for (let i = 0; i < count; i++) {
      const text = (await badges.nth(i).textContent())?.replace(/\s+/g, ' ').trim();
      if (text) {
        const match = text.match(/(.+?)\s*:\s*(\d+)/);
        if (match) {
          scores.set(match[1].trim(), parseInt(match[2], 10));
        }
      }
    }

    return scores;
  }

  async getBagTileCount(): Promise<number> {
    const text = await this.bagCount.textContent();
    return parseInt(text ?? '0', 10);
  }

  async isGameComplete(): Promise<boolean> {
    return this.gameStateBanner.isVisible();
  }

  /**
   * Wait for the opponent's turn to complete (etag polling triggers reload).
   */
  async waitForOpponentMove(timeout = 30_000): Promise<void> {
    await this.page.waitForEvent('load', { timeout });
  }

  /**
   * Extract the game ID from the current URL.
   */
  getGameIdFromUrl(): string {
    const match = this.page.url().match(/Play\/([a-f0-9-]+)/i);
    if (!match) throw new Error('Not on a Play page');
    return match[1];
  }
}
