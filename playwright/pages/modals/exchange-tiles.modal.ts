import { type Page, type Locator, expect } from '@playwright/test';

export class ExchangeTilesModal {
  readonly page: Page;
  readonly modal: Locator;
  readonly confirmButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.modal = page.getByTestId('modal-exchange-tiles');
    this.confirmButton = page.getByTestId('modal-exchange-tiles-confirm');
    this.cancelButton = page.getByTestId('modal-exchange-tiles-cancel');
  }

  async selectTile(index: number): Promise<void> {
    const checkbox = this.modal.locator('input[type="checkbox"]').nth(index);
    await checkbox.check();
  }

  async selectTiles(indices: number[]): Promise<void> {
    for (const i of indices) {
      await this.selectTile(i);
    }
  }

  async selectAllTiles(): Promise<void> {
    const checkboxes = this.modal.locator('input[type="checkbox"]');
    const count = await checkboxes.count();
    for (let i = 0; i < count; i++) {
      await checkboxes.nth(i).check();
    }
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
