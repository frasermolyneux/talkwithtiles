import { Client } from 'pg';
import { readFileSync } from 'node:fs';

async function globalSetup(): Promise<void> {
  const client = new Client({
    host: process.env.TEST_POSTGRES_HOST ?? '127.0.0.1',
    port: Number(process.env.TEST_POSTGRES_PORT ?? '5432'),
    database: 'talkwithtiles',
    user: 'talkwithtiles',
    password: process.env.TEST_POSTGRES_PASSWORD
      ?? readFileSync(process.env.TEST_POSTGRES_PASSWORD_FILE ?? '.local/database-password', 'utf8').trimEnd(),
  });
  await client.connect();
  try {
    // This harness only targets the disposable local/CI database.
    await client.query('TRUNCATE games, game_players, invites, contacts CASCADE');
  } finally {
    await client.end();
  }
}

export default globalSetup;
