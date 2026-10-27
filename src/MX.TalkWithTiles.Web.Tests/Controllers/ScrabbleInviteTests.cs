using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MX.TalkWithTiles.Contracts.Interfaces;
using MX.TalkWithTiles.Contracts.StateModels;
using MX.TalkWithTiles.Repository.Dtos;
using MX.TalkWithTiles.Repository.Interfaces;
using MX.TalkWithTiles.Web.Controllers;
using MX.TalkWithTiles.Web.Tests.Persistence;

namespace MX.TalkWithTiles.Web.Tests.Controllers;

public class ScrabbleInviteTests
{
    [Fact]
    public async Task AcceptInvite_PlayerNameWithoutOwnedInviteDoesNotAuthorizeJoining()
    {
        var state = PostgresPersistenceTests.NewGame();
        state.PlayersStateModel.Players.Add(new PlayerStateModel
        {
            PlayerId = Guid.NewGuid(), PlayerName = "test@example.invalid"
        });
        var games = new Mock<IGameStateRepository>();
        games.Setup(g => g.GetGameState(state.GameId, false)).ReturnsAsync(state);
        var invites = new Mock<IGameInviteRepository>();
        invites.Setup(i => i.GetGameInvites("test@example.invalid")).ReturnsAsync([]);
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "test@example.invalid"),
                new Claim(ClaimTypes.Name, "Test")
            ], "test"))
        };
        var controller = new ScrabbleController(NullLogger<ScrabbleController>.Instance, games.Object,
            Mock.Of<IContactsRepository>(), invites.Object, Mock.Of<IGameEngineFactory>(),
            new HttpContextAccessor { HttpContext = context })
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
        Assert.IsType<NotFoundResult>(await controller.AcceptInvite(state.GameId));
        games.Verify(g => g.UpdateGameState(It.IsAny<Guid>(), It.IsAny<GameStateModel>()), Times.Never);
    }
}
