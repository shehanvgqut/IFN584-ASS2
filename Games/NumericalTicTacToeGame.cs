using IFN584_ASS2.Core;
using IFN584_ASS2.Enums;
using IFN584_ASS2.UserUI;
using IFN584_ASS2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IFN584_ASS2.Games
{
    public class NumericalTicTacToeGame : GameTemplate
    {
        public int[][] Board { get; set; } = new int[3][] {
            new int[3], new int[3], new int[3]
        };
        public HashSet<int> UsedNumbers { get; set; } = new();
        public GameMode Mode { get; set; }

        public NumericalTicTacToeGame(GameMode mode = GameMode.HumanVsHuman)
        {
            Mode = mode;
        }

        protected override void Initialize()
        {
            Console.WriteLine("🎯 Starting Numerical Tic-Tac-Toe...");
            if (Mode == GameMode.HumanVsComputer)
            {
                Player2.IsHuman = false;
                ComputerPlayer = Player2;
            }
        }

        protected override void DisplayBoard()
        {
            ConsoleRenderer.RenderBoard(Board);
        }

        protected override bool IsComputerTurn()
        {
            return Mode == GameMode.HumanVsComputer && CurrentPlayer == ComputerPlayer;
        }

        protected override void MakeComputerMove()
        {
            foreach (var num in MoveSelector.AvailableNumbers(UsedNumbers, isOdd: false))
            {
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (Board[r][c] == 0 && IsWinningMove(r, c, num))
                        {
                            Console.WriteLine($"🤖 Computer played winning move {num} at ({r}, {c})");
                            Board[r][c] = num;
                            UsedNumbers.Add(num);
                            GameState.RecordMove(new Move(r, c, num));
                            return;
                        }
                    }
                }
            }

            var fallbackEmpty = MoveSelector.FirstEmptyCell(Board, 0);
            if (fallbackEmpty == null || MoveSelector.AvailableNumbers(UsedNumbers, false).Count == 0)
                return;

            int fallbackNum = MoveSelector.AvailableNumbers(UsedNumbers, false).First();
            Board[fallbackEmpty.Value.row][fallbackEmpty.Value.col] = fallbackNum;
            UsedNumbers.Add(fallbackNum);
            GameState.RecordMove(new Move(fallbackEmpty.Value.row, fallbackEmpty.Value.col, fallbackNum));
            Console.WriteLine($"🤖 Computer played {fallbackNum} at ({fallbackEmpty.Value.row}, {fallbackEmpty.Value.col})");
        }

        protected override void MakeMove(int input)
        {
            if (!IsMoveNumberValid(input))
            {
                Console.WriteLine("🚫 That number is invalid or already used.");
                return;
            }

            Console.Write("Enter row (0-2): ");
            if (!int.TryParse(Console.ReadLine(), out int row) || row < 0 || row > 2)
            {
                Console.WriteLine("🚫 Invalid row.");
                return;
            }

            Console.Write("Enter col (0-2): ");
            if (!int.TryParse(Console.ReadLine(), out int col) || col < 0 || col > 2)
            {
                Console.WriteLine("🚫 Invalid column.");
                return;
            }

            MakeMoveWithCoords(input, row, col);
        }

        protected override void MakeMoveWithCoords(int input, int row, int col)
        {
            if (Board[row][col] != 0)
                throw new Exception("🚫 That spot is already taken.");

            Board[row][col] = input;
            UsedNumbers.Add(input);
            GameState.RecordMove(new Move(row, col, input));
        }

        protected override bool IsMoveNumberValid(int input)
        {
            if (input < 1 || input > 9 || UsedNumbers.Contains(input))
                return false;
            return (CurrentPlayer.IsOddPlayer && input % 2 == 1) || (!CurrentPlayer.IsOddPlayer && input % 2 == 0);
        }

        protected override bool IsGameOver()
        {
            int[][] lines = {
                new[] { Board[0][0], Board[0][1], Board[0][2] },
                new[] { Board[1][0], Board[1][1], Board[1][2] },
                new[] { Board[2][0], Board[2][1], Board[2][2] },
                new[] { Board[0][0], Board[1][0], Board[2][0] },
                new[] { Board[0][1], Board[1][1], Board[2][1] },
                new[] { Board[0][2], Board[1][2], Board[2][2] },
                new[] { Board[0][0], Board[1][1], Board[2][2] },
                new[] { Board[0][2], Board[1][1], Board[2][0] }
            };

            return lines.Any(line => line.All(n => n != 0) && line.Sum() == 15) || UsedNumbers.Count == 9;
        }

        protected override void AnnounceResult()
        {
            if (Mode == GameMode.HumanVsComputer && ComputerPlayer.IsHuman == false)
                Console.WriteLine(UsedNumbers.Count == 9 ? "🤝 It's a draw!" : $"🏆 Computer wins!");
            else
                Console.WriteLine(UsedNumbers.Count == 9 ? "🤝 It's a draw!" : $"🏆 {CurrentPlayer.Name} wins!");

        }

        protected override void SaveGame()
        {
            FileManager.Save(this);
        }
        protected override void Undo()
        {
            var move = GameState.Undo();
            if (move == null)
            {
                ConsoleRenderer.ShowMessage("ℹ️ No moves to undo.", ConsoleColor.Yellow);
            }
            else
            {
                Board[move.Row][move.Col] = 0;
                UsedNumbers.Remove(move.Value);
                LastCommandWasUndo = true;
                ConsoleRenderer.ShowMessage($"↩️ Undo successful. It's still {CurrentPlayer.Name}'s turn.", ConsoleColor.Cyan);
            }
        }

        protected override void Redo()
        {
            var move = GameState.Redo();
            if (move == null)
            {
                ConsoleRenderer.ShowMessage("ℹ️ No moves to redo.", ConsoleColor.Yellow);
            }
            else
            {
                Board[move.Row][move.Col] = move.Value;
                UsedNumbers.Add(move.Value);
                LastCommandWasUndo = false;
                ConsoleRenderer.ShowMessage($"↪️ Redo successful. It's still {CurrentPlayer.Name}'s turn.", ConsoleColor.Cyan);
            }
        }

        private bool IsWinningMove(int r, int c, int num)
        {
            Board[r][c] = num;
            UsedNumbers.Add(num);

            bool win = IsGameOver();

            Board[r][c] = 0;
            UsedNumbers.Remove(num);

            return win;
        }

    }
}
