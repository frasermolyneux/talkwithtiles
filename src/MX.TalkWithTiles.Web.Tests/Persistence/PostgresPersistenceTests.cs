using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.Repository;
using MX.TalkWithTiles.Repository.Dtos;
using MX.TalkWithTiles.Repository.Models;
using Npgsql;

namespace MX.TalkWithTiles.Web.Tests.Persistence;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_POSTGRES_PASSWORD")))
        {
            Skip = "Set TEST_POSTGRES_PASSWORD to run against a disposable PostgreSQL 18 database.";
        }
    }
}

public class PostgresPersistenceTests
{
    internal static NpgsqlDataSource OpenDataSource() => NpgsqlDataSource.Create(new NpgsqlConnectionStringBuilder
    {
        Host = Environment.GetEnvironmentVariable("TEST_POSTGRES_HOST") ?? "127.0.0.1",
        Port = int.Parse(Environment.GetEnvironmentVariable("TEST_POSTGRES_PORT") ?? "5432",
            System.Globalization.CultureInfo.InvariantCulture),
        Database = "talkwithtiles",
        Username = "talkwithtiles",
        Password = Environment.GetEnvironmentVariable("TEST_POSTGRES_PASSWORD"),
        Timeout = 10,
        SslMode = SslMode.Disable,
        GssEncryptionMode = GssEncryptionMode.Disable,
        IncludeErrorDetail = false
    }.ConnectionString);

    [PostgresFact]
    public async Task Save_RestartPreservesBoardBagRacksIndexesAndInvites()
    {
        var state = NewGame();
        var inviteId = Guid.NewGuid();
        state.PlayersStateModel.Players.Add(new PlayerStateModel { PlayerId = inviteId, PlayerName = "friend@example.invalid" });
        var invites = new[] { new GameInviteDto { InviteId = inviteId, Email = "FRIEND@EXAMPLE.INVALID", GameId = state.GameId } };
        await using (var source = OpenDataSource())
        {
            await new AppDataRepository(source).Initialize();
            await new GameStateRepository(source).CreateGame(state, invites);
            Assert.NotNull(state.BagStateModel.Tiles);
            Assert.NotNull(state.BoardStateModel.Tiles);
        }

        await using var restartedSource = OpenDataSource();
        var repository = new GameStateRepository(restartedSource);
        var loaded = Assert.IsType<GameStateModel>(await repository.GetGameState(state.GameId));
        Assert.Equal(state.BoardStateModel.Tiles![1, 2].TileId, loaded.BoardStateModel.Tiles![1, 2].TileId);
        Assert.Null(loaded.BoardStateModel.Tiles[0, 0]);
        Assert.Equal(state.BagStateModel.Tiles![0].TileId, loaded.BagStateModel.Tiles![0].TileId);
        Assert.Equal(state.PlayersStateModel.Players[0].Tiles[0].TileId, loaded.PlayersStateModel.Players[0].Tiles[0].TileId);
        Assert.Contains(await new GameInviteRepository(restartedSource).GetGameInvites("friend@example.invalid"),
            invite => invite.InviteId == inviteId && invite.GameId == state.GameId);
        Assert.Contains(await repository.GetGameStates(new GameStateFilterModel
        {
            PlayerId = state.PlayersStateModel.Players[0].PlayerId,
            GamePrivacyFilter = GamePrivacyType.All
        }), g => g.GameId == state.GameId);
        var contacts = await new ContactsRepository(restartedSource).GetContacts(new ContactsFilterModel
        {
            UserId = state.PlayersStateModel.Players[0].PlayerId
        });
        Assert.Contains(contacts, c => c.Id == inviteId);
        Assert.True((await new AppDataRepository(restartedSource).HealthCheck()).Item1);
    }

    [PostgresFact]
    public async Task Save_ConflictingWriterRollsBackAndIdenticalRetryIsIdempotent()
    {
        await using var source = OpenDataSource();
        await new AppDataRepository(source).Initialize();
        var repository = new GameStateRepository(source);
        var state = NewGame();
        await repository.CreateGame(state, []);
        var first = (await repository.GetGameState(state.GameId))!;
        var stale = (await repository.GetGameState(state.GameId))!;
        first.PlayersStateModel.Players[0].Score = 42;
        await repository.UpdateGameState(state.GameId, first);
        var version = first.GameEtag;
        await repository.UpdateGameState(state.GameId, first);
        Assert.Equal(version, first.GameEtag);
        stale.PlayersStateModel.Players[0].Score = 99;
        stale.BagStateModel.Tiles!.Clear();
        stale.PlayersStateModel.Players.Add(new PlayerStateModel { PlayerId = Guid.NewGuid(), PlayerName = "intruder" });
        await Assert.ThrowsAsync<GameConcurrencyException>(() => repository.UpdateGameState(state.GameId, stale));
        var actual = (await repository.GetGameState(state.GameId))!;
        Assert.Equal(42, actual.PlayersStateModel.Players[0].Score);
        Assert.Single(actual.PlayersStateModel.Players);
        Assert.Single(actual.BagStateModel.Tiles!);
        Assert.Empty(await repository.GetGameStates(new GameStateFilterModel
        {
            PlayerId = stale.PlayersStateModel.Players[1].PlayerId,
            GamePrivacyFilter = GamePrivacyType.All
        }));
    }

    [PostgresFact]
    public async Task Save_AcceptingInviteRemovesOldIdentityAtomicallyAndHiddenGamesStayHidden()
    {
        await using var source = OpenDataSource();
        await new AppDataRepository(source).Initialize();
        var repository = new GameStateRepository(source);
        var state = NewGame();
        var inviteId = Guid.NewGuid();
        var email = $"{inviteId}@example.invalid";
        state.PlayersStateModel.Players.Add(new PlayerStateModel { PlayerId = inviteId, PlayerName = email });
        await repository.CreateGame(state, [new GameInviteDto { InviteId = inviteId, Email = email, GameId = state.GameId }]);
        var newId = Guid.NewGuid();
        state.PlayersStateModel.Players[1].PlayerId = newId;
        state.PlayersStateModel.Players[1].PlayerName = "Accepted";
        await repository.UpdateGameState(state.GameId, state);
        Assert.Empty(await new GameInviteRepository(source).GetGameInvites(email));
        Assert.Empty(await repository.GetGameStates(new GameStateFilterModel { PlayerId = inviteId, GamePrivacyFilter = GamePrivacyType.All }));
        await repository.DeleteGameStateIndex(state.GameId, newId);
        state.PlayersStateModel.Players[0].Score++;
        await repository.UpdateGameState(state.GameId, state);
        Assert.Empty(await repository.GetGameStates(new GameStateFilterModel { PlayerId = newId, GamePrivacyFilter = GamePrivacyType.All }));
        Assert.NotNull((await repository.GetGameState(state.GameId, true))!.BoardStateModel.Tiles);
    }

    [PostgresFact]
    public async Task ContactsAndInvites_AreScopedAndDeletionIsIdempotent()
    {
        await using var source = OpenDataSource();
        await new AppDataRepository(source).Initialize();
        var contacts = new ContactsRepository(source);
        var owner = Guid.NewGuid();
        var other = Guid.NewGuid();
        var contact = Guid.NewGuid();
        await contacts.UpdateContact(owner, contact, "Friend");
        await contacts.DeleteContact(other, contact);
        Assert.Single(await contacts.GetContacts(new ContactsFilterModel { UserId = owner }));
        Assert.Empty(await contacts.GetContacts(new ContactsFilterModel { UserId = other }));
        await contacts.DeleteContact(owner, contact);
        await contacts.DeleteContact(owner, contact);
        var state = NewGame();
        await new GameStateRepository(source).CreateGame(state, []);
        var invites = new GameInviteRepository(source);
        var inviteId = Guid.NewGuid();
        var email = $"{inviteId}@example.invalid";
        await invites.UpdateGameInvite(inviteId, $" {email.ToUpperInvariant()} ", state.GameId);
        await invites.DeleteGameInvite(inviteId, "someone-else@example.invalid");
        Assert.Single(await invites.GetGameInvites(email));
        await invites.DeleteGameInvite(inviteId, email);
        await invites.DeleteGameInvite(inviteId, email);
    }

    [PostgresFact]
    public async Task Save_DeletedContactsStayDeletedWhenExistingPlayersMoveOrAnotherPlayerJoins()
    {
        await using var source = OpenDataSource();
        await new AppDataRepository(source).Initialize();
        var repository = new GameStateRepository(source);
        var contacts = new ContactsRepository(source);
        var state = NewGame();
        var owner = state.PlayersStateModel.Players[0].PlayerId;
        var friend = Guid.NewGuid();
        state.PlayersStateModel.Players.Add(new PlayerStateModel { PlayerId = friend, PlayerName = "Friend" });
        await repository.CreateGame(state, []);
        Assert.Single(await contacts.GetContacts(new ContactsFilterModel { UserId = owner }));

        await contacts.DeleteContact(owner, friend);
        state.PlayersStateModel.Players[0].Score++;
        await repository.UpdateGameState(state.GameId, state);
        Assert.Empty(await contacts.GetContacts(new ContactsFilterModel { UserId = owner }));

        var newcomer = Guid.NewGuid();
        state.PlayersStateModel.Players.Add(new PlayerStateModel { PlayerId = newcomer, PlayerName = "Newcomer" });
        await repository.UpdateGameState(state.GameId, state);
        Assert.Equal(newcomer, Assert.Single(await contacts.GetContacts(new ContactsFilterModel { UserId = owner })).Id);
        Assert.Contains(await contacts.GetContacts(new ContactsFilterModel { UserId = newcomer }), c => c.Id == owner);
    }

    [PostgresFact]
    public async Task Create_ConcurrentGamesWithReversedPlayersDoNotDeadlockOnContacts()
    {
        await using var source = OpenDataSource();
        await new AppDataRepository(source).Initialize();
        var repository = new GameStateRepository(source);
        var first = NewGame();
        first.PlayersStateModel.Players.Add(new PlayerStateModel { PlayerId = Guid.NewGuid(), PlayerName = "Friend" });
        var second = NewGame();
        second.PlayersStateModel.Players = [.. first.PlayersStateModel.Players.AsEnumerable().Reverse()];
        var function = $"delay_contacts_{Guid.NewGuid():N}";
        await using var command = source.CreateCommand($"""
            CREATE FUNCTION {function}() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
                IF NEW.user_id IN ('{first.PlayersStateModel.Players[0].PlayerId}', '{first.PlayersStateModel.Players[1].PlayerId}') THEN
                    PERFORM pg_sleep(0.5);
                END IF;
                RETURN NEW;
            END $$;
            CREATE TRIGGER {function} AFTER INSERT ON contacts
                FOR EACH ROW EXECUTE FUNCTION {function}();
            """);
        await command.ExecuteNonQueryAsync();
        try
        {
            // Hold the first contact lock long enough for an opposite-order writer to overlap.
            await Task.WhenAll(repository.CreateGame(first, []), repository.CreateGame(second, []));
            Assert.NotNull(await repository.GetGameState(first.GameId));
            Assert.NotNull(await repository.GetGameState(second.GameId));
        }
        finally
        {
            command.CommandText = $"DROP TRIGGER {function} ON contacts; DROP FUNCTION {function}()";
            await command.ExecuteNonQueryAsync();
        }
    }

    [PostgresFact]
    public async Task Initialize_UnsupportedSchemaFailsClosed()
    {
        await using var source = OpenDataSource();
        var repository = new AppDataRepository(source);
        await repository.Initialize();
        await using var command = source.CreateCommand("INSERT INTO schema_version VALUES (99)");
        await command.ExecuteNonQueryAsync();
        try
        {
            await Assert.ThrowsAsync<InvalidOperationException>(repository.Initialize);
            Assert.False((await repository.HealthCheck()).Item1);
        }
        finally
        {
            command.CommandText = "DELETE FROM schema_version WHERE version = 99";
            await command.ExecuteNonQueryAsync();
        }
    }

    internal static GameStateModel NewGame()
    {
        var board = new Tile[3, 4];
        board[1, 2] = new Tile { TileId = Guid.NewGuid(), PosX = 1, PosY = 2, Letter = "A" };
        return new GameStateModel
        {
            GameId = Guid.NewGuid(),
            GamePrivacyType = GamePrivacyType.Private,
            BoardStateModel = new BoardStateModel { Tiles = board },
            BagStateModel = new BagStateModel { Tiles = [new Tile { TileId = Guid.NewGuid(), Letter = "B" }] },
            PlayersStateModel = new PlayersStateModel
            {
                Players = [new PlayerStateModel
                {
                    PlayerId = Guid.NewGuid(), PlayerName = "Owner",
                    Tiles = [new Tile { TileId = Guid.NewGuid(), Letter = "C" }]
                }]
            },
            EndGameStateModel = new EndGameStateModel(),
            ChallengeStateModel = new ChallengeStateModel(),
            PlayerMoveStateModel = new PlayerMoveStateModel()
        };
    }
}
