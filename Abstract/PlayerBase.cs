using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.Abstract
{
    public abstract class PlayerBase
    {
        public string PlayerName { get; set; }
        public int PlayerNumber { get; set; }

        protected PlayerBase(string playerName, int playerNumber)
        {
            PlayerName = playerName;
            PlayerNumber = playerNumber;
        }

        public abstract (int Row, int Col, int Value) GetGameMove(int[,] board, List<int> availableNumbers);
    }
}
