using IFN584_ASS2.Core;
using IFN584_ASS2.Games;
using IFN584_ASS2.UserUI;

namespace IFN584_ASS2
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("🎮 Welcome to the Multi-Board Game Framework!");
            Console.Write("Do you want to load a saved game? (y/n): ");
            var loadInput = Console.ReadLine()?.Trim().ToLower();

            if (loadInput == "y")
            {
                var loadedGame = FileManager.Load();
                if (loadedGame != null)
                {
                    loadedGame.Play();
                    return;
                }

                Console.WriteLine("⚠️ No valid saved game found. Starting new game.");
            }

            Console.WriteLine("\nChoose a game to play:");
            Console.WriteLine("1. Numerical Tic-Tac-Toe");
            Console.WriteLine("2. Notakto");
            Console.WriteLine("3. Gomoku");

            string? choice = Console.ReadLine()?.Trim();

            GameTemplate game = choice switch
            {
                "1" => new NumericalTicTacToeGame(),
                "2" => new NotaktoGame(),
                "3" => new GomokuGame(),
                _ => throw new Exception("❌ Invalid game choice.")
            };

            game.Play();
        }
    }
}
