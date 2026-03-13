// spec: playwright/specs/profile-feature.test-plan.md
// seed: tests/seed.spec.ts

import { test, expect } from '@playwright/test';

test.describe('Profile Page Layout and UI Validation', () => {
  test('Profile Page Layout and Structure', async ({ page }) => {
    // 1. Navigate to profile page
    await page.goto('https://localhost:5001/profile');
    await expect(page.getByText('Talk With Tiles')).toBeVisible();
    await expect(page.getByText('© 2026 - Molyneux.IO')).toBeVisible();

    // 2. Verify page heading and structure
    await expect(page.getByRole('heading', { name: 'Your Profile' })).toBeVisible();
    await expect(page.getByText('Name')).toBeVisible();

    // 3. Check the avatar or initial display
    await expect(page.getByText('A')).toBeVisible();

    // 4. Verify all profile fields are non-editable by checking for absence of form inputs
    await expect(page.locator('input, textarea, select').count()).toBeLessThanOrEqual(0);
    
    // Verify the information is displayed as text, not in form fields
    await expect(page.getByText('Alice')).toBeVisible();
    await expect(page.getByText('alice@test.local')).toBeVisible();
    await expect(page.getByText('11111111-1111-1111-1111-111111111111')).toBeVisible();
  });
});