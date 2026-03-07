import { type Page, type Locator, expect } from '@playwright/test';

export class SkipTurnModal {
  readonly page: Page;
  readonly modal: Locator;
  readonly confirmButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.modal = page.locator('#skipTurnModal');
    this.confirmButton = this.modal.locator('input[type="submit"], button[type="submit"]');
    this.cancelButton = this.modal.locator('button.btn-secondary[data-bs-dismiss="modal"]');
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
