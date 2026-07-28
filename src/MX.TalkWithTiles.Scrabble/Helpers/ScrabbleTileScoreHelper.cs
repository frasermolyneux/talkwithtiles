using System.Collections.Generic;

namespace MX.TalkWithTiles.Scrabble.Helpers;

public static class ScrabbleTileScoreHelper
{
    private static readonly Dictionary<string, int> ScoreMapping = new()
    {
        {"_", 0},

        // One point
        {"E", 1}, {"A", 1}, {"I", 1}, {"O", 1},
        {"R", 1}, {"T", 1}, {"S", 1}, {"L", 1},
        {"N", 1}, {"U", 1},

        // Two points
        {"D", 2}, {"G", 2},

        // Three points
        {"B", 3}, {"C", 3}, {"M", 3}, {"P", 3},

        // Four points
        {"F", 4}, {"W", 4}, {"H", 4}, {"V", 4}, {"Y", 4},

        // Five points
        {"K", 5},

        // Eight points
        {"X", 8}, {"J", 8},

        // Ten points
        {"Q", 10}, {"Z", 10}
    };

    public static int GetTileScore(string letter)
    {
        return ScoreMapping.GetValueOrDefault(letter, 0);
    }
}
