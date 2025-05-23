using IFN584_ASS2.Core;
using IFN584_ASS2.UserUI;
using IFN584_ASS2.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Serialization;

namespace IFN584_ASS2.Games
{
    public class NumericalTicTacToeGame : GameTemplate
    {
        // Internal game state (not directly serialized)
        [JsonIgnore] private int[,] board;
        [JsonIgnore] private bool isLoadedGame = false;

        // Serialized representations
        [JsonInclude] public int[][] BoardData { get; set; }
        [JsonInclude] public int BoardSize { get; set; }
        [JsonInclude] public int TargetSum { get; set; }
        [JsonInclude] public HashSet<int> UsedNumbers { get; set; } = new();

        /// <summary>
        /// Call this immediately after loading to skip re-initialization prompts.
        /// </summary>
        public void MarkAsLoaded() => isLoadedGame = true;

        protected override void Initialize()
        {
            if (isLoadedGame)
            {
                // Reconstruct internal board from serialized data
                board = new int[BoardSize, BoardSize];
                for (int i = 0; i < BoardSize; i++)
                    for (int j = 0; j < BoardSize; j++)
                        board[i, j] = BoardData[i][j];

                ConsoleRenderer.RenderMessage(
                    $"🎯 Resuming Numerical Tic-Tac-Toe {BoardSize}x{BoardSize}. Target sum: {TargetSum}"
                );
                return;
            }

            // ── Start of New-game prompt block ──
            Console.Write("Enter board size (e.g. 3 for 3x3): ");
            int parsedSize;
            while (!int.TryParse(Console.ReadLine(), out parsedSize) || parsedSize < 2)
            {
                ConsoleRenderer.ShowError("Please enter a valid integer (minimum 2).");
            }

            BoardSize = parsedSize;
            TargetSum = BoardSize * (BoardSize * BoardSize + 1) / 2;
            board = new int[BoardSize, BoardSize];
            UsedNumbers = new HashSet<int>();
            // ── End of New-game prompt block ──

            ConsoleRenderer.RenderMessage(
                $"🎯 Starting Numerical Tic-Tac-Toe {BoardSize}x{BoardSize}. Target sum: {TargetSum}"
            );
        }



        protected override void DisplayBoard() =>
            ConsoleRenderer.RenderBoard(board);

        protected override void MakeMove(int input) =>
            ConsoleRenderer.ShowError("Direct input not supported. Use row/col format.");

        private bool TryMakeMove(int input, int row, int col)
        {
            int max = BoardSize * BoardSize;
            if (input < 1 || input > max)
            {
                ConsoleRenderer.ShowError($"🚫 Input must be between 1 and {max}.");
                return false;
            }
            if (UsedNumbers.Contains(input))
            {
                ConsoleRenderer.ShowError("🚫 That number has already been used.");
                return false;
            }
            if ((CurrentPlayer.IsOddPlayer && input % 2 == 0) ||
                (!CurrentPlayer.IsOddPlayer && input % 2 != 0))
            {
                ConsoleRenderer.ShowError("🚫 You must use your assigned number type (odd/even).");
                return false;
            }
            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            {
                ConsoleRenderer.ShowError("🚫 Row or column out of bounds.");
                return false;
            }
            if (board[row, col] != 0)
            {
                ConsoleRenderer.ShowError("🚫 That spot is already taken.");
                return false;
            }

            board[row, col] = input;
            UsedNumbers.Add(input);
            GameState.RecordMove(new Move(row, col, input));
            return true;
        }

        protected override void MakeMoveWithCoords(int input, int row, int col) =>
            TryMakeMove(input, row, col);

        protected override bool IsGameOver()
        {
            bool checkLine(int idx, bool isRow)
            {
                int sum = 0;
                for (int i = 0; i < BoardSize; i++)
                {
                    int val = isRow ? board[idx, i] : board[i, idx];
                    if (val == 0) return false;
                    sum += val;
                }
                return sum == TargetSum;
            }

            for (int i = 0; i < BoardSize; i++)
                if (checkLine(i, true) || checkLine(i, false))
                    return true;

            int sumMain = 0, sumAnti = 0;
            for (int i = 0; i < BoardSize; i++)
            {
                if (board[i, i] == 0 || board[i, BoardSize - 1 - i] == 0) break;
                sumMain += board[i, i];
                sumAnti += board[i, BoardSize - 1 - i];
            }
            if (sumMain == TargetSum || sumAnti == TargetSum) return true;

            return UsedNumbers.Count == BoardSize * BoardSize;
        }

        protected override void AnnounceResult()
        {
            if (UsedNumbers.Count == BoardSize * BoardSize)
                ConsoleRenderer.RenderMessage("🤝 It's a draw!");
            else
                ConsoleRenderer.RenderMessage($"🏆 {CurrentPlayer.Name} wins!");
        }

        protected override void SaveGame()
        {
            // Populate jagged BoardData for JSON serialization
            BoardData = Enumerable.Range(0, BoardSize)
                .Select(i => Enumerable.Range(0, BoardSize)
                                       .Select(j => board[i, j])
                                       .ToArray())
                .ToArray();

            FileManager.Save(this);
        }

        protected override void ShowHelp() =>
            HelpProvider.ShowHelp("numerical");

        public override void Play()
        {
            // Always initialize or reconstruct the board
            Initialize();

            while (!IsGameOver())
            {
                DisplayBoard();

                if (IsComputerTurn())
                {
                    MakeComputerMove();
                    if (!IsGameOver()) SwitchPlayers();
                    continue;
                }

                bool moved = false;
                while (!moved)
                {
                    ConsoleRenderer.PromptPlayer(CurrentPlayer.Name);
                    string? cmd = Console.ReadLine()?.Trim().ToLower();

                    switch (cmd)
                    {
                        case "help": ShowHelp(); continue;
                        case "undo": Undo(); moved = true; continue;
                        case "redo": Redo(); moved = true; continue;
                        case "save": SaveGame(); moved = true; continue;
                    }

                    if (int.TryParse(cmd, out int num))
                    {
                        Console.Write($"Enter row (1-{BoardSize}): ");
                        if (!int.TryParse(Console.ReadLine(), out int r)) continue;
                        Console.Write($"Enter col (1-{BoardSize}): ");
                        if (!int.TryParse(Console.ReadLine(), out int c)) continue;
                        moved = TryMakeMove(num, r - 1, c - 1);
                    }
                    else
                    {
                        ConsoleRenderer.ShowError("🚫 Invalid input.");
                    }
                }

                if (!IsGameOver())
                    SwitchPlayers();
            }

            DisplayBoard();
            AnnounceResult();
        }

        protected override bool IsComputerTurn() =>
            CurrentPlayer.Name == "Computer";

        protected override void MakeComputerMove()
        {
            var valid = Enumerable.Range(1, BoardSize * BoardSize)
                                  .Where(n => n % 2 == (CurrentPlayer.IsOddPlayer ? 1 : 0)
                                           && !UsedNumbers.Contains(n))
                                  .ToList();
            var rnd = new Random();

            // Try winning move
            foreach (var num in valid)
                for (int r = 0; r < BoardSize; r++)
                    for (int c = 0; c < BoardSize; c++)
                        if (board[r, c] == 0)
                        {
                            board[r, c] = num;
                            if (IsGameOver()) { TryMakeMove(num, r, c); return; }
                            board[r, c] = 0;
                        }

            // Otherwise random
            while (true)
            {
                int r = rnd.Next(BoardSize), c = rnd.Next(BoardSize);
                if (board[r, c] == 0)
                {
                    TryMakeMove(valid[rnd.Next(valid.Count)], r, c);
                    return;
                }
            }
        }

        protected override void Undo()
        {
            var m1 = GameState.Undo();
            if (m1 == null)
            {
                ConsoleRenderer.ShowMessage("ℹ️ No moves to undo.", ConsoleColor.Yellow);
                return;
            }
            if (IsComputerTurn())
            {
                ConsoleRenderer.ShowMessage("↩️ Move undone. It's your turn again.", ConsoleColor.Cyan);
                return;
            }
            var m2 = GameState.Undo();
            if (m2 != null)
            {
                ConsoleRenderer.ShowMessage("↩️ Reverted both moves. Your turn.", ConsoleColor.Cyan);
                SwitchPlayers();
            }
        }

        protected override int MaxRow => BoardSize - 1;
        protected override int MaxCol => BoardSize - 1;
    }
}
