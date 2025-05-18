using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.UserUI
{
    public static class ConsoleRenderer
    {
        /// <summary>
        /// Renders a 2D game board (any value type: int, char, string).
        /// </summary>
        public static void RenderBoard<T>(T[,] board)
        {
            int size = board.GetLength(0);
            Console.WriteLine();

            for (int row = 0; row < size; row++)
            {
                Console.Write("|");
                for (int col = 0; col < size; col++)
                {
                    var cell = board[row, col];
                    string content = EqualityComparer<T>.Default.Equals(cell, default(T)) ? "  " : $"{cell,2}";
                    Console.Write($" {content} |");
                }
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Displays a message with optional color.
        /// </summary>
        public static void RenderMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Prompt the current player to enter a move.
        /// </summary>
        public static void PromptPlayer(string playerName)
        {
            Console.Write($"{playerName}, enter your move or command (undo, redo, save, help): ");
        }

        /// <summary>
        /// Renders the list of available numbers for a player.
        /// </summary>
        public static void ShowAvailableNumbers(List<int> numbers, string title = "Available Numbers")
        {
            Console.WriteLine($"{title}: {string.Join(", ", numbers)}");
        }

        /// <summary>
        /// Shows an error message in red.
        /// </summary>
        public static void ShowError(string errorMessage)
        {
            RenderMessage("❌ " + errorMessage, ConsoleColor.Red);
        }

        /// <summary>
        /// Shows a success/info message in green.
        /// </summary>
        public static void ShowSuccess(string message)
        {
            RenderMessage("✅ " + message, ConsoleColor.Green);
        }
    }
}
    