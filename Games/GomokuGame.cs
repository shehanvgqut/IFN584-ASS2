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

        public GameMode GameMode { get; set; } = GameMode.HumanVsHuman;

        private char CurrentSymbol => CurrentPlayer.IsOddPlayer ? 'X' : 'O';

        public GomokuGame(GameMode mode = GameMode.HumanVsHuman)
        {
            GameMode = mode;
            if (GameMode == GameMode.HumanVsComputer)
            {
                Player2.IsHuman = false;
                ComputerPlayer = Player2;
            }
        }

        public GomokuGame() : this(GameMode.HumanVsHuman) { }

        protected override void Initialize()
        {
            // Only reinitialize if the board wasn't restored from file
            if (Board == null || Board.Length != Size || Board[0] == null || Board[0].Length != Size || Board[0][0] == '\0')
            {
                for (int i = 0; i < Size; i++)
                {
                    Board[i] = new char[Size];
                    for (int j = 0; j < Size; j++)
                        Board[i][j] = '.';
                }
            }

            if (Player2 != null && !Player2.IsHuman)
                ComputerPlayer = Player2;

            ConsoleRenderer.RenderMessage("🔳 Gomoku (Five in a Row). Get 5 of your symbol in a row.");
        }


        protected override void DisplayBoard()
        {
            ConsoleRenderer.RenderBoard(Board);
        }

        protected override bool IsMoveNumberValid(int input) => true;

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

        protected override void MakeMoveWithCoords(int _, int row, int col)
        {
            if (Board[row][col] != '.')
                throw new Exception("🚫 That spot is already taken.");

            Board[row][col] = CurrentSymbol;
            GameState.RecordMove(new Move(row, col, CurrentSymbol));
        }

        protected override bool IsComputerTurn()
        {
            return GameMode == GameMode.HumanVsComputer && CurrentPlayer == ComputerPlayer;
        }

        protected override void MakeComputerMove()
        {
            // 1. Try to win
            var winMove = FindWinningMove(CurrentSymbol);
            if (winMove != null)
            {
                Console.WriteLine($"🤖 Computer placed {CurrentSymbol} at ({winMove.Value.row}, {winMove.Value.col}) — Winning move!");
                Board[winMove.Value.row][winMove.Value.col] = CurrentSymbol;
                GameState.RecordMove(new Move(winMove.Value.row, winMove.Value.col, CurrentSymbol));
                return;
            }

            // 2. Otherwise, pick a random empty cell
            var emptyCells = new List<(int row, int col)>();
            for (int r = 0; r < Board.Length; r++)
                for (int c = 0; c < Board[r].Length; c++)
                    if (Board[r][c] == '.')
                        emptyCells.Add((r, c));

            if (emptyCells.Count > 0)
            {
                var rand = new Random();
                var move = emptyCells[rand.Next(emptyCells.Count)];
                Console.WriteLine($"🤖 Computer placed {CurrentSymbol} at ({move.row}, {move.col})");
                Board[move.row][move.col] = CurrentSymbol;
                GameState.RecordMove(new Move(move.row, move.col, CurrentSymbol));
            }
            else
            {
                Console.WriteLine("❌ No available moves.");
            }
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
            {
                Board[move.Row][move.Col] = '.';
                LastCommandWasUtility = true;
                ConsoleRenderer.ShowMessage("↩️ Undo successful.", ConsoleColor.Cyan);
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
                Board[move.Row][move.Col] = (char)move.Value;
                LastCommandWasUtility = false;
                ConsoleRenderer.ShowMessage("↪️ Redo successful.", ConsoleColor.Cyan);
            }
            else
            {
                ConsoleRenderer.ShowMessage("ℹ️ No moves to redo.", ConsoleColor.Yellow);
            }
        }
        private (int row, int col)? FindWinningMove(char symbol)
        {
            for (int r = 0; r < Board.Length; r++)
            {
                for (int c = 0; c < Board[r].Length; c++)
                {
                    if (Board[r][c] != '.') continue;

                    // Try placing symbol temporarily
                    Board[r][c] = symbol;

                    bool wins = CheckDirection(r, c, 1, 0, symbol) ||
                                CheckDirection(r, c, 0, 1, symbol) ||
                                CheckDirection(r, c, 1, 1, symbol) ||
                                CheckDirection(r, c, 1, -1, symbol);

                    Board[r][c] = '.'; // undo test move

                    if (wins)
                        return (r, c);
                }
            }

            return null; // no winning move found
        }


        // ✅ Enables dynamic row/col range in GameTemplate
        protected override int MaxRow => 14;
        protected override int MaxCol => 14;
        public override void Play()
        {
            Initialize();

            while (!IsGameOver())
            {
                DisplayBoard();

                if (IsComputerTurn())
                {
                    MakeComputerMove();
                    if (!IsGameOver())
                        SwitchPlayers();
                    continue;
                }

                bool validInput = false;
                while (!validInput)
                {
                    ConsoleRenderer.PromptPlayer(CurrentPlayer.Name);
                    string? command = Console.ReadLine()?.Trim().ToLower();

                    switch (command)
                    {
                        case "undo":
                            Undo();
                            LastCommandWasUtility = true;
                            validInput = true;
                            break;
                        case "redo":
                            Redo();
                            LastCommandWasUtility = true;
                            validInput = true;
                            break;
                        case "save":
                            SaveGame();
                            LastCommandWasUtility = true;
                            validInput = true;
                            break;
                        case "help":
                            ShowHelp();
                            break;
                        default:
                            Console.Write($"Enter row (0-{MaxRow}): ");
                            if (!int.TryParse(command, out int row) || row < 0 || row > MaxRow)
                            {
                                ConsoleRenderer.ShowError($"🚫 Invalid row. Please enter 0 to {MaxRow}.");
                                continue;
                            }

                            Console.Write($"Enter col (0-{MaxCol}): ");
                            if (!int.TryParse(Console.ReadLine(), out int col) || col < 0 || col > MaxCol)
                            {
                                ConsoleRenderer.ShowError($"🚫 Invalid column. Please enter 0 to {MaxCol}.");
                                continue;
                            }

                            try
                            {
                                MakeMoveWithCoords(0, row, col);
                                validInput = true;
                            }
                            catch (Exception ex)
                            {
                                ConsoleRenderer.ShowError($"Error: {ex.Message}");
                            }
                            break;
                    }
                }

                if (!IsGameOver())
                {
                    if (LastCommandWasUtility)
                        LastCommandWasUtility = false;
                    else
                        SwitchPlayers();
                }
            }

            DisplayBoard();
            AnnounceResult();
        }


    }
}