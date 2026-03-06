using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.Repository.Interfaces;
using MX.TalkWithTiles.Repository.Models;
using MX.TalkWithTiles.Web.Extensions;
using MX.TalkWithTiles.Web.Models;

namespace MX.TalkWithTiles.Web.Controllers;

[Authorize]
public class ScrabbleController(
    ILogger<ScrabbleController> logger,
    IGameStateRepository gameStateRepository,
    IContactsRepository contactsRepository,
    IGameEngineFactory gameEngineFactory,
    IHttpContextAccessor httpContextAccessor) : Controller
{
    private const string PlayerGameSessionModelKey = "PlayerGameSessionModel";

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var playerId = User.GetUserGuid();

            var gamesFilterModel = new GameStateFilterModel
            {
                GamePrivacyFilter = GamePrivacyType.All,
                Order = GameStateFilterModel.OrderBy.UpdatedDesc,
                PlayerId = playerId,
                SkipTileFetch = true
            };

            var gamesEntries = await gameStateRepository.GetGameStates(gamesFilterModel);

            return View(gamesEntries);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var createGameModel = new CreateScrabbleGameModel
            {
                ContactDtos =
                    await contactsRepository.GetContacts(new ContactsFilterModel {UserId = User.GetUserGuid()})
            };

            createGameModel.PlayerModels.Add(new CreatePlayerModel());

            return View(createGameModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateScrabbleGameModel createScrabbleGameModel)
        {
            if (!ModelState.IsValid)
            {
                createScrabbleGameModel.ContactDtos =
                    await contactsRepository.GetContacts(new ContactsFilterModel {UserId = User.GetUserGuid()});
                return View(createScrabbleGameModel);
            }

            var playerId = User.GetUserGuid();

            var privacyOption = GamePrivacyType.Private;
            if (createScrabbleGameModel.PublicGame)
                privacyOption = GamePrivacyType.Public;

            var challengeResults = new Dictionary<GameChallengeReason, GameChallengeResult>
            {
                {GameChallengeReason.Catchall, createScrabbleGameModel.CatchallGameChallengeResult.GetValueOrDefault()},
                {GameChallengeReason.ThatsNotAWord, createScrabbleGameModel.ThatsNotAWordGameChallengeResult.GetValueOrDefault()},
                {
                    GameChallengeReason.ThatsNotAValidTurn,
                    createScrabbleGameModel.ThatsNotAValidTurnGameChallengeResult.GetValueOrDefault()
                }
            };

            var gameEngine = gameEngineFactory.CreateNew(privacyOption, createScrabbleGameModel.GameType.GetValueOrDefault(),
                createScrabbleGameModel.TileBagVisibilityOption.GetValueOrDefault(), createScrabbleGameModel.CanOverrideChallengeOutcome,
                challengeResults);

            gameEngine.AddPlayer(playerId, User.GetUserName());

            foreach (var modelPlayerModel in createScrabbleGameModel.PlayerModels)
            {
                if (modelPlayerModel.Identifier.IsEmail())
                {
                    var inviteId = Guid.NewGuid();
                    gameEngine.AddPlayer(inviteId, modelPlayerModel.Identifier);
                }
                else
                {
                    ModelState.AddModelError(
                        $"PlayerModels[{createScrabbleGameModel.PlayerModels.IndexOf(modelPlayerModel)}].Identifier",
                        "Please provide a valid email address for your opponent.");
                }
            }

            if (!ModelState.IsValid)
            {
                createScrabbleGameModel.ContactDtos =
                    await contactsRepository.GetContacts(new ContactsFilterModel {UserId = User.GetUserGuid()});
                return View(createScrabbleGameModel);
            }

            gameEngine.SetRandomPlayerOrder();

            var gameStateModel = gameEngine.GameStateModel;
            await gameStateRepository.UpdateGameState(gameStateModel.GameId, gameStateModel);

            logger.LogInformation("User has created a new game of '{GameType}'", createScrabbleGameModel.GameType);

            // Update the contacts for all the players
            var players = gameStateModel.Players();
            var contactUpdates = players.SelectMany(
                player => players.Where(p => p.PlayerId != player.PlayerId),
                (player, other) => (player.PlayerId, other.PlayerId, other.PlayerName));

            foreach (var (sourcePlayerId, targetPlayerId, targetPlayerName) in contactUpdates)
            {
                await contactsRepository.UpdateContact(sourcePlayerId, targetPlayerId, targetPlayerName);
            }

            return RedirectToAction("Play", new {id = gameStateModel.GameId});
        }

        [HttpGet]
        public async Task<IActionResult> Play(Guid id, bool hideScreenClutter = false)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var playGameSessionModel =
                httpContextAccessor.HttpContext?.Session.GetObjectFromJson<PlayGameSessionModel>(
                    PlayerGameSessionModelKey)
                ?? new PlayGameSessionModel();

            if (playGameSessionModel.HideScreenClutter.ContainsKey(id))
                playGameSessionModel.HideScreenClutter[id] = hideScreenClutter;
            else
                playGameSessionModel.HideScreenClutter.Add(id, hideScreenClutter);

            httpContextAccessor.HttpContext?.Session.SetObjectAsJson(PlayerGameSessionModelKey, playGameSessionModel);
            ViewBag.HideScreenClutter = hideScreenClutter;

            var gameStateModel = await gameStateRepository.GetGameState(id);
            if (gameStateModel == null) return NotFound();

            if (!gameStateModel.IsUserInGame(User) && !User.IsAdmin()) return Unauthorized();

            GenerateStateOfPlayMessage(gameStateModel);

            return View(gameStateModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetPlayerMoveResult(Guid id, [FromBody] PlayerMove playerMove)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var gameStateModel = await gameStateRepository.GetGameState(id);

            if (gameStateModel == null)
                return NotFound();

            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();

            var gameEngine = gameEngineFactory.CreateFromStateModel(gameStateModel);

            var playerMoveResult = gameEngine.MakeMove(playerMove, true);
            return Json(playerMoveResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPlayerMove(Guid id, [FromBody] PlayerMove playerMove)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var gameStateModel = await gameStateRepository.GetGameState(id);

            if (gameStateModel == null)
                return NotFound();

            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();

            if (!gameStateModel.IsCurrentPlayer(User))
                return RedirectToAction("Play", new {id, hideScreenClutter = HideScreenClutterForGame(id)});

            var gameEngine = gameEngineFactory.CreateFromStateModel(gameStateModel);

            var playerMoveResult = gameEngine.MakeMove(playerMove, false);

            if (playerMoveResult.IsValid)
            {
                await gameStateRepository.UpdateGameState(id, gameEngine.GameStateModel);
            }

            return Json(playerMoveResult);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetGameEtag(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var gameStateModel = await gameStateRepository.GetGameState(id, true);

            if (gameStateModel == null)
                return NotFound();

            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();

            return Json(new
            {
                gameStateModel.GameEtag
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SkipTurn(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var gameStateModel = await gameStateRepository.GetGameState(id);

            if (gameStateModel == null)
                return NotFound();

            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();

            if (!gameStateModel.IsCurrentPlayer(User))
                return RedirectToAction("Play", new {id, hideScreenClutter = HideScreenClutterForGame(id)});

            var gameEngine = gameEngineFactory.CreateFromStateModel(gameStateModel);
            gameEngine.SkipMove(User.GetUserGuid());
            await gameStateRepository.UpdateGameState(id, gameEngine.GameStateModel);

            return RedirectToAction("Play", new {id, hideScreenClutter = HideScreenClutterForGame(id)});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UndoTurn(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var gameStateModel = await gameStateRepository.GetGameState(id);

            if (gameStateModel == null)
                return NotFound();

            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();

            if (!gameStateModel.IsLastPlayer(User))
                return RedirectToAction("Play", new {id, hideScreenClutter = HideScreenClutterForGame(id)});

            var gameEngine = gameEngineFactory.CreateFromStateModel(gameStateModel);
            gameEngine.UndoLastTurn(User.GetUserGuid());
            await gameStateRepository.UpdateGameState(id, gameEngine.GameStateModel);

            return RedirectToAction("Play", new {id, hideScreenClutter = HideScreenClutterForGame(id)});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExchangeTiles(ExchangeTilesModel exchangeTilesModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var gameStateModel = await gameStateRepository.GetGameState(exchangeTilesModel.Id.GetValueOrDefault());

            if (gameStateModel == null)
                return NotFound();

            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();

            if (!gameStateModel.IsCurrentPlayer(User))
                return RedirectToAction("Play",
                    new
                    {
                        id = exchangeTilesModel.Id.GetValueOrDefault(), hideScreenClutter = HideScreenClutterForGame(exchangeTilesModel.Id.GetValueOrDefault())
                    });

            var gameEngine = gameEngineFactory.CreateFromStateModel(gameStateModel);
            gameEngine.ExchangeTiles(User.GetUserGuid(),
                exchangeTilesModel.ExchangeTiles.Where(t => t.Exchange).Select(e => e.TileId));

            await gameStateRepository.UpdateGameState(exchangeTilesModel.Id.GetValueOrDefault(), gameEngine.GameStateModel);

            return RedirectToAction("Play",
                new {id = exchangeTilesModel.Id.GetValueOrDefault(), hideScreenClutter = HideScreenClutterForGame(exchangeTilesModel.Id.GetValueOrDefault())});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitChallenge(IssueChallengeModel issueChallengeModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var gameStateModel = await gameStateRepository.GetGameState(issueChallengeModel.GameId.GetValueOrDefault());

            if (gameStateModel == null)
                return NotFound();

            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();

            var gameEngine = gameEngineFactory.CreateFromStateModel(gameStateModel);

            gameEngine.IssuePlayerChallenge(User.GetUserGuid(), issueChallengeModel.GameChallengeReason.GetValueOrDefault(),
                issueChallengeModel.ChallengeText ?? string.Empty);

            await gameStateRepository.UpdateGameState(issueChallengeModel.GameId.GetValueOrDefault(), gameEngine.GameStateModel);

            return RedirectToAction("Play",
                new
                {
                    id = issueChallengeModel.GameId.GetValueOrDefault(),
                    hideScreenClutter = HideScreenClutterForGame(issueChallengeModel.GameId.GetValueOrDefault())
                });
        }

        [HttpGet]
        public async Task<IActionResult> Abandon(Guid id)
        {
            return await GetGameForConfirmation(id, "Abandon");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAbandon(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var gameStateModel = await gameStateRepository.GetGameState(id, true);

            if (gameStateModel == null)
                return NotFound();

            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();

            var gameEngine = gameEngineFactory.CreateFromStateModel(gameStateModel);
            gameEngine.AbandonGame(User.GetUserGuid());

            await gameStateRepository.UpdateGameState(id, gameEngine.GameStateModel);

            logger.LogInformation("User has abandoned the game '{GameId}'", id);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            return await GetGameForConfirmation(id, "Delete");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var gameStateModel = await gameStateRepository.GetGameState(id, true);

            if (gameStateModel == null)
                return NotFound();

            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();

            await gameStateRepository.DeleteGameStateIndex(id, User.GetUserGuid());

            logger.LogInformation("User has deleted the game '{GameId}'", id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveChallenge(ResolveChallengeModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var gameStateModel = await gameStateRepository.GetGameState(model.Id.GetValueOrDefault());

            if (gameStateModel == null)
                return NotFound();

            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();

            if (!gameStateModel.IsGameInChallenge())
                return RedirectToAction("Play", new { id = model.Id.GetValueOrDefault(), hideScreenClutter = HideScreenClutterForGame(model.Id.GetValueOrDefault()) });

            if (!gameStateModel.IsChallengedPlayer(User))
                return RedirectToAction("Play", new { id = model.Id.GetValueOrDefault(), hideScreenClutter = HideScreenClutterForGame(model.Id.GetValueOrDefault()) });

            var gameEngine = gameEngineFactory.CreateFromStateModel(gameStateModel);

            if (gameStateModel.ChallengeStateModel.CanOverrideChallengeOutcome)
                gameEngine.ResolveChallenge(model.Accept, model.GameChallengeResultOverride.GetValueOrDefault());
            else
                gameEngine.ResolveChallenge(model.Accept, null);

            await gameStateRepository.UpdateGameState(model.Id.GetValueOrDefault(), gameEngine.GameStateModel);

            if (model.Accept && gameEngine.GameStateModel.ChallengeStateModel.PlayerChallengeResult is { } challengeResult)
                switch (challengeResult.GameChallengeResult)
                {
                    case GameChallengeResult.RetryPlayerMove:
                        this.AddAlertSuccess("You have resolved the challenge and will now have to redo your go.");
                        break;
                    case GameChallengeResult.LosePointsAndProceedToNextPlayer:
                        this.AddAlertSuccess(
                            "You have resolved the challenge will lose your points. Play will proceed to the next player.");
                        break;
                    case GameChallengeResult.Nothing:
                        this.AddAlertSuccess(
                            "You have resolved the challenge and nothing will happen. Play will proceed to the next player.");
                        break;
                }
            else
                this.AddAlertSuccess(
                    "You have rejected the challenge and nothing will happen. Play will proceed to the next player.");


            return RedirectToAction("Play", new { id = model.Id.GetValueOrDefault(), hideScreenClutter = HideScreenClutterForGame(model.Id.GetValueOrDefault()) });
        }

        private void GenerateStateOfPlayMessage(GameStateModel model)
        {
            if (!model.IsGameInProgress())
                return;

            if (model.IsGameInChallenge())
            {
                this.AddAlertWarning(GenerateGameInChallengeMessage(model, User));
                return;
            }

            var message = GenerateStateOfPlayMessageText(model, User);
            var turnMessage = GenerateTurnMessage(model, User);
            this.AddAlertInfo(turnMessage + " " + message);
        }

        private static string GenerateTurnMessage(GameStateModel model, System.Security.Claims.ClaimsPrincipal user)
        {
            if (model.IsCurrentPlayer(user))
                return $"It's currently <strong>your</strong> turn and the next player will be <strong>{model.NextPlayerName()}</strong>.";

            return model.IsNextPlayer(user)
                ? $"It's <strong>{model.CurrentPlayerName()}'s</strong> turn and the next player will be <strong>you</strong>."
                : $"It's <strong>{model.CurrentPlayerName()}'s</strong> turn and the next player will be <strong>{model.NextPlayerName()}</strong>.";
        }

        private static string GenerateStateOfPlayMessageText(GameStateModel model, System.Security.Claims.ClaimsPrincipal user)
        {
            return model.PlayerMoveStateModel.LastMoveType switch
            {
                LastMoveType.Null => $"Welcome to the game <strong>{user.GetUserName()}!</strong>",
                LastMoveType.Normal => model.IsLastPlayer(user)
                    ? $"The last move was from <strong>you</strong> and was: {model.LastMoveAndScore()}"
                    : $"The last move was from <strong>{model.LastPlayerName()}</strong> and was: {model.LastMoveAndScore()}",
                LastMoveType.SkippedTurn => model.IsLastPlayer(user)
                    ? "The last move was from <strong>you</strong> where you skipped your turn."
                    : $"The last move was from <strong>{model.LastPlayerName()}</strong> and they skipped their turn.",
                LastMoveType.ExchangedTiles => model.IsLastPlayer(user)
                    ? "The last move was from <strong>you</strong> where you exchanged your tiles."
                    : $"The last move was from <strong>{model.LastPlayerName()}</strong> and they exchanged their tiles.",
                LastMoveType.UndoTurn => model.IsCurrentPlayer(user)
                    ? "The last move was from <strong>you</strong> where you withdrew your tiles. You can now have your turn again."
                    : $"The last move was from <strong>{model.CurrentPlayerName()}</strong> and they withdrew their tiles. They will now have their turn again.",
                _ => string.Empty
            };
        }

        private static string GenerateGameInChallengeMessage(GameStateModel model, System.Security.Claims.ClaimsPrincipal user)
        {
            if (model.IsChallengedPlayer(user))
                return $"<strong>{model.ChallengerName()}</strong> has challenged your last move of <strong>{model.LastMoveAndScore()}</strong> with '<strong>{model.ScrabbleChallengeReason()}</strong>'. " +
                       "You now need to accept/reject the challenge for the game to continue.";
            if (model.IsChallengerPlayer(user))
                return $"<strong>You</strong> have challenged <strong>{model.ChallengedName()}'s</strong> last move of <strong>{model.LastMoveAndScore()}</strong> with '<strong>{model.ScrabbleChallengeReason()}</strong>'. " +
                       $"<strong>{model.ChallengedName()}</strong> needs to accept/reject the challenge for the game to continue.";

            return $"<strong>{model.ChallengerName()}</strong> has challenged <strong>{model.ChallengedName()}'s</strong> last move of <strong>{model.LastMoveAndScore()}</strong> with '<strong>{model.ScrabbleChallengeReason()}</strong>'. " +
                   $"<strong>{model.ChallengedName()}</strong> needs to accept/reject the challenge for the game to continue.";
        }

        private async Task<IActionResult> GetGameForConfirmation(Guid id, string viewName)
        {
            var gameStateModel = await gameStateRepository.GetGameState(id, true);
            if (gameStateModel == null) return NotFound();
            if (!gameStateModel.IsUserInGame(User)) return Unauthorized();
            return View(viewName, gameStateModel);
        }

        private bool HideScreenClutterForGame(Guid id)
        {
            var playGameSessionModel =
                httpContextAccessor.HttpContext?.Session.GetObjectFromJson<PlayGameSessionModel>(
                    PlayerGameSessionModelKey);

            if (playGameSessionModel == null)
                return false;

            if (playGameSessionModel.HideScreenClutter.ContainsKey(id))
                return playGameSessionModel.HideScreenClutter[id];
            return false;
        }
    }