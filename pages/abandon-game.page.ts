import { type Page, type Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class AbandonGamePage extends BasePage {
  readonly confirmButton: Locator;
  readonly cancelButton: Locator;
  readonly warningCard: Locator;

  constructor(page: Page) {
    super(page);
    this.warningCard = page.getByTestId('abandon-warning-card');
    this.confirmButton = page.getByTestId('btn-confirm-abandon');
    this.cancelButton = page.getByTestId('btn-cancel-abandon');
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
