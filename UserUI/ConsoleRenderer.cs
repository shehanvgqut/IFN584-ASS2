using System;
using System.Collections.Generic;

namespace IFN584_ASS2.UserUI
{
    public static class ConsoleRenderer
    {
        // ✅ NEW: Support rectangular 2D arrays like int[,] or char[,]
        public static void RenderBoard<T>(T[,] board)
        {
            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            Console.WriteLine();
            for (int r = 0; r < rows; r++)
            {
                Console.Write("|");
                for (int c = 0; c < cols; c++)
                {
                    var cell = board[r, c];
                    string content = EqualityComparer<T>.Default.Equals(cell, default) ? "  " : $"{cell,2}";
                    Console.Write($" {content} |");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        // ✅ Already existing: Jagged array support (used if any game uses T[][])
        public static void RenderBoard<T>(T[][] board)
        {
            Console.WriteLine();
            for (int row = 0; row < board.Length; row++)
            {
                Console.Write("|");
                for (int col = 0; col < board[row].Length; col++)
                {
                    var cell = board[row][col];
                    string content = EqualityComparer<T>.Default.Equals(cell, default(T)) ? "  " : $"{cell,2}";
                    Console.Write($" {content} |");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        // ✅ Optional: used by some char[][] with 1-based labels
        public static void RenderBoard(char[][] board, bool baseOne)
        {
            int size = board.Length;
            Console.Write("    ");
            for (int c = 0; c < size; c++)
                Console.Write($"{(baseOne ? c + 1 : c),3} ");
            Console.WriteLine();

            for (int r = 0; r < size; r++)
            {
                Console.Write($"{(baseOne ? r + 1 : r),3} ");
                for (int c = 0; c < size; c++)
                {
                    Console.Write($" {board[r][c],2} ");
                }
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public static void RenderMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PromptPlayer(string playerName)
        {
            Console.Write($"{playerName}, enter your move or command (undo, redo, save, help): ");
        }

        public static void ShowAvailableNumbers(List<int> numbers, string title = "Available Numbers")
        {
            Console.WriteLine($"{title}: {string.Join(", ", numbers)}");
        }

        public static void ShowError(string errorMessage)
        {
            RenderMessage("❌ " + errorMessage, ConsoleColor.Red);
        }

        public static void ShowSuccess(string message)
        {
            RenderMessage("✅ " + message, ConsoleColor.Green);
        }

        public static void ShowMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
