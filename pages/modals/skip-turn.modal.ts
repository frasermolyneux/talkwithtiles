import { type Page, type Locator, expect } from '@playwright/test';

export class SkipTurnModal {
  readonly page: Page;
  readonly modal: Locator;
  readonly confirmButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.modal = page.getByTestId('modal-skip-turn');
    this.confirmButton = page.getByTestId('modal-skip-turn-confirm');
    this.cancelButton = page.getByTestId('modal-skip-turn-cancel');
  }

  async confirm(): Promise<void> {
    await this.confirmButton.click();
    await this.page.waitForURL(/\/Scrabble\/Play\//);
  }

  async cancel(): Promise<void> {
    await this.cancelButton.click();
    await this.modal.waitFor({ state: 'hidden' });
  }
}
