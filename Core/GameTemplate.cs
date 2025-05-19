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
        protected bool LastCommandWasUtility = false; // Includes undo, redo, save

        public Player ComputerPlayer { get; protected set; }

        public void Play()
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
                    string? input = Console.ReadLine()?.Trim().ToLower();
                    LastCommandWasUtility = false; // Reset before each new input

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
                        default:
                            if (int.TryParse(input, out int num))
                            {
                                try
                                {
                                    if (!IsMoveNumberValid(num))
                                    {
                                        ConsoleRenderer.ShowError("🚫 That number is invalid or already used. Try another.");
                                        break;
                                    }

                                    Console.Write("Enter row (0-2): ");
                                    if (!int.TryParse(Console.ReadLine(), out int row) || row < 0 || row > 2)
                                    {
                                        ConsoleRenderer.ShowError("🚫 Invalid row. Please enter 0, 1, or 2.");
                                        break;
                                    }

                                    Console.Write("Enter col (0-2): ");
                                    if (!int.TryParse(Console.ReadLine(), out int col) || col < 0 || col > 2)
                                    {
                                        ConsoleRenderer.ShowError("🚫 Invalid column. Please enter 0, 1, or 2.");
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
                                ConsoleRenderer.ShowError("🚫 Invalid command or input.");
                            }
                            break;
                    }
                }

                if (!IsGameOver())
                {
                    if (LastCommandWasUtility)
                    {
                        LastCommandWasUtility = false;
                    }
                    else
                    {
                        SwitchPlayers();
                    }
                }
            }

            DisplayBoard();
            AnnounceResult();
        }

        // 🔓 Expose players and game state for serialization
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
        protected abstract void MakeMove(int input);
        protected abstract bool IsGameOver();
        protected abstract void AnnounceResult();

        protected virtual bool IsComputerTurn()
        {
            return false;
        }

        protected virtual bool IsMoveNumberValid(int input)
        {
            return true;
        }

        protected virtual void MakeMoveWithCoords(int input, int row, int col)
        {
            throw new NotImplementedException("MakeMoveWithCoords must be overridden in derived classes.");
        }

        protected virtual void MakeComputerMove()
        {
        }

        private void SwitchPlayers()
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
            HelpMenu.Show();
        }

        protected virtual void Undo()
        {
            var move = GameState.Undo();
            if (move == null)
            {
                ConsoleRenderer.ShowMessage("ℹ️ No moves to undo.", ConsoleColor.Yellow);
            }
            else
            {
                ConsoleRenderer.ShowMessage($"↩️ Undo successful. It's still {CurrentPlayer.Name}'s turn.", ConsoleColor.Cyan);
            }
        }

        protected virtual void Redo()
        {
            var move = GameState.Redo();
            if (move == null)
            {
                ConsoleRenderer.ShowMessage("ℹ️ No moves to redo.", ConsoleColor.Yellow);
            }
            else
            {
                ConsoleRenderer.ShowMessage($"↪️ Redo successful. It's still {CurrentPlayer.Name}'s turn.", ConsoleColor.Cyan);
            }
        }

        #endregion
    }
}
