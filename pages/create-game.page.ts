import { type Page, type Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';
import type { GameType, TestUser } from '../helpers/test-data';

export class CreateGamePage extends BasePage {
  readonly form: Locator;
  readonly gameTypeSelect: Locator;
  readonly tileBagVisibilitySelect: Locator;
  readonly publicGameCheckbox: Locator;
  readonly addPlayerButton: Locator;
  readonly additionalPlayersContainer: Locator;
  readonly submitButton: Locator;
  readonly advancedOptionsToggle: Locator;
  readonly overrideChallengeCheckbox: Locator;
  readonly thatsNotAWordSelect: Locator;
  readonly thatsNotAValidTurnSelect: Locator;
  readonly catchallSelect: Locator;

  constructor(page: Page) {
    super(page);
    this.form = page.getByTestId('create-game-form');
    this.gameTypeSelect = page.getByTestId('select-game-type');
    this.tileBagVisibilitySelect = page.locator('#TileBagVisibilityOption');
    this.publicGameCheckbox = page.getByTestId('checkbox-public-game');
    this.addPlayerButton = page.getByTestId('btn-add-player');
    this.additionalPlayersContainer = page.getByTestId('additional-players');
    this.submitButton = page.getByTestId('btn-create-game-submit');
    this.advancedOptionsToggle = page.getByTestId('btn-advanced-options');
    this.overrideChallengeCheckbox = page.getByTestId('checkbox-override-challenge');
    this.thatsNotAWordSelect = page.locator('#ThatsNotAWordGameChallengeResult');
    this.thatsNotAValidTurnSelect = page.locator('#ThatsNotAValidTurnGameChallengeResult');
    this.catchallSelect = page.locator('#CatchallGameChallengeResult');
  }

  async goto(): Promise<void> {
    await this.page.goto('/Scrabble/Create');
  }

  async selectGameType(gameType: GameType): Promise<void> {
    // ASP.NET GetEnumSelectList renders integer values, not enum names
    const valueMap: Record<GameType, string> = {
      StandardBoard: '0',
      SuperSizeBoard: '1',
      MiniBoard: '2',
    };
    await this.gameTypeSelect.selectOption(valueMap[gameType]);
  }

  async addPlayer(email: string, index: number): Promise<void> {
    if (index === 0) {
      // Fill the pre-existing first opponent input from the EditorTemplate
      const firstInput = this.form.locator('#PlayerModels_0__Identifier');
      await firstInput.fill(email);
    } else {
      // Click "Add Additional Player" and fill the dynamically created input
      await this.addPlayerButton.click();
      const newInput = this.page.locator(`#PlayerModels_${index}__Identifier`);
      await newInput.waitFor({ state: 'visible' });
      await newInput.fill(email);
    }
  }

  async addPlayers(players: TestUser[]): Promise<void> {
    for (let i = 0; i < players.length; i++) {
      await this.addPlayer(players[i].email, i);
    }
  }

  async setPublicGame(isPublic: boolean): Promise<void> {
    if (isPublic) {
      await this.publicGameCheckbox.check();
    } else {
      await this.publicGameCheckbox.uncheck();
    }
  }

  async openAdvancedOptions(): Promise<void> {
    const collapseContent = this.page.locator('#collapseOne');
    if (!(await collapseContent.isVisible())) {
      await this.advancedOptionsToggle.click();
      await collapseContent.waitFor({ state: 'visible' });
    }
  }

  async setChallengeOverride(enabled: boolean): Promise<void> {
    await this.openAdvancedOptions();
    if (enabled) {
      await this.overrideChallengeCheckbox.check();
    } else {
      await this.overrideChallengeCheckbox.uncheck();
    }
  }

  async submitForm(): Promise<void> {
    await this.submitButton.click();
  }

  /**
   * Create a game with default settings. Returns the game URL after redirect.
   */
  async createGame(
    gameType: GameType,
    opponents: TestUser[],
    options?: { publicGame?: boolean; challengeOverride?: boolean },
  ): Promise<string> {
    await this.selectGameType(gameType);
    await this.addPlayers(opponents);

    if (options?.publicGame !== undefined) {
      await this.setPublicGame(options.publicGame);
    }

    if (options?.challengeOverride !== undefined) {
      await this.setChallengeOverride(options.challengeOverride);
    }

    await this.submitForm();

    // Wait for redirect to the Play page
    await this.page.waitForURL(/\/Scrabble\/Play\//);
    return this.page.url();
  }
}
