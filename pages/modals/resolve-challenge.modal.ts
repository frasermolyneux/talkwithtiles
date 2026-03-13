import { type Page, type Locator, expect } from '@playwright/test';

export class ResolveChallengeModal {
  readonly page: Page;
  readonly container: Locator;
  readonly acceptButton: Locator;
  readonly rejectButton: Locator;
  readonly overrideSelect: Locator;

  constructor(page: Page) {
    this.page = page;
    this.container = page.getByTestId('handle-challenge-container');
    this.acceptButton = page.getByTestId('btn-accept-challenge');
    this.rejectButton = page.getByTestId('btn-reject-challenge');
    this.overrideSelect = page.getByTestId('challenge-override-select');
  }

  async accept(): Promise<void> {
    await this.acceptButton.click();
    await this.page.waitForURL(/\/Scrabble\/Play\//);
  }

  async reject(): Promise<void> {
    await this.rejectButton.click();
    await this.page.waitForURL(/\/Scrabble\/Play\//);
  }

  async selectOvercome(outcome: string): Promise<void> {
    if (await this.overrideSelect.isVisible()) {
      await this.overrideSelect.selectOption({ label: outcome });
    }
  }
}
