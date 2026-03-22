// spec: playwright/specs/profile-feature.test-plan.md
// seed: tests/seed.spec.ts

import { test, expect } from '@playwright/test';
import { signIn } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';

test.describe('User Profile Access and Authentication', () => {
  test('Authenticated User Can Access Profile', async ({ page }) => {
    await signIn(page, players.player1);

    // 1. Navigate to homepage with authenticated user
    await page.goto('/');
    await expect(page.getByRole('button', { name: 'Alice' })).toBeVisible();

    // 2. Click on user dropdown menu (user's name)
    await page.getByTestId('nav-user-menu').click();
    await expect(page.getByRole('link', { name: ' Profile' })).toBeVisible();

    // 3. Click on 'Profile' link from dropdown menu
    await page.getByTestId('nav-profile').click();
    await expect(page.getByRole('heading', { name: 'Your Profile' })).toBeVisible();

    // Verify we navigated to the profile page
    await expect(page).toHaveURL('/profile');
    await expect(page).toHaveTitle('Profile - Talk With Tiles');
  });
});