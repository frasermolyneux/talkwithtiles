using System.Collections.Frozen;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;

namespace MX.TalkWithTiles.Scrabble.Constants;

public static class ScrabbleStartingTiles
{
    public static readonly FrozenDictionary<GameType, IReadOnlyDictionary<string, int>> Tiles = new Dictionary<GameType, Dictionary<string, int>>
    {
                {
                    GameType.MiniBoard, new Dictionary<string, int>
                    {
                        {"A", 5}, {"B", 1}, {"C", 1}, {"D", 2}, {"E", 6},
                        {"F", 1}, {"G", 2}, {"H", 1}, {"I", 2}, {"J", 2},
                        {"K", 1}, {"L", 2}, {"M", 1}, {"N", 3}, {"O", 3},
                        {"P", 1}, {"Q", 1}, {"R", 3}, {"S", 2}, {"T", 1},
                        {"U", 2}, {"V", 1}, {"W", 1}, {"X", 1}, {"Y", 1},
                        {"Z", 1}, {"_", 1}
                    }
                },
                {
                    GameType.StandardBoard, new Dictionary<string, int>
                    {
                        {"A", 9}, {"B", 2}, {"C", 2}, {"D", 4}, {"E", 12},
                        {"F", 2}, {"G", 3}, {"H", 2}, {"I", 9}, {"J", 1},
                        {"K", 1}, {"L", 4}, {"M", 2}, {"N", 6}, {"O", 8},
                        {"P", 2}, {"Q", 1}, {"R", 6}, {"S", 4}, {"T", 6},
                        {"U", 4}, {"V", 2}, {"W", 2}, {"X", 1}, {"Y", 2},
                        {"Z", 1}, {"_", 2}
                    }
                },
                {
                    GameType.SuperSizeBoard, new Dictionary<string, int>
                    {
                        {"A", 18}, {"B", 4}, {"C", 4}, {"D", 8}, {"E", 24},
                        {"F", 4}, {"G", 6}, {"H", 4}, {"I", 18}, {"J", 2},
                        {"K", 2}, {"L", 8}, {"M", 4}, {"N", 12}, {"O", 16},
                        {"P", 4}, {"Q", 2}, {"R", 12}, {"S", 8}, {"T", 12},
                        {"U", 8}, {"V", 4}, {"W", 4}, {"X", 2}, {"Y", 4},
                        {"Z", 2}, {"_", 4}
                    }
                }
            }.ToFrozenDictionary(kvp => kvp.Key, kvp => (IReadOnlyDictionary<string, int>)kvp.Value);
}
