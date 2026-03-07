import { type Page, type Locator, expect } from '@playwright/test';
import { signIn, signOut } from '../helpers/auth.helper';
import type { TestUser } from '../helpers/test-data';

/**
 * Base page object with shared navigation and utility methods.
 */
export class BasePage {
  readonly page: Page;
  readonly navbar: Locator;
  readonly alertContainer: Locator;
  readonly homeLink: Locator;
  readonly scrabbleDropdown: Locator;
  readonly aboutDropdown: Locator;
  readonly feedbackLink: Locator;
  readonly userDropdown: Locator;

  constructor(page: Page) {
    this.page = page;
    this.navbar = page.locator('nav.navbar');
    this.alertContainer = page.locator('alerts, .alert');
    this.homeLink = page.locator('a.nav-link', { hasText: 'Home' });
    this.scrabbleDropdown = page.locator('a.nav-link.dropdown-toggle', { hasText: 'Scrabble' });
    this.aboutDropdown = page.locator('a.nav-link.dropdown-toggle', { hasText: 'About/Guides' });
    this.feedbackLink = page.locator('a.nav-link', { hasText: 'Feedback' });
    this.userDropdown = page.locator('#navbarDropdown');
  }

  async signInAs(user: TestUser): Promise<void> {
    await signIn(this.page, user);
  }

  async signOutUser(): Promise<void> {
    await signOut(this.page);
  }

  async navigateHome(): Promise<void> {
    await this.homeLink.click();
  }

  async getAlertText(): Promise<string | null> {
    const alert = this.page.locator('.alert').first();
    if (await alert.isVisible()) {
      return alert.textContent();
    }
    return null;
  }

  async expectNoErrors(): Promise<void> {
    const errorAlert = this.page.locator('.alert-danger');
    await expect(errorAlert).not.toBeVisible();
  }
}
