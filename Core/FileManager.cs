using IFN584_ASS2.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IFN584_ASS2.Core
{
    public static class FileManager
    {
        private const string SaveFilePath = "savegame.json";

        public static void Save(GameTemplate game)
        {
            var json = JsonSerializer.Serialize(game, game.GetType(), new JsonSerializerOptions
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

            // For now, assume the game being loaded is Numerical Tic-Tac-Toe
            return JsonSerializer.Deserialize<NumericalTicTacToeGame>(json, new JsonSerializerOptions
            {
                IncludeFields = true
            });
        }
    }
}
