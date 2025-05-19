using IFN584_ASS2.Core;
using IFN584_ASS2.Enums;
using IFN584_ASS2.UserUI;
using IFN584_ASS2.Utilities;
using System;
using System.Collections.Generic;

namespace IFN584_ASS2.Games
{
    public class NotaktoGame : GameTemplate
    {
        public List<char[][]> Boards { get; set; } = new();
        public GameMode GameMode { get; set; } = GameMode.HumanVsHuman;

        public NotaktoGame(GameMode mode = GameMode.HumanVsHuman)
        {
            GameMode = mode;

            if (GameMode == GameMode.HumanVsComputer)
            {
                Player2.IsHuman = false;
                ComputerPlayer = Player2;
            }
        }

        public NotaktoGame() : this(GameMode.HumanVsHuman) { }

        protected override void Initialize()
        {
            // Only create fresh boards if Boards is empty (new game)
            if (Boards == null || Boards.Count != 3)
            {
                Boards = new List<char[][]>();
                for (int b = 0; b < 3; b++)
                {
                    var board = new char[3][]
                    {
                        new char[3] { ' ', ' ', ' ' },
                        new char[3] { ' ', ' ', ' ' },
                        new char[3] { ' ', ' ', ' ' }
                    };
                    Boards.Add(board);
                }
            }

            // Restore computer player after deserialization
            if (Player2 != null && !Player2.IsHuman)
                ComputerPlayer = Player2;

            ConsoleRenderer.RenderMessage("🧩 Notakto (3-board X-only Tic-Tac-Toe). Last to complete a line loses.");
        }

        protected override void DisplayBoard()
        {
            Console.WriteLine();
            for (int b = 0; b < Boards.Count; b++)
            {
                Console.WriteLine($"Board {b + 1}:");
                ConsoleRenderer.RenderBoard(Boards[b]);
            }
        }

        protected override bool IsMoveNumberValid(int input)
        {
            return true; // input is ignored
        }

        protected override void MakeMove(int _)
        {
            Console.Write("Choose board (1-3): ");
            if (!int.TryParse(Console.ReadLine(), out int boardIdx) || boardIdx < 1 || boardIdx > 3)
            {
                ConsoleRenderer.ShowError("🚫 Invalid board number. Must be 1, 2, or 3.");
                return;
            }

            boardIdx--;

            Console.Write("Enter row (0-2): ");
            if (!int.TryParse(Console.ReadLine(), out int row) || row < 0 || row > 2)
            {
                ConsoleRenderer.ShowError("🚫 Invalid row.");
                return;
            }

            Console.Write("Enter col (0-2): ");
            if (!int.TryParse(Console.ReadLine(), out int col) || col < 0 || col > 2)
            {
                ConsoleRenderer.ShowError("🚫 Invalid column.");
                return;
            }

            if (Boards[boardIdx][row][col] != ' ')
            {
                ConsoleRenderer.ShowError("🚫 That spot is already taken.");
                return;
            }

            Boards[boardIdx][row][col] = 'X';
            GameState.RecordMove(new Move(row, col, boardIdx));
        }

        protected override void MakeMoveWithCoords(int boardIndex, int row, int col)
        {
            if (boardIndex < 0 || boardIndex >= Boards.Count)
                throw new Exception("🚫 Invalid board index.");

            if (Boards[boardIndex][row][col] != ' ')
                throw new Exception("🚫 That spot is already taken.");

            Boards[boardIndex][row][col] = 'X';
            GameState.RecordMove(new Move(row, col, boardIndex));
        }

        protected override bool IsComputerTurn()
        {
            return GameMode == GameMode.HumanVsComputer && CurrentPlayer == ComputerPlayer;
        }

        protected override void MakeComputerMove()
        {
            for (int b = 0; b < Boards.Count; b++)
            {
                if (HasThreeInARow(Boards[b])) continue;

                var move = MoveSelector.FirstEmptyCell(Boards[b], ' ');
                if (move != null)
                {
                    Console.WriteLine($"🤖 Computer played at Board {b + 1}, ({move.Value.row}, {move.Value.col})");
                    Boards[b][move.Value.row][move.Value.col] = 'X';
                    GameState.RecordMove(new Move(move.Value.row, move.Value.col, b));
                    return;
                }
            }

            Console.WriteLine("❌ No available moves for computer.");
        }

        protected override bool IsGameOver()
        {
            int activeBoards = 0;
            foreach (var board in Boards)
            {
                if (!HasThreeInARow(board))
                    activeBoards++;
            }

            return activeBoards == 0;
        }

        private bool HasThreeInARow(char[][] b)
        {
            for (int i = 0; i < 3; i++)
            {
                if (b[i][0] == 'X' && b[i][1] == 'X' && b[i][2] == 'X') return true;
                if (b[0][i] == 'X' && b[1][i] == 'X' && b[2][i] == 'X') return true;
            }

            return (b[0][0] == 'X' && b[1][1] == 'X' && b[2][2] == 'X') ||
                   (b[0][2] == 'X' && b[1][1] == 'X' && b[2][0] == 'X');
        }

        protected override void AnnounceResult()
        {
            ConsoleRenderer.RenderMessage($"🪦 {CurrentPlayer.Name} made the last move. They lose.", ConsoleColor.Red);
        }

        protected override void SaveGame()
        {
            FileManager.Save(this);
        }

        protected override void Undo()
        {
            var move = GameState.Undo();
            if (move != null)
            {
                Boards[move.Value][move.Row][move.Col] = ' ';
                LastCommandWasUtility = true;
                ConsoleRenderer.ShowMessage("↩️ Undo successful. It's still your turn.", ConsoleColor.Cyan);
            }
            else
            {
                ConsoleRenderer.ShowMessage("ℹ️ No moves to undo.", ConsoleColor.Yellow);
            }
        }

        protected override void Redo()
        {
            var move = GameState.Redo();
            if (move != null)
            {
                Boards[move.Value][move.Row][move.Col] = 'X';
                LastCommandWasUtility = false;
                ConsoleRenderer.ShowMessage("↪️ Redo successful. It's still your turn.", ConsoleColor.Cyan);
            }
            else
            {
                ConsoleRenderer.ShowMessage("ℹ️ No moves to redo.", ConsoleColor.Yellow);
            }
        }
    }
}
