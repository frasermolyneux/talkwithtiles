import { type Page, type Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class HomePage extends BasePage {
  readonly welcomeBanner: Locator;
  readonly createGameButton: Locator;
  readonly carousel: Locator;
  readonly signInButton: Locator;

  constructor(page: Page) {
    super(page);
    this.welcomeBanner = page.locator('.bg-light.p-5');
    this.createGameButton = page.locator('a', { hasText: 'Create Scrabble Game' });
    this.carousel = page.locator('#carouselExampleIndicators');
    this.signInButton = page.locator('a', { hasText: 'Continue with Microsoft' });
  }

  async goto(): Promise<void> {
    await this.page.goto('/');
  }

  async expectWelcomeBanner(userName: string): Promise<void> {
    await expect(this.welcomeBanner).toContainText(`Welcome ${userName}`);
  }

  async expectSignInPrompt(): Promise<void> {
    await expect(this.signInButton).toBeVisible();
  }

  async clickCreateGame(): Promise<void> {
    await this.createGameButton.click();
  }
}
