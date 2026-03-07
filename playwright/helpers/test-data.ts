/**
 * Test user definitions for multi-player scenarios.
 * UserIds are deterministic GUIDs so game state is predictable.
 */
export interface TestUser {
  userId: string;
  userName: string;
  email: string;
  role?: string;
}

export const players = {
  player1: {
    userId: '11111111-1111-1111-1111-111111111111',
    userName: 'Alice',
    email: 'alice@test.local',
  } satisfies TestUser,

  player2: {
    userId: '22222222-2222-2222-2222-222222222222',
    userName: 'Bob',
    email: 'bob@test.local',
  } satisfies TestUser,

  player3: {
    userId: '33333333-3333-3333-3333-333333333333',
    userName: 'Charlie',
    email: 'charlie@test.local',
  } satisfies TestUser,

  player4: {
    userId: '44444444-4444-4444-4444-444444444444',
    userName: 'Diana',
    email: 'diana@test.local',
  } satisfies TestUser,

  admin: {
    userId: '00000000-0000-0000-0000-000000000001',
    userName: 'Admin User',
    email: 'admin@test.local',
    role: 'Admin',
  } satisfies TestUser,
};

export type GameType = 'MiniBoard' | 'StandardBoard' | 'SuperSizeBoard';

export interface GameConfig {
  gameType: GameType;
  players: TestUser[];
}

export const defaultGameConfig: GameConfig = {
  gameType: 'StandardBoard',
  players: [players.player1, players.player2],
};

/**
 * Common Scrabble words for realistic test gameplay.
 * Grouped by length for strategic placement.
 */
export const scrabbleWords = {
  twoLetter: ['AT', 'IS', 'IT', 'IN', 'ON', 'TO', 'DO', 'GO', 'UP', 'AN', 'OR', 'IF', 'NO', 'SO', 'HE', 'WE', 'BE'],
  threeLetter: ['CAT', 'DOG', 'HAT', 'RUN', 'SIT', 'THE', 'AND', 'FOR', 'BUT', 'NOT', 'HAS', 'HIS', 'HER', 'CAN', 'SET', 'RED', 'BIG', 'OLD', 'NEW', 'TOP', 'TEN', 'SIX', 'OAK', 'JAR', 'ZAP'],
  fourLetter: ['CATS', 'DOGS', 'FISH', 'BIRD', 'TREE', 'STAR', 'MOON', 'RAIN', 'FIRE', 'WIND', 'JUMP', 'PLAY', 'HAND', 'WORD', 'TILE', 'GAME', 'DARK', 'QUIZ', 'FLEX'],
  fiveLetter: ['HELLO', 'WORLD', 'HOUSE', 'WATER', 'STONE', 'LIGHT', 'PLANT', 'GRAIN', 'SOUTH', 'NORTH', 'QUEST'],
  sixLetter: ['CASTLE', 'STREAM', 'FOREST', 'GARDEN', 'BRIDGE', 'MARKET'],
  sevenLetter: ['FLOWERS', 'KITCHEN', 'SHELTER', 'JOURNEY', 'QUARTER'],
};

/**
 * Board center positions by game type (0-indexed).
 */
export const boardCenter: Record<GameType, { x: number; y: number }> = {
  MiniBoard: { x: 4, y: 4 },
  StandardBoard: { x: 7, y: 7 },
  SuperSizeBoard: { x: 9, y: 9 },
};
