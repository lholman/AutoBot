using System;

namespace AutoBot.Cmd
{
    public static class ConsoleExtender
    {
        public static void Error(string text, params object[] args)
        {
            ColourText(ConsoleColor.Red, text, args);
        }

        public static void Info(string text, params object[] args)
        {
            ColourText(ConsoleColor.Yellow, text, args);
        }

        public static void Write(string text, params object[] args)
        {
            ColourText(Console.ForegroundColor, text, args);
        }

        public static void ColourText(ConsoleColor colour, string text, params object[] args)
        {
            ConsoleColor oldColour = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(text, args);
            Console.ForegroundColor = oldColour;
        }
    }
}
