using IFN584_ASS2.Core;
using IFN584_ASS2.Enums;
using IFN584_ASS2.Games;
using IFN584_ASS2.UserUI;
using System;

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
                    //loadedGame.ShowBoard();  // ✅ call public wrapper method
                    loadedGame.Play();
                    return;
                }

                Console.WriteLine("⚠️ No valid saved game found. Starting a new game.");
            }

            GameTemplate game = null;

            while (true)
            {
                Console.WriteLine("\nChoose a game to play:");
                Console.WriteLine("1. Numerical Tic-Tac-Toe");
                Console.WriteLine("2. Notakto");
                Console.WriteLine("3. Gomoku");
                Console.Write("Enter your choice (1-3): ");
                string? choice = Console.ReadLine()?.Trim();

                if (choice == "1")
                {
                    GameMode selectedMode = PromptForMode("Numerical Tic-Tac-Toe");
                    game = new NumericalTicTacToeGame(selectedMode);
                    break;
                }
                else if (choice == "2")
                {
                    GameMode selectedMode = PromptForMode("Notakto");
                    game = new NotaktoGame(selectedMode);
                    break;
                }
                else if (choice == "3")
                {
                    GameMode selectedMode = PromptForMode("Gomoku");
                    game = new GomokuGame(selectedMode);
                    break;
                }
                else
                {
                    Console.WriteLine("❌ Invalid input. Please enter 1, 2, or 3.");
                }
            }

            game.Play();
        }

        static GameMode PromptForMode(string gameName)
        {
            while (true)
            {
                Console.WriteLine($"\nSelect mode for {gameName}:");
                Console.WriteLine("1. Human vs Human");
                Console.WriteLine("2. Human vs Computer");
                Console.Write("Enter your choice (1 or 2): ");
                string? modeChoice = Console.ReadLine()?.Trim();

                if (modeChoice == "1")
                    return GameMode.HumanVsHuman;
                else if (modeChoice == "2")
                    return GameMode.HumanVsComputer;
                else
                    Console.WriteLine("❌ Invalid input. Please enter 1 or 2.");
            }
        }
    }
}
