import { type Page, type Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class AbandonGamePage extends BasePage {
  readonly confirmButton: Locator;
  readonly cancelButton: Locator;
  readonly warningCard: Locator;

  constructor(page: Page) {
    super(page);
    this.warningCard = page.locator('.card.border-danger, .card');
    this.confirmButton = page.locator('input[type="submit"], button[type="submit"]');
    this.cancelButton = page.locator('a:has-text("Cancel"), a:has-text("Return")');
  }

  async goto(gameId: string): Promise<void> {
    await this.page.goto(`/Scrabble/Abandon/${gameId}`);
  }

  async confirmAbandon(): Promise<void> {
    await this.confirmButton.click();
    await this.page.waitForURL(/\/Scrabble/);
  }

  async cancel(): Promise<void> {
    await this.cancelButton.click();
  }
}
