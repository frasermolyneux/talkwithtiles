import { type Page, type Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class ScrabbleIndexPage extends BasePage {
  readonly activeGamesTable: Locator;
  readonly completedGamesTable: Locator;
  readonly createGameLink: Locator;
  readonly pageHeading: Locator;

  constructor(page: Page) {
    super(page);
    this.activeGamesTable = page.getByTestId('active-games');
    this.completedGamesTable = page.getByTestId('completed-games');
    this.createGameLink = page.getByTestId('nav-create-game');
    this.pageHeading = page.getByRole('heading', { level: 4 });
  }

  async goto(): Promise<void> {
    await this.page.goto('/Scrabble');
  }

  async expectGameInList(gameType?: string): Promise<void> {
    await expect(this.activeGamesTable).toBeVisible();
    if (gameType) {
      await expect(this.activeGamesTable).toContainText(gameType);
    }
  }

  async getActiveGameLinks(): Promise<Locator> {
    return this.activeGamesTable.locator('a[href*="Scrabble/Play"]');
  }

  async clickFirstActiveGame(): Promise<void> {
    const links = await this.getActiveGameLinks();
    await links.first().click();
  }

  async getGameIdFromFirstRow(): Promise<string> {
    const link = this.activeGamesTable.locator('a[href*="Scrabble/Play"]').first();
    const href = await link.getAttribute('href');
    const match = href?.match(/Play\/([a-f0-9-]+)/i);
    if (!match) throw new Error('Could not extract game ID from link');
    return match[1];
  }
}
