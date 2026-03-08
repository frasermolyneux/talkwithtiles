import { type Page, type Locator, expect } from '@playwright/test';

export class UndoTurnModal {
  readonly page: Page;
  readonly modal: Locator;
  readonly confirmButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.modal = page.getByTestId('modal-undo-turn');
    this.confirmButton = page.getByTestId('modal-undo-turn-confirm');
    this.cancelButton = page.getByTestId('modal-undo-turn-cancel');
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
