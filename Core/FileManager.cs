using IFN584_ASS2.Games;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IFN584_ASS2.Core
{
    public static class FileManager
    {
        private const string SaveFilePath = "savegame.json";

        public static void Save(GameTemplate game)
        {
            var wrapper = new SaveWrapper
            {
                GameType = game.GetType().Name, // e.g. NotaktoGame
                GameData = JsonSerializer.SerializeToElement(game, game.GetType(), new JsonSerializerOptions
                {
                    IncludeFields = true,
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                })
            };

            var json = JsonSerializer.Serialize(wrapper, new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            });

            File.WriteAllText(SaveFilePath, json);
            Console.WriteLine("💾 Game saved successfully.");
        }

        public static GameTemplate? Load()
        {
            if (!File.Exists(SaveFilePath))
            {
                Console.WriteLine("⚠️ No save file found.");
                return null;
            }

            string json = File.ReadAllText(SaveFilePath);

            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var wrapper = JsonSerializer.Deserialize<SaveWrapper>(json, options);

            if (wrapper == null || string.IsNullOrEmpty(wrapper.GameType))
            {
                Console.WriteLine("⚠️ Failed to load saved game metadata.");
                return null;
            }

            return wrapper.GameType switch
            {
                nameof(NumericalTicTacToeGame) => JsonSerializer.Deserialize<NumericalTicTacToeGame>(wrapper.GameData.GetRawText(), options),
                nameof(NotaktoGame) => JsonSerializer.Deserialize<NotaktoGame>(wrapper.GameData.GetRawText(), options),
                nameof(GomokuGame) => JsonSerializer.Deserialize<GomokuGame>(wrapper.GameData.GetRawText(), options),
                _ => null
            };
        }


        private class SaveWrapper
        {
            public string GameType { get; set; } = string.Empty;
            public JsonElement GameData { get; set; }
        }
    }
}
