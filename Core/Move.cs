using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.Core
{
    public class Move
    {
        public int Row { get; }
        public int Col { get; }
        public int Value { get; }

        public Move(int row, int col, int value)
        {
            Row = row;
            Col = col;
            Value = value;
        }
    }
}
