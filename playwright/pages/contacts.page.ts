import { type Page, type Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class ContactsPage extends BasePage {
  readonly contactsTable: Locator;
  readonly pageHeading: Locator;

  constructor(page: Page) {
    super(page);
    this.contactsTable = page.locator('table');
    this.pageHeading = page.locator('h2, h3').first();
  }

  async goto(): Promise<void> {
    await this.page.goto('/Contacts');
  }

  async getContactCount(): Promise<number> {
    const rows = this.contactsTable.locator('tbody tr');
    return rows.count();
  }

  async deleteContact(index: number): Promise<void> {
    const deleteButton = this.contactsTable.locator('a[href*="DeleteContact"]').nth(index);
    await deleteButton.click();
  }
}
