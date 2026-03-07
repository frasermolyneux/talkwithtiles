import { type Page, type Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class FeedbackPage extends BasePage {
  readonly feedbackForm: Locator;
  readonly submitButton: Locator;

  constructor(page: Page) {
    super(page);
    this.feedbackForm = page.locator('form[action*="Feedback"]');
    this.submitButton = page.locator('button[type="submit"], input[type="submit"]');
  }

  async goto(): Promise<void> {
    await this.page.goto('/Feedback');
  }

  async submitFeedback(): Promise<void> {
    await this.submitButton.click();
  }
}
