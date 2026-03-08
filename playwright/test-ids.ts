/**
 * Centralised data-testid constants.
 * Every data-testid attribute in the Razor views should have a matching entry here.
 * Page objects reference these constants instead of hard-coding strings.
 */

// Navigation & Layout
export const NAV = {
  brand: 'nav-brand',
  home: 'nav-home',
  toggler: 'nav-toggler',
  scrabble: 'nav-scrabble',
  myGames: 'nav-my-games',
  createGame: 'nav-create-game',
  about: 'nav-about',
  feedback: 'nav-feedback',
  userMenu: 'nav-user-menu',
  signIn: 'nav-sign-in',
  signOut: 'nav-sign-out',
  contacts: 'nav-contacts',
  analytics: 'nav-analytics',
} as const;

export const LAYOUT = {
  alertContainer: 'alert-container',
  mainContent: 'main-content',
} as const;

// Home page
export const HOME = {
  welcomeBanner: 'welcome-banner',
  createGameButton: 'btn-create-game',
  carousel: 'home-carousel',
  signInButton: 'btn-sign-in',
} as const;

// Game Play
export const PLAY = {
  gameStateBanner: 'game-state-banner',
  playerSummary: 'player-summary',
  playerBadge: 'player-badge',
  board: 'scrabble-board',
  rack: 'scrabble-rack',
  turnScore: 'turn-score',
  gameControls: 'game-controls',
  btnSkipTurn: 'btn-skip-turn',
  btnExchangeTiles: 'btn-exchange-tiles',
  btnSubmitTurn: 'btn-submit-turn',
  btnIssueChallenge: 'btn-issue-challenge',
  btnUndoTurn: 'btn-undo-turn',
  btnRecallTiles: 'btn-recall-tiles',
  btnShuffleTiles: 'btn-shuffle-tiles',
  bagButton: 'bag-button',
  bagCount: 'bag-count',
  selectedTileMessage: 'selected-tile-message',
} as const;

// Modals
export const MODAL = {
  skipTurn: 'modal-skip-turn',
  skipTurnConfirm: 'modal-skip-turn-confirm',
  skipTurnCancel: 'modal-skip-turn-cancel',
  undoTurn: 'modal-undo-turn',
  undoTurnConfirm: 'modal-undo-turn-confirm',
  undoTurnCancel: 'modal-undo-turn-cancel',
  exchangeTiles: 'modal-exchange-tiles',
  exchangeTilesConfirm: 'modal-exchange-tiles-confirm',
  exchangeTilesCancel: 'modal-exchange-tiles-cancel',
  issueChallenge: 'modal-issue-challenge',
  issueChallengeReason: 'modal-issue-challenge-reason',
  issueChallengeText: 'modal-issue-challenge-text',
  issueChallengeConfirm: 'modal-issue-challenge-confirm',
  issueChallengeCancel: 'modal-issue-challenge-cancel',
  remainingTiles: 'modal-remaining-tiles',
  remainingTilesClose: 'modal-remaining-tiles-close',
} as const;

// Game Index
export const GAME_INDEX = {
  activeGames: 'active-games',
  completedGames: 'completed-games',
} as const;

// Create Game
export const CREATE_GAME = {
  form: 'create-game-form',
  gameType: 'select-game-type',
  tileBagVisibility: 'select-tile-bag-visibility',
  publicGame: 'checkbox-public-game',
  addPlayer: 'btn-add-player',
  additionalPlayers: 'additional-players',
  submit: 'btn-create-game-submit',
  advancedOptions: 'btn-advanced-options',
  overrideChallenge: 'checkbox-override-challenge',
} as const;

// Abandon Game
export const ABANDON_GAME = {
  warningCard: 'abandon-warning-card',
  confirm: 'btn-confirm-abandon',
  cancel: 'btn-cancel-abandon',
} as const;

// Delete Game
export const DELETE_GAME = {
  confirm: 'btn-confirm-delete',
} as const;

// Feedback
export const FEEDBACK = {
  form: 'feedback-form',
  submit: 'btn-submit-feedback',
} as const;

// Challenge Resolution
export const CHALLENGE = {
  container: 'handle-challenge-container',
  overrideSelect: 'challenge-override-select',
  btnAccept: 'btn-accept-challenge',
  btnReject: 'btn-reject-challenge',
} as const;

// Contacts
export const CONTACTS = {
  table: 'contacts-table',
} as const;
