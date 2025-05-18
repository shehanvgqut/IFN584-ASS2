using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.Core
{
    public class GameState
    {
        private Stack<Move> undoStack = new();
        private Stack<Move> redoStack = new();

        public void RecordMove(Move move)
        {
            undoStack.Push(move);
            redoStack.Clear();
        }

        public Move? Undo()
        {
            if (undoStack.Count == 0) return null;
            Move move = undoStack.Pop();
            redoStack.Push(move);
            return move;
        }

        public Move? Redo()
        {
            if (redoStack.Count == 0) return null;
            Move move = redoStack.Pop();
            undoStack.Push(move);
            return move;
        }

        public IEnumerable<Move> GetMoveHistory() => undoStack;
    }
}
