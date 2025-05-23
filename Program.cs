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
            Console.WriteLine("Welcome to the Multi-Board Game Framework!");

            string? loadInput;
            while (true)
            {
                Console.Write("Do you want to load a saved game? (y/n): ");
                loadInput = Console.ReadLine()?.Trim().ToLower();

                if (loadInput == "y" || loadInput == "n")
                    break;

                Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
            }

            bool keepPlaying = true;

            if (loadInput == "y")
            {
                var loadedGame = FileManager.Load();
                if (loadedGame != null)
                {
                    if (loadedGame is NumericalTicTacToeGame nt)
                        nt.MarkAsLoaded();
                    keepPlaying = loadedGame.Play(); // returns false if user types "menu"
                }
                else
                {
                    Console.WriteLine("No valid saved game found. Starting a new game.");
                }
            }

            while (keepPlaying)
            {
                GameTemplate game = null;

                while (true)
                {
                    Console.WriteLine("\nChoose a game to play:");
                    Console.WriteLine("1. Numerical Tic-Tac-Toe");
                    Console.WriteLine("2. Notakto");
                    Console.WriteLine("3. Gomoku");
                    Console.WriteLine("0. Exit");
                    Console.Write("Enter your choice (0-3): ");
                    string? choice = Console.ReadLine()?.Trim();

                    if (choice == "1")
                    {
                        GameMode selectedMode = PromptForMode("Numerical Tic-Tac-Toe");
                        var numGame = new NumericalTicTacToeGame();

                        numGame.Player1 = new Player("Player 1", true);
                        numGame.Player2 = selectedMode == GameMode.HumanVsComputer
                            ? new Player("Computer", false)
                            : new Player("Player 2", false);

                        game = numGame;
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
                    else if (choice == "0")
                    {
                        Console.WriteLine("Goodbye!");
                        keepPlaying = false;
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter 0, 1, 2, or 3.");
                    }
                }

                if (game != null)
                {
                    // if game.Play() returns false, the user chose to go back to menu
                    if (!game.Play())
                    {
                        continue; // return to game menu
                    }
                }
            }
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
                    Console.WriteLine("Invalid input. Please enter 1 or 2.");
            }
        }
    }
}
