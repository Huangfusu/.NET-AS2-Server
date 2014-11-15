using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace AS2_SimulationServer
{
    class Settings
    {
        static int workerThreads;
        static int completionPortThread;

        static Settings()
        {
            System.Threading.ThreadPool.GetMinThreads(out workerThreads, out completionPortThread);
        }


        public static string Email
        { get; set; }

        public static string AS2From
        { get; set; }

        public static string AS2To
        { get; set; }        

        public static string URL
        { get; set; }

        public static bool IsAsync
        { get; set; }

        public static string ReceiptDeliveryOption
        {get; set;}

        public static int AvgMessagePerMin
        { get; set; }

        public static int MinMessagePerMin
        { get; set; }

        public static int MaxMessagePerMin
        { get; set; }

        public static bool Log
        { get; set; }

        public static string MinFolder
        { get; set; }

        public static string AvgFolder
        { get; set; }

        public static string MaxFolder
        { get; set; }

        public static X509Certificate2 EncryptionCertificate
        { get; set; }

        public static X509Certificate2 SigningCertificate
        { get; set; }

        public static int Duration
        { get; set; }

        public static string ExecutionPath
        { get; set; }

        public static int  Timeout
        {get;set;}

        public static string HttpURL
        { get; set; }

        public static string SslURL
        { get; set; }

        public static string IPAddress
        { get; set; }

        public static bool NoSQL
        { get; set; }

        public static int DefaultWorkerThreads
        {
            get
            {
                return workerThreads;
            }
        }

        public static int DefaultCompletionPortThread
        {
            get
            {
                return completionPortThread;
            }
        }       
    }
}
