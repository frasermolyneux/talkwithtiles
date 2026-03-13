import { type Page, type APIRequestContext } from '@playwright/test';
import type { TestUser } from './test-data';

/**
 * Sign in as a test user by calling the TestAuthController endpoint.
 * Sets a cookie-based identity for the browser context.
 */
export async function signIn(page: Page, user: TestUser): Promise<void> {
  const response = await page.request.post('/api/test/signin', {
    data: {
      userId: user.userId,
      userName: user.userName,
      email: user.email,
      role: user.role ?? null,
    },
  });

  if (!response.ok()) {
    throw new Error(`Failed to sign in as ${user.userName}: ${response.status()} ${await response.text()}`);
  }
}

/**
 * Sign out the current test user.
 */
export async function signOut(page: Page): Promise<void> {
  const response = await page.request.post('/api/test/signout');
  if (!response.ok()) {
    throw new Error(`Failed to sign out: ${response.status()}`);
  }
}

/**
 * Sign in using a standalone API request context (useful for setup).
 */
export async function signInWithContext(request: APIRequestContext, user: TestUser): Promise<void> {
  const response = await request.post('/api/test/signin', {
    data: {
      userId: user.userId,
      userName: user.userName,
      email: user.email,
      role: user.role ?? null,
    },
  });

  if (!response.ok()) {
    throw new Error(`Failed to sign in as ${user.userName}: ${response.status()} ${await response.text()}`);
  }
}
