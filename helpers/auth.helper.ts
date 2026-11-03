import { type Page, type APIRequestContext } from '@playwright/test';
import type { TestUser } from './test-data';

async function getAntiforgeryToken(request: APIRequestContext): Promise<string> {
  const response = await request.get('/sign-in');
  if (!response.ok()) {
    throw new Error(`Failed to load sign-in page: ${response.status()}`);
  }

  const html = await response.text();
  const token = html.match(/const antiforgeryToken = "([^"]+)"/)?.[1];
  if (!token) {
    throw new Error('Failed to find the antiforgery token on the sign-in page');
  }

  return token;
}

/**
 * Sign in as a test user by calling the TestAuthController endpoint.
 * Sets a cookie-based identity for the browser context.
 */
export async function signIn(page: Page, user: TestUser): Promise<void> {
  const antiforgeryToken = await getAntiforgeryToken(page.request);
  const response = await page.request.post('/api/test/signin', {
    headers: { RequestVerificationToken: antiforgeryToken },
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
  const antiforgeryToken = await getAntiforgeryToken(page.request);
  const response = await page.request.post('/api/test/signout', {
    headers: { RequestVerificationToken: antiforgeryToken },
  });
  if (!response.ok()) {
    throw new Error(`Failed to sign out: ${response.status()}`);
  }
}

/**
 * Sign in using a standalone API request context (useful for setup).
 */
export async function signInWithContext(request: APIRequestContext, user: TestUser): Promise<void> {
  const antiforgeryToken = await getAntiforgeryToken(request);
  const response = await request.post('/api/test/signin', {
    headers: { RequestVerificationToken: antiforgeryToken },
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
