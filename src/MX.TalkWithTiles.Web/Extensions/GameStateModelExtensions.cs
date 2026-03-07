using System.Security.Claims;
using System.Text;
using MX.TalkWithTiles.Common.Extensions;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Web.Extensions;

public static class GameStateModelExtensions
{
        public static bool IsUserInGame(this GameStateModel model, ClaimsPrincipal user)
        {
            return model.PlayersStateModel.Players.Any(p => p.PlayerId == user.GetUserGuid());
        }

        public static bool IsGameInChallenge(this GameStateModel model)
        {
            return model.ChallengeStateModel.ChallengeReason != null;
        }

        public static bool IsChallengedPlayer(this GameStateModel model, ClaimsPrincipal user)
        {
            return model.ChallengeStateModel.ChallengedPlayerId == user.GetUserGuid();
        }

        public static bool IsChallengerPlayer(this GameStateModel model, ClaimsPrincipal user)
        {
            return model.ChallengeStateModel.ChallengerPlayerId == user.GetUserGuid();
        }

        public static string LastPlayerName(this GameStateModel model)
        {
            var playerId =
                model.PlayerMoveStateModel.PlayerOrderIds.PreviousTo(model.PlayerMoveStateModel.CurrentPlayerId);
            var player = model.PlayersStateModel.Players.SingleOrDefault(p => p.PlayerId == playerId);

            return player?.PlayerName ?? "Unknown";
        }

        public static bool IsCurrentPlayer(this GameStateModel model, ClaimsPrincipal user)
        {
            return model.PlayerMoveStateModel.CurrentPlayerId == user.GetUserGuid();
        }

        public static bool IsLastPlayer(this GameStateModel model, ClaimsPrincipal user)
        {
            var playerId =
                model.PlayerMoveStateModel.PlayerOrderIds.PreviousTo(model.PlayerMoveStateModel.CurrentPlayerId);

            return playerId == user.GetUserGuid();
        }

        public static List<Tile> UserTiles(this GameStateModel model, ClaimsPrincipal user)
        {
            if (!model.IsUserInGame(user))
                return [];

            var player = model.PlayersStateModel.Players.Single(p => p.PlayerId == user.GetUserGuid());

            return player.Tiles;
        }

        public static bool IsGameInProgress(this GameStateModel model)
        {
            return model.EndGameStateModel.GameStateType == GameStateType.InProgress;
        }

        public static bool CanIssueChallenge(this GameStateModel model, ClaimsPrincipal user)
        {
            if (!model.IsUserInGame(user))
                return false;

            return !model.IsGameInChallenge() && !model.IsLastPlayer(user) &&
                   model.PlayerMoveStateModel.LastMoveType == LastMoveType.Normal;
        }

        public static bool ShowRemainingTiles(this GameStateModel model, ClaimsPrincipal user)
        {
            if (!model.IsUserInGame(user))
                return false;

            return model.TileBagVisibilityOption switch
            {
                TileBagVisibilityOption.ShowRemainingTiles => true,
                TileBagVisibilityOption.ShowAfterFirstTurns => model.PlayerMoveStateModel.TurnsTaken >=
                                                               model.PlayersStateModel.Players.Count,
                TileBagVisibilityOption.ShowNearEndGame => (model.BagStateModel.Tiles?.Count ?? 0) <=
                                                           model.PlayersStateModel.Players.Count * 7,
                TileBagVisibilityOption.OnlyShowRemainingCount => false,
                TileBagVisibilityOption.DoNotShowRemainingTiles => false,
                _ => false
            };
        }

        public static Guid CurrentPlayerId(this GameStateModel model)
        {
            return model.PlayerMoveStateModel.CurrentPlayerId;
        }

        public static bool IsGameCompleted(this GameStateModel model)
        {
            return model.EndGameStateModel.GameStateType == GameStateType.Completed;
        }

        public static bool IsSingleWinner(this GameStateModel model)
        {
            return model.EndGameStateModel.Winners.Count == 1;
        }

        public static string WinnerName(this GameStateModel model)
        {
            var winningIds = model.EndGameStateModel.Winners;
            var winningPlayers = model.PlayersStateModel.Players.Where(p => winningIds.Contains(p.PlayerId)).ToList();

            return string.Join(",", winningPlayers.Select(p => p.PlayerName));
        }

        public static int WinningPoints(this GameStateModel model)
        {
            return model.EndGameStateModel.WinnerPoints;
        }

        public static string ChallengerName(this GameStateModel model)
        {
            var playerId = model.ChallengeStateModel.ChallengerPlayerId;
            var player = model.PlayersStateModel.Players.SingleOrDefault(p => p.PlayerId == playerId);

            return player?.PlayerName ?? "Unknown";
        }

        public static string ChallengedName(this GameStateModel model)
        {
            var playerId = model.ChallengeStateModel.ChallengedPlayerId;
            var player = model.PlayersStateModel.Players.SingleOrDefault(p => p.PlayerId == playerId);

            return player?.PlayerName ?? "Unknown";
        }

        public static string LastMoveAndScore(this GameStateModel model)
        {
            if (model.PlayerMoveStateModel.LastMoveResult is not { } lastTurn)
                throw new InvalidOperationException("Cannot get last move score: no previous move recorded.");

            var sb = new StringBuilder();
            foreach (var wordAndPoint in lastTurn.WordsAndPoints)
                sb.Append($"{wordAndPoint.Word} ({wordAndPoint.Score}) ");

            sb.Append($"= {lastTurn.Points}");
            return sb.ToString();
        }

        public static GameChallengeReason ScrabbleChallengeReason(this GameStateModel model)
        {
            if (model.ChallengeStateModel.ChallengeReason != null)
                return (GameChallengeReason) model.ChallengeStateModel.ChallengeReason;
            return GameChallengeReason.Catchall;
        }

        public static GameChallengeResult ScrabbleChallengeResultForReason(this GameStateModel model,
            GameChallengeReason challengeReason)
        {
            return model.ChallengeStateModel.ChallengeResults[challengeReason];
        }

        public static List<PlayerStateModel> Players(this GameStateModel model)
        {
            return model.PlayersStateModel.Players
                .OrderBy(p => model.PlayerMoveStateModel.PlayerOrderIds.IndexOf(p.PlayerId)).ToList();
        }

        public static bool ShowRemainingTileCountOnly(this GameStateModel model)
        {
            return model.TileBagVisibilityOption == TileBagVisibilityOption.OnlyShowRemainingCount;
        }

        public static string NextPlayerName(this GameStateModel model)
        {
            var playerId = model.PlayerMoveStateModel.PlayerOrderIds.NextOf(model.PlayerMoveStateModel.CurrentPlayerId);
            var player = model.PlayersStateModel.Players.SingleOrDefault(p => p.PlayerId == playerId);

            return player?.PlayerName ?? "Unknown";
        }

        public static bool IsNextPlayer(this GameStateModel model, ClaimsPrincipal user)
        {
            var playerId = model.PlayerMoveStateModel.PlayerOrderIds.NextOf(model.PlayerMoveStateModel.CurrentPlayerId);
            return playerId == user.GetUserGuid();
        }

        public static string CurrentPlayerName(this GameStateModel model)
        {
            var playerId = model.PlayerMoveStateModel.CurrentPlayerId;
            var player = model.PlayersStateModel.Players.SingleOrDefault(p => p.PlayerId == playerId);

            return player?.PlayerName ?? "Unknown";
        }

        public static bool IsResolvedChallenged(this GameStateModel model, ClaimsPrincipal user)
        {
            if (model.ChallengeStateModel.PlayerChallengeResult == null)
                return false;

            return model.ChallengeStateModel.PlayerChallengeResult.ChallengedPlayerId == user.GetUserGuid();
        }

        public static bool IsResolvedChallenger(this GameStateModel model, ClaimsPrincipal user)
        {
            if (model.ChallengeStateModel.PlayerChallengeResult == null)
                return false;

            return model.ChallengeStateModel.PlayerChallengeResult.ChallengerPlayerId == user.GetUserGuid();
        }

        public static GameChallengeResult ResolvedChallengeResult(this GameStateModel model)
        {
            if (model.ChallengeStateModel.PlayerChallengeResult is not { } result)
                throw new InvalidOperationException("Cannot get resolved challenge result: no challenge result recorded.");

            return result.GameChallengeResult;
        }

        public static string ResolvedChallengerName(this GameStateModel model)
        {
            if (model.ChallengeStateModel.PlayerChallengeResult is not { } result)
                throw new InvalidOperationException("Cannot get resolved challenger name: no challenge result recorded.");

            var playerId = result.ChallengerPlayerId;
            var player = model.PlayersStateModel.Players.SingleOrDefault(p => p.PlayerId == playerId);

            return player?.PlayerName ?? "Unknown";
        }

        public static string ResolvedChallengedName(this GameStateModel model)
        {
            if (model.ChallengeStateModel.PlayerChallengeResult is not { } result)
                throw new InvalidOperationException("Cannot get resolved challenged name: no challenge result recorded.");

            var playerId = result.ChallengedPlayerId;
            var player = model.PlayersStateModel.Players.SingleOrDefault(p => p.PlayerId == playerId);

            return player?.PlayerName ?? "Unknown";
        }

        public static bool CanUndoLastTurn(this GameStateModel model, ClaimsPrincipal user)
        {
            return model.IsLastPlayer(user) && !model.IsGameInChallenge() &&
                   model.PlayerMoveStateModel.LastMoveType == LastMoveType.Normal;
        }

        public static bool IsTileInLastMove(this GameStateModel model, Tile tile)
        {
            if (model.PlayerMoveStateModel.LastMoveType != LastMoveType.Normal)
                return false;

            if (model.PlayerMoveStateModel.LastMoveResult == null)
                return false;

            return model.PlayerMoveStateModel.LastMoveResult.Tiles.Any(t =>
                t.Letter == tile.Letter && t.PosX == tile.PosX && t.PosY == tile.PosY);
        }
}