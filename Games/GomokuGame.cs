using IFN584_ASS2.Core;
using IFN584_ASS2.Enums;
using IFN584_ASS2.UserUI;
using IFN584_ASS2.Utilities;
using System;

namespace IFN584_ASS2.Games
{
    public class GomokuGame : GameTemplate
    {
        public char[][] Board { get; set; } = new char[15][];
        private const int Size = 15;
        private GameMode gameMode;

        private char CurrentSymbol => CurrentPlayer.IsOddPlayer ? 'X' : 'O';

        public GomokuGame(GameMode mode = GameMode.HumanVsHuman)
        {
            gameMode = mode;
        }

        protected override void Initialize()
        {
            for (int i = 0; i < Size; i++)
            {
                Board[i] = new char[Size];
                for (int j = 0; j < Size; j++)
                    Board[i][j] = '.';
            }

            if (gameMode == GameMode.HumanVsComputer)
                OtherPlayer.IsHuman = false;

            ConsoleRenderer.RenderMessage("🔳 Gomoku (Five in a Row). Get 5 of your symbol in a row.");
        }

        protected override void DisplayBoard()
        {
            ConsoleRenderer.RenderBoard(Board);
        }

        protected override bool IsMoveNumberValid(int input) => true;

        protected override void MakeMoveWithCoords(int _, int row, int col)
        {
            if (Board[row][col] != '.')
                throw new Exception("🚫 That spot is already taken.");

            Board[row][col] = CurrentSymbol;
            GameState.RecordMove(new Move(row, col, CurrentSymbol));
        }

        protected override void MakeMove(int _)
        {
            Console.Write("Enter row (0-14): ");
            if (!int.TryParse(Console.ReadLine(), out int row) || row < 0 || row >= Size)
            {
                ConsoleRenderer.ShowError("🚫 Invalid row. Must be between 0 and 14.");
                return;
            }

            Console.Write("Enter col (0-14): ");
            if (!int.TryParse(Console.ReadLine(), out int col) || col < 0 || col >= Size)
            {
                ConsoleRenderer.ShowError("🚫 Invalid column. Must be between 0 and 14.");
                return;
            }

            if (Board[row][col] != '.')
            {
                ConsoleRenderer.ShowError("🚫 That spot is already taken.");
                return;
            }

            Board[row][col] = CurrentSymbol;
            GameState.RecordMove(new Move(row, col, CurrentSymbol));
        }

        protected override bool IsComputerTurn()
        {
            return gameMode == GameMode.HumanVsComputer && CurrentPlayer == ComputerPlayer;
        }

        protected override void MakeComputerMove()
        {
            var move = MoveSelector.FirstEmptyCell(Board, '.');
            if (move == null)
            {
                Console.WriteLine("❌ No available moves.");
                return;
            }

            int row = move.Value.row;
            int col = move.Value.col;
            char symbol = CurrentSymbol;

            Console.WriteLine($"🤖 Computer placed {symbol} at ({row}, {col})");
            Board[row][col] = symbol;
            GameState.RecordMove(new Move(row, col, symbol));
        }

        protected override bool IsGameOver()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    char sym = Board[r][c];
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
                if (nr < 0 || nc < 0 || nr >= Size || nc >= Size || Board[nr][nc] != sym)
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
                Board[move.Row][move.Col] = '.';
            else
                ConsoleRenderer.ShowError("No moves to undo.");
        }

        protected override void Redo()
        {
            var move = GameState.Redo();
            if (move != null)
                Board[move.Row][move.Col] = (char)move.Value;
            else
                ConsoleRenderer.ShowError("No moves to redo.");
        }
    }
}
