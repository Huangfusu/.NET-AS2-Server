using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;


namespace AS2_SimulationServer
{
    class Logger
    {

        private static ConcurrentQueue<string> queue = new ConcurrentQueue<string>();


        static Logger()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerAsync();
        }
         private static void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
         {
             string line;

         

             while (true)
             {
                


                 using (var fs = (System.IO.File.Exists(Settings.LogPath)) ? new System.IO.FileStream (Settings.LogPath,System.IO.FileMode.Append)
                     :new System.IO.FileStream(Settings.LogPath,System.IO.FileMode.Create))
                 {
                     var writer = new System.IO.StreamWriter(fs);

                     while (queue.TryDequeue(out line))
                     {
                         try
                         {
                             writer.WriteLine(line);
                         }
                         catch (Exception ex)
                         {
                             FormatServerResponse.AsyncDisplayErrorMessage(ex.Message);
                         }
                     }
                     writer.Close();
                 }
                 
                 System.Threading.Thread.Sleep(2000);
             }
         }

         public static void Log(string line)
         {
             queue.Enqueue(line);
         }
    }
}
