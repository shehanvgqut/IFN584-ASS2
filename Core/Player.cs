using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.Core
{
    public class Player
    {
        public string Name { get; }
        public bool IsOddPlayer { get; }

        public Player(string name, bool isOddPlayer)
        {
            Name = name;
            IsOddPlayer = isOddPlayer;
        }
    }
}
