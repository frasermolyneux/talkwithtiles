using System.Collections.Frozen;
using System.Collections.Generic;
using MX.TalkWithTiles.Contracts.Constants;
using MX.TalkWithTiles.Contracts.Models;

namespace MX.TalkWithTiles.Scrabble.Constants;

public static class ScrabbleBoardTiles
{
    public static readonly FrozenDictionary<GameType, IReadOnlyList<Tile>> Tiles = new Dictionary<GameType, List<Tile>>
    {
            {
                GameType.MiniBoard, new List<Tile>
                {
                    new Tile {PosX = 0, PosY = 0, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 0, PosY = 8, TileType = TileType.TripleWordScoreTile},

                    new Tile {PosX = 1, PosY = 1, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 1, PosY = 4, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 1, PosY = 7, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 2, PosY = 2, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 2, PosY = 6, TileType = TileType.TripleLetterScoreTile},

                    new Tile {PosX = 3, PosY = 3, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 3, PosY = 5, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 4, PosY = 1, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 4, PosY = 4, TileType = TileType.CentreTile},
                    new Tile {PosX = 4, PosY = 7, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 5, PosY = 3, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 5, PosY = 5, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 6, PosY = 2, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 6, PosY = 6, TileType = TileType.TripleLetterScoreTile},

                    new Tile {PosX = 7, PosY = 1, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 7, PosY = 4, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 7, PosY = 7, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 8, PosY = 0, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 8, PosY = 8, TileType = TileType.TripleWordScoreTile}
                }
            },
            {
                GameType.StandardBoard, new List<Tile>
                {
                    new Tile {PosX = 0, PosY = 0, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 0, PosY = 3, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 0, PosY = 7, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 0, PosY = 11, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 0, PosY = 14, TileType = TileType.TripleWordScoreTile},

                    new Tile {PosX = 1, PosY = 1, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 1, PosY = 5, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 1, PosY = 9, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 1, PosY = 13, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 2, PosY = 2, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 2, PosY = 6, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 2, PosY = 8, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 2, PosY = 12, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 3, PosY = 0, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 3, PosY = 3, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 3, PosY = 7, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 3, PosY = 11, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 3, PosY = 14, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 4, PosY = 4, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 4, PosY = 10, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 5, PosY = 1, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 5, PosY = 5, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 5, PosY = 9, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 5, PosY = 13, TileType = TileType.TripleLetterScoreTile},

                    new Tile {PosX = 6, PosY = 2, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 6, PosY = 6, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 6, PosY = 8, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 6, PosY = 12, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 7, PosY = 0, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 7, PosY = 3, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 7, PosY = 7, TileType = TileType.CentreTile},
                    new Tile {PosX = 7, PosY = 11, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 7, PosY = 14, TileType = TileType.TripleWordScoreTile},

                    new Tile {PosX = 8, PosY = 2, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 8, PosY = 6, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 8, PosY = 8, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 8, PosY = 12, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 9, PosY = 1, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 9, PosY = 5, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 9, PosY = 9, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 9, PosY = 13, TileType = TileType.TripleLetterScoreTile},

                    new Tile {PosX = 10, PosY = 4, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 10, PosY = 10, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 11, PosY = 0, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 11, PosY = 3, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 11, PosY = 7, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 11, PosY = 11, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 11, PosY = 14, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 12, PosY = 2, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 12, PosY = 6, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 12, PosY = 8, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 12, PosY = 12, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 13, PosY = 1, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 13, PosY = 5, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 13, PosY = 9, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 13, PosY = 13, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 14, PosY = 0, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 14, PosY = 3, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 14, PosY = 7, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 14, PosY = 11, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 14, PosY = 14, TileType = TileType.TripleWordScoreTile}
                }
            },
            {
                GameType.SuperSizeBoard, new List<Tile>
                {
                    new Tile {PosX = 0, PosY = 0, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 0, PosY = 4, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 0, PosY = 9, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 0, PosY = 14, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 0, PosY = 18, TileType = TileType.TripleWordScoreTile},

                    new Tile {PosX = 1, PosY = 1, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 1, PosY = 5, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 1, PosY = 8, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 1, PosY = 10, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 1, PosY = 13, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 1, PosY = 17, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 2, PosY = 2, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 2, PosY = 6, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 2, PosY = 12, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 2, PosY = 16, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 3, PosY = 3, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 3, PosY = 7, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 3, PosY = 11, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 3, PosY = 15, TileType = TileType.TripleWordScoreTile},

                    new Tile {PosX = 4, PosY = 0, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 4, PosY = 4, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 4, PosY = 8, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 4, PosY = 10, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 4, PosY = 14, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 4, PosY = 18, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 5, PosY = 1, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 5, PosY = 5, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 5, PosY = 9, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 5, PosY = 13, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 5, PosY = 17, TileType = TileType.TripleLetterScoreTile},

                    new Tile {PosX = 6, PosY = 2, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 6, PosY = 6, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 6, PosY = 12, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 6, PosY = 16, TileType = TileType.TripleLetterScoreTile},

                    new Tile {PosX = 7, PosY = 3, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 7, PosY = 7, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 7, PosY = 11, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 7, PosY = 15, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 8, PosY = 1, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 8, PosY = 4, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 8, PosY = 8, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 8, PosY = 10, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 8, PosY = 14, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 8, PosY = 17, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 9, PosY = 0, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 9, PosY = 5, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 9, PosY = 9, TileType = TileType.CentreTile},
                    new Tile {PosX = 9, PosY = 13, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 9, PosY = 18, TileType = TileType.TripleWordScoreTile},

                    new Tile {PosX = 10, PosY = 1, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 10, PosY = 4, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 10, PosY = 8, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 10, PosY = 10, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 10, PosY = 14, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 10, PosY = 17, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 11, PosY = 3, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 11, PosY = 7, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 11, PosY = 11, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 11, PosY = 15, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 12, PosY = 2, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 12, PosY = 6, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 12, PosY = 12, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 12, PosY = 16, TileType = TileType.TripleLetterScoreTile},

                    new Tile {PosX = 13, PosY = 1, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 13, PosY = 5, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 13, PosY = 9, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 13, PosY = 13, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 13, PosY = 17, TileType = TileType.TripleLetterScoreTile},

                    new Tile {PosX = 14, PosY = 0, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 14, PosY = 4, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 14, PosY = 8, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 14, PosY = 10, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 14, PosY = 14, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 14, PosY = 18, TileType = TileType.DoubleLetterScoreTile},

                    new Tile {PosX = 15, PosY = 3, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 15, PosY = 7, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 15, PosY = 11, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 15, PosY = 15, TileType = TileType.TripleWordScoreTile},

                    new Tile {PosX = 16, PosY = 2, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 16, PosY = 6, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 16, PosY = 12, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 16, PosY = 16, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 17, PosY = 1, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 17, PosY = 5, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 17, PosY = 8, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 17, PosY = 10, TileType = TileType.DoubleWordScoreTile},
                    new Tile {PosX = 17, PosY = 13, TileType = TileType.TripleLetterScoreTile},
                    new Tile {PosX = 17, PosY = 17, TileType = TileType.DoubleWordScoreTile},

                    new Tile {PosX = 18, PosY = 0, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 18, PosY = 4, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 18, PosY = 9, TileType = TileType.TripleWordScoreTile},
                    new Tile {PosX = 18, PosY = 14, TileType = TileType.DoubleLetterScoreTile},
                    new Tile {PosX = 18, PosY = 18, TileType = TileType.TripleWordScoreTile}
                }
            }
        }.ToFrozenDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<Tile>)kvp.Value);
}
