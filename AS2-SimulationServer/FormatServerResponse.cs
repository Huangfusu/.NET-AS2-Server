using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AS2_SimulationServer
{
    class FormatServerResponse
    {
        static int index;
        static string format = "\n{0} {1}.";

        public static void DisplayServiceStart()
        {
            index++;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(String.Format(format,">","Successfully started the HTTP AS2 Server"));
        }
        public static void DisplayServiceStop()
        {
            index++;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(String.Format(format, ">", "Press any key to exit HTTP AS2 Server"));
        }

        public static void DisplayMessage(string message)
        {
            index++;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(String.Format(format, ">", message));
        }

        public static void AsyncDisplayMessage(string message)
        {
            AsyncConsole.AsyncWriteLine(String.Format(format, ">", message), ConsoleColor.White);
            
        }
       

        public static void DisplayClientStart()
        {
            index++;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(String.Format(format, ">", "Starting EDI Client..."));
        }
        public static void DisplaySuccessMessage(string msg)
        {
            index++;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(String.Format(format, ">", msg));
        }
        public static void AsyncDisplaySuccessMessage(string msg)
        {
            AsyncConsole.AsyncWriteLine(String.Format(format, ">", msg), ConsoleColor.Green);

        }
        
        public static void DisplayErrorMessage(string msg)
        {
            index++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(String.Format(format, ">", msg));
        }

        public static void AsyncDisplayErrorMessage(string msg)
        {
            
            AsyncConsole.AsyncWriteLine(String.Format(format, ">", msg), ConsoleColor.Red);
        }
        
    }
}
