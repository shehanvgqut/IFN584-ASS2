using IFN584_ASS2.Core;
using IFN584_ASS2.UserUI;
using IFN584_ASS2.Utilities;
using System;

namespace IFN584_ASS2.Games
{
    public class GomokuGame : GameTemplate
    {
        private const int Size = 15;
        private char[,] board = new char[Size, Size];
        private char CurrentSymbol => CurrentPlayer.IsOddPlayer ? 'X' : 'O';

        protected override void Initialize()
        {
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    board[r, c] = '.';

            ConsoleRenderer.RenderMessage("🔳 Gomoku (Five in a Row). Get 5 of your symbol in a row.");
        }

        protected override void DisplayBoard()
        {
            ConsoleRenderer.RenderBoard(board);
        }

        protected override void MakeMove(int dummy)
        {
            Console.Write("Enter row (0-14): ");
            if (!int.TryParse(Console.ReadLine(), out int row) || row < 0 || row >= 15)
            {
                ConsoleRenderer.ShowError("🚫 Invalid row. Must be between 0 and 14.");
                return;
            }

            Console.Write("Enter col (0-14): ");
            if (!int.TryParse(Console.ReadLine(), out int col) || col < 0 || col >= 15)
            {
                ConsoleRenderer.ShowError("🚫 Invalid column. Must be between 0 and 14.");
                return;
            }

            if (board[row, col] != '.')
            {
                ConsoleRenderer.ShowError("🚫 That spot is already taken.");
                return;
            }

            board[row, col] = CurrentSymbol;
            GameState.RecordMove(new Move(row, col, CurrentSymbol));
        }


        protected override bool IsGameOver()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    char sym = board[r, c];
                    if (sym == '.') continue;

                    if (CheckDirection(r, c, 1, 0, sym) ||
                        CheckDirection(r, c, 0, 1, sym) ||
                        CheckDirection(r, c, 1, 1, sym) ||
                        CheckDirection(r, c, 1, -1, sym))
                        return true;
                }
            }

            return false;
        }

        private bool CheckDirection(int r, int c, int dr, int dc, char sym)
        {
            int count = 1;
            for (int i = 1; i < 5; i++)
            {
                int nr = r + i * dr;
                int nc = c + i * dc;
                if (nr >= Size || nc >= Size || nr < 0 || nc < 0 || board[nr, nc] != sym)
                    break;
                count++;
            }

            return count == 5;
        }

        protected override void AnnounceResult()
        {
            ConsoleRenderer.RenderMessage($"🏆 {CurrentPlayer.Name} wins with 5 in a row!", ConsoleColor.Green);
        }

        protected override void ShowHelp()
        {
            HelpProvider.ShowHelp("gomoku");
        }

        protected override void SaveGame()
        {
            FileManager.Save(this);
        }

        protected override void Undo()
        {
            var move = GameState.Undo();
            if (move != null)
                board[move.Row, move.Col] = '.';
            else
                ConsoleRenderer.ShowError("No moves to undo.");
        }

        protected override void Redo()
        {
            var move = GameState.Redo();
            if (move != null)
                board[move.Row, move.Col] = (char)move.Value;
            else
                ConsoleRenderer.ShowError("No moves to redo.");
        }
    }
}
