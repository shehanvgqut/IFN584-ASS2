using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.Abstract
{
    public class HumanPlayer : PlayerBase
    {
        public HumanPlayer(string playerName, int playerNumber) : base(playerName, playerNumber) { }

        public override (int Row, int Col, int Value) GetGameMove(int[,] board, List<int> availableNumbers)
        {
            while (true)
            {
                Console.Write("Enter move as row,col,value (e.g., 1,2,7): ");
                var input = Console.ReadLine()?.Split(',');

                if (input?.Length == 3 &&
                    int.TryParse(input[0], out int row) &&
                    int.TryParse(input[1], out int col) &&
                    int.TryParse(input[2], out int value))
                {
                    return (row - 1, col - 1, value); // -1 for 0-based indexing
                }

                Console.WriteLine("Invalid input. Please enter three comma-separated numbers.");
            }
        }
    }
}
