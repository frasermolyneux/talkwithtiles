import { test, expect } from '@playwright/test';
import { signIn } from '../helpers/auth.helper';
import { players } from '../helpers/test-data';
import { FeedbackPage } from '../pages/feedback.page';

test.describe('Feedback', () => {
  test('authenticated user can access feedback page', async ({ page }) => {
    await signIn(page, players.player1);

    const feedbackPage = new FeedbackPage(page);
    await feedbackPage.goto();

    await expect(page).toHaveURL(/feedback/i);
    await expect(feedbackPage.submitButton).toBeVisible();
  });
});
