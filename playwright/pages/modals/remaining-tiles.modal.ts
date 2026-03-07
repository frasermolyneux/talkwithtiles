import { type Page, type Locator, expect } from '@playwright/test';

export class RemainingTilesModal {
  readonly page: Page;
  readonly modal: Locator;
  readonly tileList: Locator;
  readonly closeButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.modal = page.locator('#remainingTilesModal');
    this.tileList = this.modal.locator('.badge');
    this.closeButton = this.modal.locator('[data-bs-dismiss="modal"]');
  }

  async getTileCount(): Promise<number> {
    return this.tileList.count();
  }

  async close(): Promise<void> {
    await this.closeButton.click();
    await this.modal.waitFor({ state: 'hidden' });
  }
}
