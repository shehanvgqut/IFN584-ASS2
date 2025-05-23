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
        [JsonIgnore] private int[,] board;
        [JsonIgnore] private bool isLoadedGame = false;

        [JsonInclude] public int[][] BoardData { get; set; }
        [JsonInclude] public int BoardSize { get; set; }
        [JsonInclude] public int TargetSum { get; set; }
        [JsonInclude] public HashSet<int> UsedNumbers { get; set; } = new();
        [JsonInclude] public GameState SavedGameState { get; set; }

        public void MarkAsLoaded() => isLoadedGame = true;

        protected override void Initialize()
        {
            if (isLoadedGame)
            {
                board = new int[BoardSize, BoardSize];
                for (int i = 0; i < BoardSize; i++)
                    for (int j = 0; j < BoardSize; j++)
                        board[i, j] = BoardData[i][j];

                // Restore the game state with move history
                if (SavedGameState != null)
                {
                    GameState = SavedGameState;
                }
                else
                {
                    GameState = new GameState();
                }

                ConsoleRenderer.RenderMessage($"Resuming Numerical Tic-Tac-Toe {BoardSize}x{BoardSize}. Target sum: {TargetSum}");
                return;
            }

            Console.Write("Enter board size (e.g. 3 for 3x3): ");
            int parsedSize;
            while (!int.TryParse(Console.ReadLine(), out parsedSize) || parsedSize < 3)
            {
                ConsoleRenderer.ShowError("Please enter a valid integer (minimum 3). 2x2 is not supported due to mathematical constraints.");
            }

            BoardSize = parsedSize;
            TargetSum = BoardSize * (BoardSize * BoardSize + 1) / 2;
            board = new int[BoardSize, BoardSize];
            UsedNumbers = new HashSet<int>();

            ConsoleRenderer.RenderMessage($"Starting Numerical Tic-Tac-Toe {BoardSize}x{BoardSize}. Target sum: {TargetSum}");
        }

        protected override void DisplayBoard() => ConsoleRenderer.RenderBoard(board);
        protected override void MakeMove(int input) => ConsoleRenderer.ShowError("Direct input not supported. Use row/col format.");
        protected override void MakeMoveWithCoords(int input, int row, int col) => TryMakeMove(input, row, col);

        private bool TryMakeMove(int input, int row, int col)
        {
            int max = BoardSize * BoardSize;
            if (input < 1 || input > max || UsedNumbers.Contains(input)
                || (CurrentPlayer.IsOddPlayer && input % 2 == 0)
                || (!CurrentPlayer.IsOddPlayer && input % 2 != 0)
                || row < 0 || row >= BoardSize || col < 0 || col >= BoardSize || board[row, col] != 0)
            {
                ConsoleRenderer.ShowError("Invalid move.");
                return false;
            }

            board[row, col] = input;
            UsedNumbers.Add(input);
            GameState.RecordMove(new Move(row, col, input));
            return true;
        }

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

            // Check main diagonal
            int sumMain = 0;
            bool mainDiagonalComplete = true;
            for (int i = 0; i < BoardSize; i++)
            {
                if (board[i, i] == 0)
                {
                    mainDiagonalComplete = false;
                    break;
                }
                sumMain += board[i, i];
            }
            if (mainDiagonalComplete && sumMain == TargetSum) return true;

            // Check anti-diagonal
            int sumAnti = 0;
            bool antiDiagonalComplete = true;
            for (int i = 0; i < BoardSize; i++)
            {
                if (board[i, BoardSize - 1 - i] == 0)
                {
                    antiDiagonalComplete = false;
                    break;
                }
                sumAnti += board[i, BoardSize - 1 - i];
            }
            if (antiDiagonalComplete && sumAnti == TargetSum) return true;

            return UsedNumbers.Count == BoardSize * BoardSize;
        }

        protected override void AnnounceResult()
        {
            if (UsedNumbers.Count == BoardSize * BoardSize)
                ConsoleRenderer.RenderMessage("It's a draw.");
            else
                ConsoleRenderer.RenderMessage($"{CurrentPlayer.Name} wins!");
        }

        protected override void SaveGame()
        {
            BoardData = Enumerable.Range(0, BoardSize)
                .Select(i => Enumerable.Range(0, BoardSize)
                                       .Select(j => board[i, j])
                                       .ToArray())
                .ToArray();

            // Save the current game state with move history
            SavedGameState = GameState;

            FileManager.Save(this);
        }

        protected override void ShowHelp()
        {
            int maxNumber = BoardSize * BoardSize;
            var oddNumbers = Enumerable.Range(1, maxNumber).Where(n => n % 2 == 1).ToList();
            var evenNumbers = Enumerable.Range(1, maxNumber).Where(n => n % 2 == 0).ToList();

            ConsoleRenderer.RenderMessage($"Numerical Tic-Tac-Toe Help:\n" +
                $"- Player 1 (odd numbers): {string.Join(", ", oddNumbers)}\n" +
                $"- Player 2 (even numbers): {string.Join(", ", evenNumbers)}\n" +
                $"- Target sum: {TargetSum}\n" +
                $"- Form a row/column/diagonal summing to {TargetSum} to win.\n" +
                $"- Enter a number, then specify row and column (1-{BoardSize}).\n");
        }

        public override bool Play()
        {
            Initialize();
            GameState ??= new GameState();

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
                    Console.Write($"{CurrentPlayer.Name}, enter your move or command (undo, redo, save, help, menu): ");
                    string? cmd = Console.ReadLine()?.Trim().ToLower();

                    switch (cmd)
                    {
                        case "help":
                            ShowHelp();
                            continue;
                        case "undo":
                            Undo();
                            moved = false; // Undo handles its own player switching
                            continue;
                        case "redo":
                            Redo();
                            moved = false; // Redo handles its own player switching
                            continue;
                        case "save":
                            SaveGame();
                            ConsoleRenderer.ShowMessage("Game saved successfully.", ConsoleColor.Green);
                            moved = false; // Save doesn't count as a move, don't switch players
                            continue;
                        case "menu":
                        case "back":
                        case "back to menu":
                            ConsoleRenderer.ShowMessage("Returning to main menu...", ConsoleColor.Yellow);
                            return false;
                    }

                    if (int.TryParse(cmd, out int num))
                    {
                        Console.Write($"Enter row (1-{BoardSize}): ");
                        if (!int.TryParse(Console.ReadLine(), out int r))
                        {
                            ConsoleRenderer.ShowError("Invalid row number.");
                            continue;
                        }
                        Console.Write($"Enter col (1-{BoardSize}): ");
                        if (!int.TryParse(Console.ReadLine(), out int c))
                        {
                            ConsoleRenderer.ShowError("Invalid column number.");
                            continue;
                        }
                        moved = TryMakeMove(num, r - 1, c - 1);
                    }
                    else
                    {
                        ConsoleRenderer.ShowError("Invalid input. Enter a number or command.");
                    }
                }

                if (!IsGameOver()) SwitchPlayers();
            }

            DisplayBoard();
            AnnounceResult();
            return true;
        }

        protected override bool IsComputerTurn() => CurrentPlayer.Name == "Computer";

        // IMPROVED AI IMPLEMENTATION
        protected override void MakeComputerMove()
        {
            var validNumbers = Enumerable.Range(1, BoardSize * BoardSize)
                                  .Where(n => n % 2 == (CurrentPlayer.IsOddPlayer ? 1 : 0)
                                           && !UsedNumbers.Contains(n))
                                  .ToList();

            if (validNumbers.Count == 0) return;

            var bestMove = GetBestMove(validNumbers);

            if (bestMove != null)
            {
                TryMakeMove(bestMove.Value.number, bestMove.Value.row, bestMove.Value.col);
                ConsoleRenderer.ShowMessage($"Computer plays {bestMove.Value.number} at ({bestMove.Value.row + 1}, {bestMove.Value.col + 1})", ConsoleColor.Green);
            }
        }

        private (int number, int row, int col)? GetBestMove(List<int> validNumbers)
        {
            // First, try to win immediately
            var winningMove = FindWinningMove(validNumbers);
            if (winningMove != null) return winningMove;

            // Second, try to block opponent from winning
            var blockingMove = FindBlockingMove();
            if (blockingMove != null) return blockingMove;

            // For small boards or early game, use strategic heuristics
            if (BoardSize <= 4 || UsedNumbers.Count < BoardSize * 2)
            {
                var strategicMove = FindStrategicMove(validNumbers);
                if (strategicMove != null) return strategicMove;
            }

            // Use minimax for deeper analysis
            return FindBestMoveWithMinimax(validNumbers, depth: Math.Min(6, validNumbers.Count));
        }

        private (int number, int row, int col)? FindWinningMove(List<int> validNumbers)
        {
            foreach (var num in validNumbers)
            {
                for (int r = 0; r < BoardSize; r++)
                {
                    for (int c = 0; c < BoardSize; c++)
                    {
                        if (board[r, c] == 0)
                        {
                            board[r, c] = num;
                            UsedNumbers.Add(num);

                            bool isWinning = CheckWinCondition();

                            board[r, c] = 0;
                            UsedNumbers.Remove(num);

                            if (isWinning)
                                return (num, r, c);
                        }
                    }
                }
            }
            return null;
        }

        private (int number, int row, int col)? FindBlockingMove()
        {
            // Get opponent's valid numbers
            var opponentNumbers = Enumerable.Range(1, BoardSize * BoardSize)
                                     .Where(n => n % 2 == (CurrentPlayer.IsOddPlayer ? 0 : 1)
                                              && !UsedNumbers.Contains(n))
                                     .ToList();

            foreach (var num in opponentNumbers)
            {
                for (int r = 0; r < BoardSize; r++)
                {
                    for (int c = 0; c < BoardSize; c++)
                    {
                        if (board[r, c] == 0)
                        {
                            board[r, c] = num;
                            UsedNumbers.Add(num);

                            bool opponentWins = CheckWinCondition();

                            board[r, c] = 0;
                            UsedNumbers.Remove(num);

                            if (opponentWins)
                            {
                                // Try to block with our numbers
                                var myNumbers = Enumerable.Range(1, BoardSize * BoardSize)
                                                   .Where(n => n % 2 == (CurrentPlayer.IsOddPlayer ? 1 : 0)
                                                            && !UsedNumbers.Contains(n))
                                                   .ToList();

                                // Find a number that can block this position
                                foreach (var myNum in myNumbers)
                                {
                                    return (myNum, r, c);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private (int number, int row, int col)? FindStrategicMove(List<int> validNumbers)
        {
            var rnd = new Random();

            // Strategy 1: Prioritize center positions for odd-sized boards
            if (BoardSize % 2 == 1)
            {
                int center = BoardSize / 2;
                if (board[center, center] == 0)
                {
                    var bestNum = GetBestNumberForPosition(center, center, validNumbers);
                    return (bestNum, center, center);
                }
            }

            // Strategy 2: Prioritize corners and edges
            var priorityPositions = GetPriorityPositions().Where(pos => board[pos.row, pos.col] == 0).ToList();

            if (priorityPositions.Any())
            {
                var pos = priorityPositions[rnd.Next(priorityPositions.Count)];
                var bestNum = GetBestNumberForPosition(pos.row, pos.col, validNumbers);
                return (bestNum, pos.row, pos.col);
            }

            // Strategy 3: Random valid move
            var emptyCells = GetEmptyCells();
            if (emptyCells.Any())
            {
                var pos = emptyCells[rnd.Next(emptyCells.Count)];
                var num = validNumbers[rnd.Next(validNumbers.Count)];
                return (num, pos.row, pos.col);
            }

            return null;
        }

        private List<(int row, int col)> GetPriorityPositions()
        {
            var positions = new List<(int row, int col)>();

            // Add corners
            positions.Add((0, 0));
            positions.Add((0, BoardSize - 1));
            positions.Add((BoardSize - 1, 0));
            positions.Add((BoardSize - 1, BoardSize - 1));

            // Add edges
            for (int i = 1; i < BoardSize - 1; i++)
            {
                positions.Add((0, i));
                positions.Add((BoardSize - 1, i));
                positions.Add((i, 0));
                positions.Add((i, BoardSize - 1));
            }

            return positions;
        }

        private int GetBestNumberForPosition(int row, int col, List<int> validNumbers)
        {
            // Try to find a number that maximizes potential winning lines
            var bestScore = -1;
            var bestNumber = validNumbers[0];

            foreach (var num in validNumbers)
            {
                var score = EvaluateNumberForPosition(num, row, col);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestNumber = num;
                }
            }

            return bestNumber;
        }

        private int EvaluateNumberForPosition(int number, int row, int col)
        {
            int score = 0;

            // Check how this number contributes to potential winning lines
            score += EvaluateLineContribution(number, row, col, true);  // Row
            score += EvaluateLineContribution(number, row, col, false); // Column

            // Check diagonals
            if (row == col)
                score += EvaluateLineContribution(number, row, col, true, true); // Main diagonal

            if (row + col == BoardSize - 1)
                score += EvaluateLineContribution(number, row, col, false, true); // Anti-diagonal

            return score;
        }

        private int EvaluateLineContribution(int number, int row, int col, bool isRow, bool isDiagonal = false)
        {
            int currentSum = 0;
            int emptyCount = 0;

            for (int i = 0; i < BoardSize; i++)
            {
                int value;
                if (isDiagonal)
                {
                    value = isRow ? board[i, i] : board[i, BoardSize - 1 - i];
                }
                else
                {
                    value = isRow ? board[row, i] : board[i, col];
                }

                if (value == 0)
                    emptyCount++;
                else
                    currentSum += value;
            }

            // If adding this number could lead to target sum, give high score
            int remainingSum = TargetSum - currentSum - number;
            if (emptyCount == 1 && remainingSum == 0)
                return 100; // This would be a winning move

            // Give points based on how close we are to target
            return Math.Max(0, 10 - Math.Abs(remainingSum));
        }

        private (int number, int row, int col)? FindBestMoveWithMinimax(List<int> validNumbers, int depth)
        {
            var bestScore = int.MinValue;
            (int number, int row, int col)? bestMove = null;

            foreach (var num in validNumbers)
            {
                var emptyCells = GetEmptyCells();
                foreach (var cell in emptyCells)
                {
                    // Make the move
                    board[cell.row, cell.col] = num;
                    UsedNumbers.Add(num);

                    var score = Minimax(depth - 1, false, int.MinValue, int.MaxValue);

                    // Undo the move
                    board[cell.row, cell.col] = 0;
                    UsedNumbers.Remove(num);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = (num, cell.row, cell.col);
                    }
                }
            }

            return bestMove;
        }

        private int Minimax(int depth, bool isMaximizing, int alpha, int beta)
        {
            if (depth == 0 || IsGameOver())
            {
                return EvaluatePosition();
            }

            var validNumbers = Enumerable.Range(1, BoardSize * BoardSize)
                                  .Where(n => n % 2 == (isMaximizing == CurrentPlayer.IsOddPlayer ? 1 : 0)
                                           && !UsedNumbers.Contains(n))
                                  .ToList();

            if (!validNumbers.Any()) return EvaluatePosition();

            var emptyCells = GetEmptyCells();
            if (!emptyCells.Any()) return EvaluatePosition();

            if (isMaximizing)
            {
                var maxEval = int.MinValue;
                foreach (var num in validNumbers)
                {
                    foreach (var cell in emptyCells)
                    {
                        board[cell.row, cell.col] = num;
                        UsedNumbers.Add(num);

                        var eval = Minimax(depth - 1, false, alpha, beta);
                        maxEval = Math.Max(maxEval, eval);
                        alpha = Math.Max(alpha, eval);

                        board[cell.row, cell.col] = 0;
                        UsedNumbers.Remove(num);

                        if (beta <= alpha) break;
                    }
                    if (beta <= alpha) break;
                }
                return maxEval;
            }
            else
            {
                var minEval = int.MaxValue;
                foreach (var num in validNumbers)
                {
                    foreach (var cell in emptyCells)
                    {
                        board[cell.row, cell.col] = num;
                        UsedNumbers.Add(num);

                        var eval = Minimax(depth - 1, true, alpha, beta);
                        minEval = Math.Min(minEval, eval);
                        beta = Math.Min(beta, eval);

                        board[cell.row, cell.col] = 0;
                        UsedNumbers.Remove(num);

                        if (beta <= alpha) break;
                    }
                    if (beta <= alpha) break;
                }
                return minEval;
            }
        }

        private int EvaluatePosition()
        {
            if (CheckWinCondition())
            {
                return CurrentPlayer.IsOddPlayer ? 1000 : -1000;
            }

            int score = 0;

            // Evaluate all lines for potential
            for (int i = 0; i < BoardSize; i++)
            {
                score += EvaluateLine(i, true);  // Rows
                score += EvaluateLine(i, false); // Columns
            }

            // Evaluate diagonals
            score += EvaluateDiagonal(true);  // Main diagonal
            score += EvaluateDiagonal(false); // Anti-diagonal

            return score;
        }

        private int EvaluateLine(int index, bool isRow)
        {
            int sum = 0;
            int oddCount = 0;
            int evenCount = 0;
            int emptyCount = 0;

            for (int i = 0; i < BoardSize; i++)
            {
                int value = isRow ? board[index, i] : board[i, index];
                if (value == 0)
                {
                    emptyCount++;
                }
                else
                {
                    sum += value;
                    if (value % 2 == 1) oddCount++;
                    else evenCount++;
                }
            }

            return EvaluateLineScore(sum, oddCount, evenCount, emptyCount);
        }

        private int EvaluateDiagonal(bool isMain)
        {
            int sum = 0;
            int oddCount = 0;
            int evenCount = 0;
            int emptyCount = 0;

            for (int i = 0; i < BoardSize; i++)
            {
                int value = isMain ? board[i, i] : board[i, BoardSize - 1 - i];
                if (value == 0)
                {
                    emptyCount++;
                }
                else
                {
                    sum += value;
                    if (value % 2 == 1) oddCount++;
                    else evenCount++;
                }
            }

            return EvaluateLineScore(sum, oddCount, evenCount, emptyCount);
        }

        private int EvaluateLineScore(int sum, int oddCount, int evenCount, int emptyCount)
        {
            if (emptyCount == 0)
            {
                return sum == TargetSum ? 1000 : 0;
            }

            // If line has both odd and even numbers, it might be contested
            if (oddCount > 0 && evenCount > 0)
                return 0;

            int remainingSum = TargetSum - sum;

            // Check if it's possible to complete this line
            if (oddCount > 0) // Line has odd numbers
            {
                // Need more odd numbers to complete
                var availableOdds = Enumerable.Range(1, BoardSize * BoardSize)
                                       .Where(n => n % 2 == 1 && !UsedNumbers.Contains(n));
                if (CanCompleteWithNumbers(availableOdds, remainingSum, emptyCount))
                    return CurrentPlayer.IsOddPlayer ? 50 : -50;
            }
            else if (evenCount > 0) // Line has even numbers
            {
                // Need more even numbers to complete
                var availableEvens = Enumerable.Range(1, BoardSize * BoardSize)
                                        .Where(n => n % 2 == 0 && !UsedNumbers.Contains(n));
                if (CanCompleteWithNumbers(availableEvens, remainingSum, emptyCount))
                    return CurrentPlayer.IsOddPlayer ? -50 : 50;
            }

            return 0;
        }

        private bool CanCompleteWithNumbers(IEnumerable<int> availableNumbers, int targetSum, int slotsNeeded)
        {
            var numbers = availableNumbers.ToList();
            if (numbers.Count < slotsNeeded) return false;

            // Simple check: can we make the target sum with available numbers?
            var combinations = GetCombinations(numbers, slotsNeeded);
            return combinations.Any(combo => combo.Sum() == targetSum);
        }

        private IEnumerable<List<int>> GetCombinations(List<int> numbers, int length)
        {
            if (length == 1)
                return numbers.Select(n => new List<int> { n });

            return numbers.SelectMany((n, i) =>
                GetCombinations(numbers.Skip(i + 1).ToList(), length - 1)
                    .Select(combo => new List<int> { n }.Concat(combo).ToList()));
        }

        private List<(int row, int col)> GetEmptyCells()
        {
            var emptyCells = new List<(int row, int col)>();
            for (int r = 0; r < BoardSize; r++)
            {
                for (int c = 0; c < BoardSize; c++)
                {
                    if (board[r, c] == 0)
                        emptyCells.Add((r, c));
                }
            }
            return emptyCells;
        }

        private bool CheckWinCondition()
        {
            // Check rows and columns
            for (int i = 0; i < BoardSize; i++)
            {
                if (CheckLine(i, true) || CheckLine(i, false))
                    return true;
            }

            // Check diagonals
            return CheckDiagonal(true) || CheckDiagonal(false);
        }

        private bool CheckLine(int index, bool isRow)
        {
            int sum = 0;
            for (int i = 0; i < BoardSize; i++)
            {
                int value = isRow ? board[index, i] : board[i, index];
                if (value == 0) return false;
                sum += value;
            }
            return sum == TargetSum;
        }

        private bool CheckDiagonal(bool isMain)
        {
            int sum = 0;
            for (int i = 0; i < BoardSize; i++)
            {
                int value = isMain ? board[i, i] : board[i, BoardSize - 1 - i];
                if (value == 0) return false;
                sum += value;
            }
            return sum == TargetSum;
        }

        protected override void Undo()
        {
            var move = GameState.Undo();
            if (move == null)
            {
                ConsoleRenderer.ShowMessage("No moves to undo.", ConsoleColor.Yellow);
                return;
            }

            board[move.Row, move.Col] = 0;
            UsedNumbers.Remove((int)move.Value);
            ConsoleRenderer.ShowMessage("Undo successful.", ConsoleColor.Cyan);
            SwitchPlayers();
        }

        protected override void Redo()
        {
            var move = GameState.Redo();
            if (move == null)
            {
                ConsoleRenderer.ShowMessage("No moves to redo.", ConsoleColor.Yellow);
                return;
            }

            board[move.Row, move.Col] = (int)move.Value;
            UsedNumbers.Add((int)move.Value);
            ConsoleRenderer.ShowMessage("Redo successful.", ConsoleColor.Cyan);
            SwitchPlayers();
        }

        protected override int MaxRow => BoardSize - 1;
        protected override int MaxCol => BoardSize - 1;
    }
}