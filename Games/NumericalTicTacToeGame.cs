using IFN584_ASS2.Core;
using IFN584_ASS2.UserUI;
using IFN584_ASS2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.Games
{
        public class NumericalTicTacToeGame : GameTemplate
        {
            private int[,] board = new int[3, 3];
            private HashSet<int> usedNumbers = new();

            protected override void Initialize()
            {
                Console.WriteLine("🎯 Starting Numerical Tic-Tac-Toe...");
            }

            protected override void DisplayBoard()
            {
                ConsoleRenderer.RenderBoard(board);
            }


        protected override void MakeMove(int input)
        {
            if (input < 1 || input > 9)
            {
                Console.WriteLine("🚫 Input must be between 1 and 9.");
                return;
            }

            if (usedNumbers.Contains(input))
            {
                Console.WriteLine("🚫 This number has already been used.");
                return;
            }

            if (CurrentPlayer.IsOddPlayer && input % 2 == 0 ||
                !CurrentPlayer.IsOddPlayer && input % 2 != 0)
            {
                Console.WriteLine("🚫 You must use your assigned number type (odd/even).");
                return;
            }

            Console.Write("Enter row (0-2): ");
            if (!int.TryParse(Console.ReadLine(), out int row) || row < 0 || row > 2)
            {
                Console.WriteLine("🚫 Invalid row. Please enter a number between 0 and 2.");
                return;
            }

            Console.Write("Enter col (0-2): ");
            if (!int.TryParse(Console.ReadLine(), out int col) || col < 0 || col > 2)
            {
                Console.WriteLine("🚫 Invalid column. Please enter a number between 0 and 2.");
                return;
            }

            if (board[row, col] != 0)
            {
                Console.WriteLine("🚫 That spot is taken.");
                return;
            }

            board[row, col] = input;
            usedNumbers.Add(input);
            GameState.RecordMove(new Move(row, col, input));
        }

        protected override bool IsGameOver()
            {
                int[][] lines = {
                new[] { board[0,0], board[0,1], board[0,2] },
                new[] { board[1,0], board[1,1], board[1,2] },
                new[] { board[2,0], board[2,1], board[2,2] },
                new[] { board[0,0], board[1,0], board[2,0] },
                new[] { board[0,1], board[1,1], board[2,1] },
                new[] { board[0,2], board[1,2], board[2,2] },
                new[] { board[0,0], board[1,1], board[2,2] },
                new[] { board[0,2], board[1,1], board[2,0] },
            };

                foreach (var line in lines)
                {
                    if (line.All(n => n != 0) && line.Sum() == 15)
                        return true;
                }

                return usedNumbers.Count == 9;
            }

            protected override void AnnounceResult()
            {
                if (usedNumbers.Count == 9)
                    Console.WriteLine("🤝 It's a draw!");
                else
                    Console.WriteLine($"🏆 {CurrentPlayer.Name} wins!");
            }

            protected override void SaveGame()
            {
                FileManager.Save(this);
            }

            protected override void ShowHelp()
            {
                HelpProvider.ShowHelp("numerical");
            }
    }
}
