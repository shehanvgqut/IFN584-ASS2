using System;
using System.Collections.Generic;

namespace IFN584_ASS2.Core
{
    public static class MoveSelector
    {
        /// <summary>
        /// Returns the first empty cell on a char[,] board.
        /// Used in Gomoku and Notakto.
        /// </summary>
        public static (int row, int col)? FirstEmptyCell(char[,] board, char empty = '.')
        {
            int rows = board.GetLength(0);
            int cols = board.GetLength(1);

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (board[r, c] == empty)
                        return (r, c);

            return null;
        }

        /// <summary>
        /// Returns a list of unused numbers from the range 1 to 9, filtered by parity.
        /// Used in Numerical Tic-Tac-Toe.
        /// </summary>
        public static List<int> AvailableNumbers(HashSet<int> used, bool isOdd)
        {
            var numbers = new List<int>();

            for (int i = 1; i <= 9; i++)
            {
                if (!used.Contains(i) && (i % 2 == (isOdd ? 1 : 0)))
                {
                    numbers.Add(i);
                }
            }

            return numbers;
        }

        /// <summary>
        /// Returns the first empty cell on a jagged array (T[][]).
        /// Works with int[][] or char[][].
        /// </summary>
        public static (int row, int col)? FirstEmptyCell<T>(T[][] board, T empty)
        {
            for (int r = 0; r < board.Length; r++)
            {
                for (int c = 0; c < board[r].Length; c++)
                {
                    if (EqualityComparer<T>.Default.Equals(board[r][c], empty))
                        return (r, c);
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a random empty cell on a jagged array (T[][]).
        /// </summary>
        public static (int row, int col)? RandomEmptyCell<T>(T[][] board, T empty)
        {
            var emptyCells = new List<(int, int)>();

            for (int r = 0; r < board.Length; r++)
            {
                for (int c = 0; c < board[r].Length; c++)
                {
                    if (EqualityComparer<T>.Default.Equals(board[r][c], empty))
                        emptyCells.Add((r, c));
                }
            }

            if (emptyCells.Count == 0)
                return null;

            var rand = new Random();
            return emptyCells[rand.Next(emptyCells.Count)];
        }
    }
}
