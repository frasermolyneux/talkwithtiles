using System;
using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Repository.Models;

public class GameStateFilterModel
{
    public enum OrderBy
    {
        UpdatedAsc,
        UpdatedDesc
    }

    public GamePrivacyType GamePrivacyFilter { get; set; } = GamePrivacyType.Public;
    public GameStateType StateFilter { get; set; } = GameStateType.InProgress;
    public OrderBy Order { get; set; } = OrderBy.UpdatedDesc;
    public Guid PlayerId { get; set; } = Guid.Empty;
    public bool SkipTileFetch { get; set; }
}