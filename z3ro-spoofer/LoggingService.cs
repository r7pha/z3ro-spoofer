using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace z3ro_spoofer
{
    public class LoggingService
    {
        /*
         * 1:info
         * 2:warn
         * 3:error
        */
        public static void Log(string Message, int Level = 1, bool SaveToFile = true)
        {
            switch (Level)
            {
                case 1:
                    write("[INFO]", ConsoleColor.Black, ConsoleColor.Blue);
                    write(" "+Message, ConsoleColor.Gray, ConsoleColor.Black,true);
                    Console.WriteLine();
                    break;
                case 2:
                    write("[WARNING]", ConsoleColor.Black, ConsoleColor.Yellow);
                    write(" " + Message, ConsoleColor.Gray, ConsoleColor.Black, true);
                    Console.WriteLine();
                    break;
                case 3:
                    write("[ERROR]", ConsoleColor.Black, ConsoleColor.Red);
                    write(" " + Message, ConsoleColor.Gray, ConsoleColor.Black, true);
                    Console.WriteLine();
                    break;
            }
        }
        public static void Divider(string Text = "")
        {
            write(new string('=', 10), ConsoleColor.DarkGray, ConsoleColor.Black);
            Console.Write(" "); write(Text.ToUpper(), ConsoleColor.Gray, ConsoleColor.Black); Console.Write(" ");
            write(new string('=', 10), ConsoleColor.DarkGray, ConsoleColor.Black);
            Console.WriteLine();
        }
        private static void write(string text, ConsoleColor? fg = null, ConsoleColor? bg = null,bool newline=true)
        {
            var oldFg = Console.ForegroundColor;
            var oldBg = Console.BackgroundColor;

            if (fg.HasValue) Console.ForegroundColor = fg.Value;
            if (bg.HasValue) Console.BackgroundColor = bg.Value;

            Console.Write(text);

            Console.ForegroundColor = oldFg;
            Console.BackgroundColor = oldBg;
        }

    }
}
