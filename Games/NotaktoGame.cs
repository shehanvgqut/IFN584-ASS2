using IFN584_ASS2.Core;
using IFN584_ASS2.UserUI;
using IFN584_ASS2.Utilities;
using System;

namespace IFN584_ASS2.Games
{
    public class NotaktoGame : GameTemplate
    {
        private char[][,] boards = new char[3][,];
        private int currentBoard = 0;

        protected override void Initialize()
        {
            for (int i = 0; i < 3; i++)
            {
                boards[i] = new char[3, 3];
                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 3; c++)
                        boards[i][r, c] = ' ';
            }
            ConsoleRenderer.RenderMessage("🧩 Notakto (3-board X-only Tic-Tac-Toe). Last to complete a line loses.");
        }

        protected override void DisplayBoard()
        {
            Console.WriteLine();
            for (int b = 0; b < 3; b++)
            {
                Console.WriteLine($"Board {b + 1}:");
                ConsoleRenderer.RenderBoard(boards[b]);
            }
        }

        protected override void MakeMove(int dummy)
        {
            Console.Write("Choose board (1-3): ");
            if (!int.TryParse(Console.ReadLine(), out int boardIdx) || boardIdx < 1 || boardIdx > 3)
            {
                ConsoleRenderer.ShowError("🚫 Invalid board number. Must be 1, 2, or 3.");
                return;
            }

            boardIdx--; // convert to 0-based index

            Console.Write("Enter row (0-2): ");
            if (!int.TryParse(Console.ReadLine(), out int row) || row < 0 || row > 2)
            {
                ConsoleRenderer.ShowError("🚫 Invalid row. Must be 0, 1, or 2.");
                return;
            }

            Console.Write("Enter col (0-2): ");
            if (!int.TryParse(Console.ReadLine(), out int col) || col < 0 || col > 2)
            {
                ConsoleRenderer.ShowError("🚫 Invalid column. Must be 0, 1, or 2.");
                return;
            }

            if (boards[boardIdx][row, col] != ' ')
            {
                ConsoleRenderer.ShowError("🚫 Position already taken.");
                return;
            }

            boards[boardIdx][row, col] = 'X';
            GameState.RecordMove(new Move(row, col, boardIdx));
        }

        protected override bool IsGameOver()
        {
            int activeBoards = 0;

            foreach (var board in boards)
            {
                if (!HasThreeInARow(board))
                    activeBoards++;
            }

            return activeBoards == 0;
        }

        private bool HasThreeInARow(char[,] b)
        {
            for (int i = 0; i < 3; i++)
            {
                if (b[i, 0] == 'X' && b[i, 1] == 'X' && b[i, 2] == 'X') return true;
                if (b[0, i] == 'X' && b[1, i] == 'X' && b[2, i] == 'X') return true;
            }

            return (b[0, 0] == 'X' && b[1, 1] == 'X' && b[2, 2] == 'X') ||
                   (b[0, 2] == 'X' && b[1, 1] == 'X' && b[2, 0] == 'X');
        }

        protected override void AnnounceResult()
        {
            ConsoleRenderer.RenderMessage($"🪦 {CurrentPlayer.Name} made the last move. They lose.", ConsoleColor.Red);
        }

        protected override void ShowHelp()
        {
            HelpProvider.ShowHelp("notakto");
        }

        protected override void SaveGame()
        {
            FileManager.Save(this);
        }

        protected override void Undo()
        {
            var move = GameState.Undo();
            if (move != null)
                boards[move.Value][move.Row, move.Col] = ' ';
            else
                ConsoleRenderer.ShowError("No moves to undo.");
        }

        protected override void Redo()
        {
            var move = GameState.Redo();
            if (move != null)
                boards[move.Value][move.Row, move.Col] = 'X';
            else
                ConsoleRenderer.ShowError("No moves to redo.");
        }
    }
}
