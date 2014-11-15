using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;


namespace AS2_SimulationServer
{
    class SendAsyncMDN
    {

        private static ConcurrentQueue<PropogationContext> queue = new ConcurrentQueue<PropogationContext>();


         static SendAsyncMDN()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerAsync();
        }
         private static void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
         {
             PropogationContext context;

             while (true)
             {
                 while (queue.TryDequeue(out context))
                 {
                     try
                     {
                         MDNSend generateMDN = new MDNSend();
                         generateMDN.ASyncMDNSend(context);
                     }
                     catch (Exception ex)
                     {
                         FormatServerResponse.AsyncDisplayErrorMessage(ex.Message);
                     }
                 }                
                 
                 System.Threading.Thread.Sleep(2000);
             }
         }

         public static void AsyncSend(PropogationContext context)
         {
             queue.Enqueue(context);
         }
    }
}
