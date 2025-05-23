using IFN584_ASS2.Core;
using IFN584_ASS2.Enums;
using IFN584_ASS2.UserUI;
using IFN584_ASS2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

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
            for (int i = 0; i < Size; i++)
            {
                Board[i] = new char[Size];
                for (int j = 0; j < Size; j++)
                    Board[i][j] = '.';
            }

            if (Player2 != null && !Player2.IsHuman)
                ComputerPlayer = Player2;

            ConsoleRenderer.RenderMessage("Gomoku (Five in a Row). Get 5 of your symbol in a row.");
        }

        protected override void DisplayBoard() => ConsoleRenderer.RenderBoard(Board, baseOne: true);

        protected override bool IsMoveNumberValid(int input) => true;

        protected override void MakeMove(int _)
        {
            Console.Write("Enter row (1-15): ");
            if (!int.TryParse(Console.ReadLine(), out int row) || row < 1 || row > Size)
            {
                ConsoleRenderer.ShowError("Invalid row.");
                return;
            }

            Console.Write("Enter col (1-15): ");
            if (!int.TryParse(Console.ReadLine(), out int col) || col < 1 || col > Size)
            {
                ConsoleRenderer.ShowError("Invalid column.");
                return;
            }

            row--; col--;

            if (Board[row][col] != '.')
            {
                ConsoleRenderer.ShowError("That spot is already taken.");
                return;
            }

            Board[row][col] = CurrentSymbol;
            GameState.RecordMove(new Move(row, col, CurrentSymbol));
        }

        protected override void MakeMoveWithCoords(int _, int row, int col)
        {
            row--; col--;
            if (Board[row][col] != '.')
                throw new Exception("That spot is already taken.");

            Board[row][col] = CurrentSymbol;
            GameState.RecordMove(new Move(row, col, CurrentSymbol));
        }

        protected override bool IsComputerTurn() =>
            GameMode == GameMode.HumanVsComputer && CurrentPlayer == ComputerPlayer;

        protected override void MakeComputerMove()
        {
            char ai = CurrentSymbol;
            char opponent = ai == 'X' ? 'O' : 'X';

            var move = GetBestMove(ai, opponent);

            if (move.HasValue)
            {
                var (r, c) = move.Value;
                Board[r][c] = ai;
                Console.WriteLine($"Computer placed {ai} at ({r + 1}, {c + 1})");
                GameState.RecordMove(new Move(r, c, ai));
            }
            else
            {
                Console.WriteLine("No valid moves left.");
            }
        }

        private (int row, int col)? GetBestMove(char ai, char opponent)
        {
            var winMove = FindWinningMove(ai);
            if (winMove.HasValue) return winMove;

            var blockMove = FindWinningMove(opponent);
            if (blockMove.HasValue) return blockMove;

            var forkMove = FindForkMove(ai);
            if (forkMove.HasValue) return forkMove;

            var blockForkMove = FindForkMove(opponent);
            if (blockForkMove.HasValue) return blockForkMove;

            var offensiveMove = FindBestOffensiveMove(ai, opponent);
            if (offensiveMove.HasValue) return offensiveMove;

            return GetMinimaxMove(ai, opponent, 3);
        }

        private (int row, int col)? FindWinningMove(char symbol)
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (Board[r][c] != '.') continue;

                    Board[r][c] = symbol;
                    bool isWin = CheckWinCondition(r, c, symbol);
                    Board[r][c] = '.';

                    if (isWin) return (r, c);
                }
            }
            return null;
        }

        private (int row, int col)? FindForkMove(char symbol)
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (Board[r][c] != '.') continue;

                    Board[r][c] = symbol;
                    int threatCount = CountThreats(symbol);
                    Board[r][c] = '.';

                    if (threatCount >= 2) return (r, c);
                }
            }
            return null;
        }

        private int CountThreats(char symbol)
        {
            int threats = 0;
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (Board[r][c] != '.') continue;

                    Board[r][c] = symbol;
                    if (CheckWinCondition(r, c, symbol)) threats++;
                    Board[r][c] = '.';
                }
            }
            return threats;
        }

        private (int row, int col)? FindBestOffensiveMove(char ai, char opponent)
        {
            var moves = new List<(int row, int col, int score)>();

            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (Board[r][c] != '.') continue;
                    if (!IsNearExistingPiece(r, c)) continue; 

                    int score = EvaluatePosition(r, c, ai, opponent);
                    moves.Add((r, c, score));
                }
            }

            if (moves.Count == 0) return (Size / 2, Size / 2); 

            var bestMove = moves.OrderByDescending(m => m.score).First();
            return (bestMove.row, bestMove.col);
        }

        private bool IsNearExistingPiece(int r, int c)
        {
            for (int dr = -2; dr <= 2; dr++)
            {
                for (int dc = -2; dc <= 2; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int nr = r + dr, nc = c + dc;
                    if (nr >= 0 && nc >= 0 && nr < Size && nc < Size && Board[nr][nc] != '.')
                        return true;
                }
            }
            return false;
        }

        private int EvaluatePosition(int r, int c, char ai, char opponent)
        {
            int score = 0;

            
            var directions = new[] { (1, 0), (0, 1), (1, 1), (1, -1) };

            foreach (var (dr, dc) in directions)
            {
                score += EvaluateDirection(r, c, dr, dc, ai, true);   // Offensive
                score -= EvaluateDirection(r, c, dr, dc, opponent, false) / 2; // Defensive
            }

        
            score += GetPositionalBonus(r, c);

            return score;
        }

        private int EvaluateDirection(int r, int c, int dr, int dc, char symbol, bool isOffensive)
        {
            int totalScore = 0;

            // Check both directions from the position
            int leftCount = CountInDirection(r, c, -dr, -dc, symbol);
            int rightCount = CountInDirection(r, c, dr, dc, symbol);
            int totalCount = leftCount + rightCount;

            if (totalCount == 0) return 0;

            // Check if the line is blocked
            bool leftBlocked = IsBlocked(r - (leftCount + 1) * dr, c - (leftCount + 1) * dc, symbol);
            bool rightBlocked = IsBlocked(r + (rightCount + 1) * dr, c + (rightCount + 1) * dc, symbol);

            // Score based on pattern strength
            if (totalCount >= 4) totalScore += isOffensive ? 100000 : 50000; // Four in a row
            else if (totalCount == 3)
            {
                if (!leftBlocked && !rightBlocked) totalScore += isOffensive ? 10000 : 5000; // Open three
                else if (!leftBlocked || !rightBlocked) totalScore += isOffensive ? 1000 : 500; // Semi-open three
            }
            else if (totalCount == 2)
            {
                if (!leftBlocked && !rightBlocked) totalScore += isOffensive ? 500 : 250; // Open two
                else if (!leftBlocked || !rightBlocked) totalScore += isOffensive ? 100 : 50; // Semi-open two
            }
            else if (totalCount == 1)
            {
                if (!leftBlocked && !rightBlocked) totalScore += isOffensive ? 50 : 25; // Open one
            }

            return totalScore;
        }

        private int CountInDirection(int r, int c, int dr, int dc, char symbol)
        {
            int count = 0;
            int nr = r + dr, nc = c + dc;

            while (nr >= 0 && nc >= 0 && nr < Size && nc < Size && Board[nr][nc] == symbol)
            {
                count++;
                nr += dr;
                nc += dc;
            }

            return count;
        }

        private bool IsBlocked(int r, int c, char symbol)
        {
            if (r < 0 || c < 0 || r >= Size || c >= Size) return true; 
            char cell = Board[r][c];
            return cell != '.' && cell != symbol; 
        }

        private int GetPositionalBonus(int r, int c)
        {
            int center = Size / 2;
            int distanceFromCenter = Math.Abs(center - r) + Math.Abs(center - c);
            return Math.Max(0, 50 - distanceFromCenter * 5); // Prefer center positions
        }

        private (int row, int col)? GetMinimaxMove(char ai, char opponent, int depth)
        {
            var candidateMoves = GetCandidateMoves().Take(10).ToList(); // Limit search space

            if (candidateMoves.Count == 0) return null;

            var bestMove = candidateMoves[0];
            int bestScore = int.MinValue;

            foreach (var (r, c) in candidateMoves)
            {
                Board[r][c] = ai;
                int score = Minimax(depth - 1, false, ai, opponent, int.MinValue, int.MaxValue);
                Board[r][c] = '.';

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = (r, c);
                }
            }

            return bestMove;
        }

        private int Minimax(int depth, bool isMaximizing, char ai, char opponent, int alpha, int beta)
        {
            if (depth == 0 || IsGameOver())
            {
                return EvaluateBoardState(ai, opponent);
            }

            var moves = GetCandidateMoves().Take(8).ToList(); 

            if (isMaximizing)
            {
                int maxScore = int.MinValue;
                foreach (var (r, c) in moves)
                {
                    Board[r][c] = ai;
                    int score = Minimax(depth - 1, false, ai, opponent, alpha, beta);
                    Board[r][c] = '.';

                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha) break; 
                }
                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                foreach (var (r, c) in moves)
                {
                    Board[r][c] = opponent;
                    int score = Minimax(depth - 1, true, ai, opponent, alpha, beta);
                    Board[r][c] = '.';

                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, score);
                    if (beta <= alpha) break; 
                }
                return minScore;
            }
        }

        private List<(int row, int col)> GetCandidateMoves()
        {
            var moves = new List<(int row, int col, int priority)>();

            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (Board[r][c] != '.') continue;
                    if (!IsNearExistingPiece(r, c)) continue;

                    int priority = GetMovePriority(r, c);
                    moves.Add((r, c, priority));
                }
            }

            return moves.OrderByDescending(m => m.priority)
                       .Select(m => (m.row, m.col))
                       .ToList();
        }

        private int GetMovePriority(int r, int c)
        {
            int priority = 0;

            // Check proximity to existing pieces
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int nr = r + dr, nc = c + dc;
                    if (nr >= 0 && nc >= 0 && nr < Size && nc < Size && Board[nr][nc] != '.')
                        priority += 10;
                }
            }

            return priority;
        }

        private int EvaluateBoardState(char ai, char opponent)
        {
            int score = 0;

            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (Board[r][c] == ai)
                        score += EvaluatePosition(r, c, ai, opponent);
                    else if (Board[r][c] == opponent)
                        score -= EvaluatePosition(r, c, opponent, ai);
                }
            }

            return score;
        }

        private bool CheckWinCondition(int r, int c, char symbol)
        {
            var directions = new[] { (1, 0), (0, 1), (1, 1), (1, -1) };

            foreach (var (dr, dc) in directions)
            {
                int count = 1 + CountInDirection(r, c, dr, dc, symbol) + CountInDirection(r, c, -dr, -dc, symbol);
                if (count >= 5) return true;
            }

            return false;
        }

        private bool CheckDirection(int r, int c, int dr, int dc, char sym)
        {
            int count = 1;
            for (int i = 1; i < 5; i++)
            {
                int nr = r + i * dr, nc = c + i * dc;
                if (nr < 0 || nc < 0 || nr >= Size || nc >= Size || Board[nr][nc] != sym)
                    break;
                count++;
            }
            return count == 5;
        }

        protected override bool IsGameOver()
        {
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                {
                    char sym = Board[r][c];
                    if (sym == '.') continue;
                    if (CheckWinCondition(r, c, sym))
                        return true;
                }
            return false;
        }

        protected override void AnnounceResult() =>
            ConsoleRenderer.RenderMessage($"{CurrentPlayer.Name} wins with 5 in a row!", ConsoleColor.Green);

        protected override void ShowHelp() => HelpProvider.ShowHelp("gomoku");

        protected override void SaveGame() => FileManager.Save(this);

        protected override void Undo()
        {
            var move = GameState.Undo();
            if (move != null)
            {
                Board[move.Row][move.Col] = '.';
                LastCommandWasUtility = true;
                ConsoleRenderer.ShowMessage("Undo successful.", ConsoleColor.Cyan);
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
                Board[move.Row][move.Col] = (char)move.Value;
                LastCommandWasUtility = false;
                ConsoleRenderer.ShowMessage("Redo successful.", ConsoleColor.Cyan);
            }
            else
            {
                ConsoleRenderer.ShowMessage("No moves to redo.", ConsoleColor.Yellow);
            }
        }

        protected override int MaxRow => 15;
        protected override int MaxCol => 15;
    }
}