import { test as base, type Page, type BrowserContext } from '@playwright/test';
import { signIn } from '../helpers/auth.helper';
import { players, type TestUser } from '../helpers/test-data';
import { PlayGamePage } from '../pages/play-game.page';
import { CreateGamePage } from '../pages/create-game.page';

/**
 * Represents a single player's browser session.
 */
export interface PlayerSession {
  context: BrowserContext;
  page: Page;
  user: TestUser;
  playPage: PlayGamePage;
}

/**
 * Extended test fixtures for multi-player Scrabble game testing.
 */
export const test = base.extend<{
  /** Two authenticated player sessions ready for gameplay. */
  twoPlayers: [PlayerSession, PlayerSession];
  /** Three authenticated player sessions. */
  threePlayers: [PlayerSession, PlayerSession, PlayerSession];
  /** Four authenticated player sessions. */
  fourPlayers: [PlayerSession, PlayerSession, PlayerSession, PlayerSession];
}>({
  twoPlayers: async ({ browser }, use) => {
    const sessions = await createPlayerSessions(browser, [players.player1, players.player2]);
    await use(sessions as [PlayerSession, PlayerSession]);
    await cleanupSessions(sessions);
  },

  threePlayers: async ({ browser }, use) => {
    const sessions = await createPlayerSessions(browser, [
      players.player1,
      players.player2,
      players.player3,
    ]);
    await use(sessions as [PlayerSession, PlayerSession, PlayerSession]);
    await cleanupSessions(sessions);
  },

  fourPlayers: async ({ browser }, use) => {
    const sessions = await createPlayerSessions(browser, [
      players.player1,
      players.player2,
      players.player3,
      players.player4,
    ]);
    await use(sessions as [PlayerSession, PlayerSession, PlayerSession, PlayerSession]);
    await cleanupSessions(sessions);
  },
});

async function createPlayerSessions(
  browser: import('@playwright/test').Browser,
  users: TestUser[],
): Promise<PlayerSession[]> {
  const sessions: PlayerSession[] = [];

  for (const user of users) {
    const context = await browser.newContext({ ignoreHTTPSErrors: true });
    const page = await context.newPage();
    await signIn(page, user);
    const playPage = new PlayGamePage(page);
    sessions.push({ context, page, user, playPage });
  }

  return sessions;
}

async function cleanupSessions(sessions: PlayerSession[]): Promise<void> {
  for (const session of sessions) {
    await session.context.close();
  }
}

export { expect } from '@playwright/test';
