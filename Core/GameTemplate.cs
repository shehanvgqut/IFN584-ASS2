using IFN584_ASS2.UserUI;
using IFN584_ASS2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.Core
{
    public abstract class GameTemplate
    {
        protected Player CurrentPlayer = new("Player 1", true);
        protected Player OtherPlayer = new("Player 2", false);
        protected GameState GameState = new();

        public void Play()
        {
            Initialize();

            while (!IsGameOver())
            {
                DisplayBoard();

                bool validInput = false;
                while (!validInput)
                {
                    ConsoleRenderer.PromptPlayer(CurrentPlayer.Name);
                    string? input = Console.ReadLine()?.Trim().ToLower();

                    switch (input)
                    {
                        case "undo": Undo(); validInput = true; break;
                        case "redo": Redo(); validInput = true; break;
                        case "help": ShowHelp(); break;
                        case "save": SaveGame(); validInput = true; break;
                        default:
                            if (int.TryParse(input, out int num))
                            {
                                try
                                {
                                    MakeMove(num);
                                    validInput = true;
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    ConsoleRenderer.ShowError("You entered a position outside the board. Try again.");
                                }
                                catch (Exception ex)
                                {
                                    ConsoleRenderer.ShowError($"Error: {ex.Message}");
                                }
                            }
                            else
                            {
                                ConsoleRenderer.ShowError("Invalid input. Please enter a number or type 'help'.");
                            }
                            break;
                    }
                }

                if (!IsGameOver())
                    SwitchPlayers();
            }

            DisplayBoard();
            AnnounceResult();
        }


        protected abstract void Initialize();
        protected abstract void DisplayBoard();
        protected abstract void MakeMove(int input);
        protected abstract bool IsGameOver();
        protected abstract void AnnounceResult();
        
        private void SwitchPlayers()
        {
            (CurrentPlayer, OtherPlayer) = (OtherPlayer, CurrentPlayer);
        }

        #region Virtual implementation
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
            GameState.Undo();
        }

        protected virtual void Redo()
        {
            GameState.Redo();
        }

        #endregion
    }

}
