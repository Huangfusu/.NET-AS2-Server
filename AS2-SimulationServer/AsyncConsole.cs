using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace AS2_SimulationServer
{
    class AsyncConsole
    {
        private static ConcurrentQueue<ConsoleMessage> queue = new ConcurrentQueue<ConsoleMessage>();
        

        static AsyncConsole()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerAsync();
        }

        

        public static  void AsyncWriteLine(string message, ConsoleColor color)
        {
            ConsoleMessage thisMessage;
            thisMessage.message = message;
            thisMessage.color = color;
            queue.Enqueue(thisMessage);

            
        }

        private static void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ConsoleMessage message;
            while (true)
            {
                while (true)
                {
                   /* if (!ConsoleWrite())
                        break;
                    **/

                    if (queue.TryDequeue(out message))
                    {
                        Console.ForegroundColor = message.color;
                        Console.WriteLine(message.message);
                    }
                    else
                        break;

                }

                System.Threading.Thread.Sleep(500);
            }
        }

        private static  bool ConsoleWrite()
        {
            ConsoleMessage message;
            bool result = queue.TryDequeue(out message);
            if (result)
            {
                Console.ForegroundColor = message.color;
                Console.WriteLine(message.message);
            }

            return result;
        }

        private struct ConsoleMessage
        {
            public string message;

            public ConsoleColor color;
        }

    }


   

}
