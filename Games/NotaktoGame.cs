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
        public List<bool> CompletedBoards { get; set; } = new();
        public List<string> BoardWinners { get; set; } = new();
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
            Boards.Clear();
            CompletedBoards.Clear();
            BoardWinners.Clear();

            for (int b = 0; b < 3; b++)
            {
                Boards.Add(new char[3][]
                {
                    new char[3] { ' ', ' ', ' ' },
                    new char[3] { ' ', ' ', ' ' },
                    new char[3] { ' ', ' ', ' ' }
                });
                CompletedBoards.Add(false);
                BoardWinners.Add("");
            }

            if (Player2 != null && !Player2.IsHuman)
                ComputerPlayer = Player2;

            ConsoleRenderer.RenderMessage("Notakto (3-board X-only Tic-Tac-Toe). Last to complete a line loses.");
        }

        protected override void DisplayBoard()
        {
            Console.WriteLine();
            for (int b = 0; b < Boards.Count; b++)
            {
                string status;
                if (CompletedBoards[b])
                    status = $"[DEAD - Completed by {BoardWinners[b]}]";
                else
                    status = "[Active]";

                Console.WriteLine($"Board {b + 1} {status}:");
                ConsoleRenderer.RenderBoard(Boards[b]);
            }
        }

        protected override bool IsMoveNumberValid(int input) => true;

        protected override void MakeMove(int input)
        {
            ConsoleRenderer.ShowError("This game requires board, row and column input separately.");
        }

        protected override void MakeMoveWithCoords(int boardIndex, int row, int col)
        {
            if (CompletedBoards[boardIndex])
            {
                throw new Exception($"Board {boardIndex + 1} is completed and cannot be used anymore.");
            }

            if (Boards[boardIndex][row][col] == ' ')
            {
                Boards[boardIndex][row][col] = 'X';
                if (HasThreeInARow(Boards[boardIndex]))
                {
                    CompletedBoards[boardIndex] = true;
                    BoardWinners[boardIndex] = CurrentPlayer.Name;
                    ConsoleRenderer.RenderMessage($"Board {boardIndex + 1} completed by {CurrentPlayer.Name}. This board is now dead!", ConsoleColor.Yellow);
                }
                GameState.RecordMove(new Move(row, col, boardIndex));
                return;
            }

            throw new Exception("That cell is already occupied.");
        }

        protected override bool IsComputerTurn() =>
            GameMode == GameMode.HumanVsComputer && CurrentPlayer == ComputerPlayer;

        protected override void MakeComputerMove()
        {
            for (int b = 0; b < Boards.Count; b++)
            {
                var safeMoves = new List<(int row, int col)>();

                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (Boards[b][r][c] == ' ')
                        {
                            Boards[b][r][c] = 'X';
                            bool willLose = HasThreeInARow(Boards[b]);
                            Boards[b][r][c] = ' ';

                            if (!willLose)
                                safeMoves.Add((r, c));
                        }
                    }
                }

                if (safeMoves.Count > 0)
                {
                    var move = safeMoves[new Random().Next(safeMoves.Count)];
                    Boards[b][move.row][move.col] = 'X';
                    if (HasThreeInARow(Boards[b]))
                    {
                        CompletedBoards[b] = true;
                        BoardWinners[b] = CurrentPlayer.Name;
                        ConsoleRenderer.RenderMessage($"Board {b + 1} completed by {CurrentPlayer.Name}.", ConsoleColor.Yellow);
                    }
                    Console.WriteLine($"Computer played at Board {b + 1}, Row {move.row + 1}, Col {move.col + 1}");
                    GameState.RecordMove(new Move(move.row, move.col, b));
                    return;
                }
            }

            for (int b = 0; b < Boards.Count; b++)
            {
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (Boards[b][r][c] == ' ')
                        {
                            Boards[b][r][c] = 'X';
                            if (HasThreeInARow(Boards[b]))
                            {
                                CompletedBoards[b] = true;
                                BoardWinners[b] = CurrentPlayer.Name;
                                ConsoleRenderer.RenderMessage($"Board {b + 1} completed by {CurrentPlayer.Name}.", ConsoleColor.Yellow);
                            }
                            Console.WriteLine($"Computer played at Board {b + 1}, Row {r + 1}, Col {c + 1}");
                            GameState.RecordMove(new Move(r, c, b));
                            return;
                        }
                    }
                }
            }
        }

        protected override bool IsGameOver()
        {
            for (int b = 0; b < Boards.Count; b++)
            {
                if (CompletedBoards[b]) continue;

                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (Boards[b][r][c] == ' ')
                            return false; 
                    }
                }
            }
            return true; 
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
            var winner = CurrentPlayer == Player1 ? Player2.Name : Player1.Name;
            ConsoleRenderer.RenderMessage($"{winner} wins!", ConsoleColor.Green);
        }

        protected override void SaveGame()
        {
            try
            {
                FileManager.Save(this);
                ConsoleRenderer.RenderMessage("Game state saved to file successfully!", ConsoleColor.Green);
                ConsoleRenderer.ShowMessage($"Current player: {CurrentPlayer.Name}", ConsoleColor.White);
                ConsoleRenderer.ShowMessage($"Boards completed: {CompletedBoards.Count(c => c)}/3", ConsoleColor.White);
            }
            catch (Exception ex)
            {
                ConsoleRenderer.ShowError($"Failed to save game: {ex.Message}");
            }
        }

        protected override void Undo()
        {
            var move = GameState.Undo();
            if (move != null)
            {
                Boards[move.Value][move.Row][move.Col] = ' ';
                CompletedBoards[move.Value] = HasThreeInARow(Boards[move.Value]);
                if (!CompletedBoards[move.Value])
                    BoardWinners[move.Value] = "";
                LastCommandWasUtility = true;
                ConsoleRenderer.ShowMessage($"Undo successful! Removed X from Board {move.Value + 1}, Row {move.Row + 1}, Col {move.Col + 1}.", ConsoleColor.Cyan);
                ConsoleRenderer.ShowMessage("It's still your turn.", ConsoleColor.Cyan);
            }
            else
            {
                ConsoleRenderer.ShowMessage("No moves to undo.", ConsoleColor.Yellow);
            }
        }

        protected override void Redo()
        {
            var move = GameState.Redo();
            if (move != null)
            {
                Boards[move.Value][move.Row][move.Col] = 'X';
                if (HasThreeInARow(Boards[move.Value]))
                {
                    CompletedBoards[move.Value] = true;
                    BoardWinners[move.Value] = CurrentPlayer.Name;
                }
                LastCommandWasUtility = false;
                ConsoleRenderer.ShowMessage($"Redo successful! Placed X at Board {move.Value + 1}, Row {move.Row + 1}, Col {move.Col + 1}.", ConsoleColor.Cyan);
                ConsoleRenderer.ShowMessage("It's still your turn.", ConsoleColor.Cyan);
            }
            else
            {
                ConsoleRenderer.ShowMessage("No moves to redo.", ConsoleColor.Yellow);
            }
        }

        private int GetValidInput(string prompt, int min, int max, out bool isUtilityCommand)
        {
            isUtilityCommand = false;
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine()?.Trim().ToLower();

                switch (input)
                {
                    case "menu":
                    case "main menu":
                        isUtilityCommand = true;
                        return -1; 
                    case "undo":
                        Console.WriteLine();
                        DisplayBoard(); 
                        ConsoleRenderer.RenderMessage("Performing undo...", ConsoleColor.Yellow);
                        Undo();
                        Console.WriteLine();
                        DisplayBoard(); 
                        isUtilityCommand = true;
                        return -2; 
                    case "redo":
                        Console.WriteLine();
                        DisplayBoard();
                        ConsoleRenderer.RenderMessage("Performing redo...", ConsoleColor.Yellow);
                        Redo();
                        Console.WriteLine();
                        DisplayBoard(); 
                        isUtilityCommand = true;
                        return -3; 
                    case "help":
                        ShowHelp();
                        continue; 
                    case "save":
                        Console.WriteLine();
                        ConsoleRenderer.RenderMessage("Saving game...", ConsoleColor.Yellow);
                        SaveGame();
                        ConsoleRenderer.ShowMessage("Game saved successfully!", ConsoleColor.Green);
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        Console.WriteLine();
                        DisplayBoard(); 
                        continue; 
                }

                if (int.TryParse(input, out int value) && value >= min && value <= max)
                {
                    return value;
                }

                ConsoleRenderer.ShowError($"Please enter a number between {min} and {max}, or use commands: help, save, undo, redo, menu");
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine();
            ConsoleRenderer.RenderMessage("**NOTAKTO HELP**", ConsoleColor.Cyan);
            Console.WriteLine("Game Rules:");
            Console.WriteLine("- This is X-only Tic-Tac-Toe played on 3 boards");
            Console.WriteLine("- Players take turns placing X's on any ACTIVE board");
            Console.WriteLine("- When a board gets 3 X's in a row, it becomes DEAD");
            Console.WriteLine("- DEAD boards cannot be used anymore!");
            Console.WriteLine("- The player who makes the last possible move WINS");
            Console.WriteLine("- Game ends when all boards are either dead or completely filled");
            Console.WriteLine();
            Console.WriteLine("Commands available anytime:");
            Console.WriteLine("- help: Show this help message");
            Console.WriteLine("- save: Save the current game");
            Console.WriteLine("- undo: Undo the last move");
            Console.WriteLine("- redo: Redo a previously undone move");
            Console.WriteLine("- menu: Return to main menu");
            Console.WriteLine();
            Console.WriteLine("How to play:");
            Console.WriteLine("1. Enter board number (1-3) - only ACTIVE boards work");
            Console.WriteLine("2. Enter row number (1-3)");
            Console.WriteLine("3. Enter column number (1-3)");
            Console.WriteLine();
        }

        public override bool Play()
        {
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

                bool valid = false;
                while (!valid)
                {
                    Console.WriteLine($"\n{CurrentPlayer.Name}, it's your turn!");
                    Console.WriteLine("(You can type 'help' for commands, or 'menu' to return to main menu)");

                    int boardNumber = GetValidInput("Enter board number (1-3): ", 1, 3, out bool isUtilityCommand1);
                    if (isUtilityCommand1)
                    {
                        if (boardNumber == -1) 
                            return false;
                        if (boardNumber == -2 || boardNumber == -3) 
                        {
                            LastCommandWasUtility = true;
                            continue; 
                        }
                    }

                    // Get row number
                    int rowNumber = GetValidInput("Enter row number (1-3): ", 1, 3, out bool isUtilityCommand2);
                    if (isUtilityCommand2)
                    {
                        if (rowNumber == -1) 
                            return false;
                        if (rowNumber == -2 || rowNumber == -3) 
                        {
                            LastCommandWasUtility = true;
                            continue; 
                        }
                    }

                    // Get column number
                    int colNumber = GetValidInput("Enter column number (1-3): ", 1, 3, out bool isUtilityCommand3);
                    if (isUtilityCommand3)
                    {
                        if (colNumber == -1) 
                            return false;
                        if (colNumber == -2 || colNumber == -3) 
                        {
                            LastCommandWasUtility = true;
                            continue; 
                        }
                    }

                    try
                    {
                        MakeMoveWithCoords(boardNumber - 1, rowNumber - 1, colNumber - 1);
                        valid = true;
                        Console.WriteLine($"Move placed at Board {boardNumber}, Row {rowNumber}, Col {colNumber}");
                    }
                    catch (Exception ex)
                    {
                        ConsoleRenderer.ShowError($"Error: {ex.Message}");
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
    }
}