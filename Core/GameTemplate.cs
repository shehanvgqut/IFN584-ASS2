using IFN584_ASS2.UserUI;
using IFN584_ASS2.Utilities;
using System;

namespace IFN584_ASS2.Core
{
    public abstract class GameTemplate
    {
        protected Player CurrentPlayer = new("Player 1", true);
        protected Player OtherPlayer = new("Player 2", false);
        protected GameState GameState = new();
        protected bool LastCommandWasUtility = false;

        public Player ComputerPlayer { get; set; }

        protected virtual int MaxRow => 2;
        protected virtual int MaxCol => 2;

        public virtual bool Play()
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
                    Console.Write($"{CurrentPlayer.Name}, enter your move or command (undo, redo, save, help, menu): ");
                    string? input = Console.ReadLine()?.Trim().ToLower();
                    LastCommandWasUtility = false;

                    switch (input)
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

                        case "help":
                            ShowHelp();
                            break;

                        case "save":
                            SaveGame();
                            LastCommandWasUtility = true;
                            validInput = true;
                            break;

                        case "menu":
                        case "back":
                        case "back to menu":
                            ConsoleRenderer.ShowMessage("Returning to the main menu...", ConsoleColor.Yellow);
                            return false;

                        default:
                            if (int.TryParse(input, out int num))
                            {
                                try
                                {
                                    if (!IsMoveNumberValid(num))
                                    {
                                        ConsoleRenderer.ShowError("That number is invalid or already used. Try another.");
                                        break;
                                    }

                                    Console.Write($"Enter row (0–{MaxRow}): ");
                                    if (!int.TryParse(Console.ReadLine(), out int row) || row < 0 || row > MaxRow)
                                    {
                                        ConsoleRenderer.ShowError($"Invalid row. Please enter 0 to {MaxRow}.");
                                        break;
                                    }

                                    Console.Write($"Enter col (0–{MaxCol}): ");
                                    if (!int.TryParse(Console.ReadLine(), out int col) || col < 0 || col > MaxCol)
                                    {
                                        ConsoleRenderer.ShowError($"Invalid column. Please enter 0 to {MaxCol}.");
                                        break;
                                    }

                                    MakeMoveWithCoords(num, row, col);
                                    validInput = true;
                                }
                                catch (Exception ex)
                                {
                                    ConsoleRenderer.ShowError($"Error: {ex.Message}");
                                }
                            }
                            else
                            {
                                ConsoleRenderer.ShowError("Invalid command or input.");
                            }
                            break;
                    }
                }

                if (!IsGameOver())
                {
                    if (!LastCommandWasUtility)
                        SwitchPlayers();
                    LastCommandWasUtility = false;
                }
            }

            DisplayBoard();
            AnnounceResult();
            return true;
        }

        public Player Player1
        {
            get => CurrentPlayer;
            set => CurrentPlayer = value;
        }

        public Player Player2
        {
            get => OtherPlayer;
            set => OtherPlayer = value;
        }

        public GameState State
        {
            get => GameState;
            set => GameState = value;
        }

        protected abstract void Initialize();
        protected abstract void DisplayBoard();
        public void ShowBoard() => DisplayBoard();

        protected abstract void MakeMove(int input);
        protected abstract bool IsGameOver();
        protected abstract void AnnounceResult();

        protected virtual bool IsComputerTurn() => false;
        protected virtual bool IsMoveNumberValid(int input) => true;

        protected virtual void MakeMoveWithCoords(int input, int row, int col)
        {
            throw new NotImplementedException("MakeMoveWithCoords must be overridden in derived classes.");
        }

        protected virtual void MakeComputerMove() { }

        protected void SwitchPlayers()
        {
            (CurrentPlayer, OtherPlayer) = (OtherPlayer, CurrentPlayer);
        }

        #region Virtual Implementation

        protected virtual void SaveGame()
        {
            FileManager.Save(this);
        }

        protected virtual void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine("  undo   - Revert the last move");
            Console.WriteLine("  redo   - Reapply the last undone move");
            Console.WriteLine("  save   - Save the current game state");
            Console.WriteLine("  help   - Show this help menu");
            Console.WriteLine("  menu   - Return to the main menu");
            Console.WriteLine("  Or enter a number to make a move, followed by row and column.");
            Console.WriteLine();
        }

        protected virtual void Undo()
        {
            var move = GameState.Undo();
            if (move == null)
                ConsoleRenderer.ShowMessage("No moves to undo.", ConsoleColor.Yellow);
            else
                ConsoleRenderer.ShowMessage($"Undo successful. It's still {CurrentPlayer.Name}'s turn.", ConsoleColor.Cyan);
        }

        protected virtual void Redo()
        {
            var move = GameState.Redo();
            if (move == null)
                ConsoleRenderer.ShowMessage("No moves to redo.", ConsoleColor.Yellow);
            else
                ConsoleRenderer.ShowMessage($"Redo successful. It's still {CurrentPlayer.Name}'s turn.", ConsoleColor.Cyan);
        }

        #endregion
    }
}
