// spec: playwright/specs/profile-feature.test-plan.md
// seed: tests/seed.spec.ts

import { test, expect } from '@playwright/test';

test.describe('Profile Navigation and User Experience', () => {
  test('Profile Navigation Flow', async ({ page }) => {
    // 1. Start from homepage and navigate to profile
    await page.goto('/');
    await page.getByTestId('nav-user-menu').click();
    await page.getByTestId('nav-profile').click();

    // Verify we reached the profile page
    await expect(page).toHaveURL('/profile');
    await expect(page.getByRole('heading', { name: 'Your Profile' })).toBeVisible();

    // 2. From profile page, test navigation to other sections
    await page.getByTestId('nav-home').click();
    await expect(page).toHaveURL('/');
    await expect(page.getByText('Welcome Alice!')).toBeVisible();

    // Test navigation to Feedback page
    await page.getByTestId('nav-feedback').click();
    await expect(page).toHaveURL('/feedback');
    await expect(page.getByRole('heading', { name: 'Feedback' })).toBeVisible();

    // 3. Test browser back button functionality
    await page.goBack();
    await expect(page).toHaveURL('/');
    await expect(page.getByText('Welcome Alice!')).toBeVisible();

    // 4. Test direct URL access to profile
    await page.goto('/profile');
    await expect(page).toHaveURL('/profile');
    await expect(page.getByRole('heading', { name: 'Your Profile' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Alice' })).toBeVisible();
  });
});