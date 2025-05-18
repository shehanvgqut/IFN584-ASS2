using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.Utilities
{
    public class HelpMenu
    {
        public static void Show()
        {
            Console.WriteLine("🆘 HELP MENU:");
            Console.WriteLine("- Type numbers to play your move.");
            Console.WriteLine("- 'undo': Undo the last move.");
            Console.WriteLine("- 'redo': Redo an undone move.");
            Console.WriteLine("- 'save': Save the game.");
            Console.WriteLine("- 'help': Show this help menu.");
        }
    }
}
