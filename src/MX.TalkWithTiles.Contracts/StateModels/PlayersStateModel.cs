using System.Collections.Generic;

namespace MX.TalkWithTiles.Contracts.StateModels;

public class PlayersStateModel
{
    public List<PlayerStateModel> Players { get; set; } = [];
}
