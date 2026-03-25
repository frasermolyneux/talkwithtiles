// spec: playwright/specs/profile-feature.test-plan.md
// seed: tests/seed.spec.ts

import { test, expect } from '@playwright/test';
import { signIn } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';

test.describe('Profile Information Display and Validation', () => {
  test('Profile Shows Correct User Information', async ({ page }) => {
    await signIn(page, players.player1);

    // 1. Navigate to profile page as authenticated user
    await page.goto('/profile');
    await expect(page.getByRole('heading', { name: 'Your Profile' })).toBeVisible();

    // 2. Verify Name field is displayed
    await expect(page.getByTestId('profile-name')).toBeVisible();
    await expect(page.getByTestId('profile-name')).toHaveText('Alice');

    // 3. Verify Email field is displayed
    await expect(page.getByTestId('profile-email')).toBeVisible();
    await expect(page.getByTestId('profile-email')).toHaveText('alice@test.local');

    // 4. Verify User ID field is displayed
    await expect(page.getByTestId('profile-user-id')).toBeVisible();
    await expect(page.getByTestId('profile-user-id')).toHaveText('11111111-1111-1111-1111-111111111111');
  });
});