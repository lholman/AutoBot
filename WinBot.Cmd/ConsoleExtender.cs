using System;

namespace WinBot.Cmd
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

            if (text.StartsWith("@{") && text.EndsWith("}"))
            {
                //assume we are passing a PowerShell hash table result
                text = text.Substring(2, text.Length - 3);
                string[] rows = text.Split(';');

                foreach (var row in rows)
                {
                    Console.WriteLine(row.Trim(), args);
                }
            }
            else
                Console.WriteLine(text);
           
            Console.ForegroundColor = oldColour;
        }
    }
}
