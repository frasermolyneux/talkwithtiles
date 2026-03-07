import { type Page, type Locator, expect } from '@playwright/test';

export class IssueChallengeModal {
  readonly page: Page;
  readonly modal: Locator;
  readonly reasonSelect: Locator;
  readonly challengeText: Locator;
  readonly confirmButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.modal = page.locator('#issueChallengeModal');
    this.reasonSelect = this.modal.locator('select');
    this.challengeText = this.modal.locator('input[type="text"], textarea');
    this.confirmButton = this.modal.locator('input[type="submit"], button[type="submit"]');
    this.cancelButton = this.modal.locator('button.btn-secondary[data-bs-dismiss="modal"]');
  }

  async selectReason(reason: string): Promise<void> {
    await this.reasonSelect.selectOption({ label: reason });
  }

  async enterChallengeText(text: string): Promise<void> {
    if (await this.challengeText.isVisible()) {
      await this.challengeText.fill(text);
    }
  }

  async confirm(reason?: string, text?: string): Promise<void> {
    if (reason) await this.selectReason(reason);
    if (text) await this.enterChallengeText(text);
    await this.confirmButton.click();
    await this.page.waitForURL(/\/Scrabble\/Play\//);
  }

  async cancel(): Promise<void> {
    await this.cancelButton.click();
    await this.modal.waitFor({ state: 'hidden' });
  }
}
