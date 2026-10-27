using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using MX.TalkWithTiles.Contracts.Models;
using MX.TalkWithTiles.Contracts.StateModels;

namespace MX.TalkWithTiles.Repository;

internal static class GameJson
{
    internal static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(typeInfo =>
        {
            if (typeInfo.Type == typeof(GameStateModel))
            {
                foreach (var property in typeInfo.Properties.Where(p =>
                    p.Name is nameof(GameStateModel.GameEtag) or nameof(GameStateModel.LastUpdated)).ToArray())
                {
                    typeInfo.Properties.Remove(property);
                }
            }
        });
        var options = new JsonSerializerOptions { TypeInfoResolver = resolver };
        options.Converters.Add(new BoardConverter());
        return options;
    }

    private sealed class BoardConverter : JsonConverter<Tile[,]>
    {
        public override Tile[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var rows = JsonSerializer.Deserialize<Tile?[][]>(ref reader, options)
                ?? throw new JsonException("Missing board.");
            var height = rows.Length == 0 ? 0 : rows[0].Length;
            var board = new Tile[rows.Length, height];
            for (var x = 0; x < rows.Length; x++)
            {
                if (rows[x].Length != height)
                {
                    throw new JsonException("Invalid board dimensions.");
                }

                for (var y = 0; y < height; y++)
                {
                    board[x, y] = rows[x][y]!;
                }
            }

            return board;
        }

        public override void Write(Utf8JsonWriter writer, Tile[,] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            for (var x = 0; x < value.GetLength(0); x++)
            {
                writer.WriteStartArray();
                for (var y = 0; y < value.GetLength(1); y++)
                {
                    JsonSerializer.Serialize(writer, value[x, y], options);
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}
