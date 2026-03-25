// spec: playwright/specs/profile-feature.test-plan.md
// seed: tests/seed.spec.ts

import { test, expect } from '@playwright/test';
import { signIn } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';

test.describe('Profile Page Layout and UI Validation', () => {
  test('Profile Page Layout and Structure', async ({ page }) => {
    await signIn(page, players.player1);

    // 1. Navigate to profile page
    await page.goto('/profile');
    await expect(page.getByTestId('nav-brand')).toBeVisible();
    await expect(page.getByText('© 2026 - Molyneux.IO')).toBeVisible();

    // 2. Verify page heading and structure
    await expect(page.getByRole('heading', { name: 'Your Profile' })).toBeVisible();
    await expect(page.getByTestId('profile-card')).toBeVisible();

    // 3. Check the avatar or initial display
    await expect(page.getByTestId('profile-card').locator('.rounded-circle')).toHaveText('A');

    // 4. Verify all profile fields are non-editable by checking for absence of form inputs
    await expect(page.locator('input, textarea, select')).toHaveCount(0);

    // Verify the information is displayed as text, not in form fields
    await expect(page.getByTestId('profile-name')).toHaveText('Alice');
    await expect(page.getByTestId('profile-email')).toHaveText('alice@test.local');
    await expect(page.getByTestId('profile-user-id')).toHaveText('11111111-1111-1111-1111-111111111111');
  });
});