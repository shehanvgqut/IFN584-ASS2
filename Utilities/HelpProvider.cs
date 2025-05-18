using IFN584_ASS2.UserUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFN584_ASS2.Utilities
{
    public static class HelpProvider
    {
        public static void ShowHelp(string gameName)
        {
            ConsoleRenderer.RenderMessage("🆘 Available Commands:");

            switch (gameName.ToLower())
            {
                case "numerical":
                    ConsoleRenderer.RenderMessage("- Input a number between 1 and 9 (e.g. 5)");
                    break;

                case "notakto":
                    ConsoleRenderer.RenderMessage("- Input board, row, column (e.g. 1,1,2)");
                    break;

                case "gomoku":
                    ConsoleRenderer.RenderMessage("- You'll be prompted to enter row and column separately (0–14)");
                    break;

                default:
                    ConsoleRenderer.RenderMessage("- Game-specific help not found.");
                    break;
            }

            ConsoleRenderer.RenderMessage("- Special commands:");
            ConsoleRenderer.RenderMessage("  • undo   – Undo the last move");
            ConsoleRenderer.RenderMessage("  • redo   – Redo the last undone move");
            ConsoleRenderer.RenderMessage("  • save   – Save the current game");
            ConsoleRenderer.RenderMessage("  • help   – Show this help menu again");
        }
    }
}
