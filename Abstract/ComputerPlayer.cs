using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.Abstract
{
    public class ComputerPlayer : PlayerBase
    {
        private readonly Random _random = new();

        public ComputerPlayer(string playerName, int playerNumber) : base(playerName, playerNumber) { }

        public override (int Row, int Col, int Value) GetGameMove(int[,] board, List<int> availableNumbers)
        {
            int size = board.GetLength(0);

            foreach (var number in availableNumbers)
            {
                for (int r = 0; r < size; r++)
                {
                    for (int c = 0; c < size; c++)
                    {
                        if (board[r, c] == 0)
                        {
                            board[r, c] = number;
                            if (CheckWinningMove(board, size))
                            {
                                board[r, c] = 0;
                                return (r, c, number);
                            }
                            board[r, c] = 0;
                        }
                    }
                }
            }

            while (true)
            {
                int row = _random.Next(size);
                int col = _random.Next(size);
                if (board[row, col] == 0)
                {
                    int value = availableNumbers[_random.Next(availableNumbers.Count)];
                    return (row, col, value);
                }
            }
        }

        private bool CheckWinningMove(int[,] board, int size)
        {
            int target = size * (size * size + 1) / 2;

            for (int i = 0; i < size; i++)
            {
                if (Enumerable.Range(0, size).Sum(j => board[i, j]) == target ||
                    Enumerable.Range(0, size).Sum(j => board[j, i]) == target)
                    return true;
            }

            int diag1 = 0, diag2 = 0;
            for (int i = 0; i < size; i++)
            {
                diag1 += board[i, i];
                diag2 += board[i, size - 1 - i];
            }

            return diag1 == target || diag2 == target;
        }
    }
}
