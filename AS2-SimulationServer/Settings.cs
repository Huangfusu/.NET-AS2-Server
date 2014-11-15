using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace AS2_SimulationServer
{
    class Settings
    {
        static int minWorkerThreads;
        static int minCompletionPortThread;
        static int maxWorkerThreads;
        static int maxCompletionPortThread;


        static Settings()
        {
            System.Threading.ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThread);
            System.Threading.ThreadPool.GetMaxThreads(out maxWorkerThreads, out maxCompletionPortThread);
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

        public static int DefaultMinWorkerThreads
        {
            get
            {
                return minWorkerThreads;
            }
        }

        public static int DefaultMinCompletionPortThread
        {
            get
            {
                return minCompletionPortThread;
            }
        }

        public static int DefaultMaxWorkerThreads
        {
            get
            {
                return maxWorkerThreads;
            }
        }

        public static int DefaultMaxCompletionPortThread
        {
            get
            {
                return maxCompletionPortThread;
            }
        }

        public static string BasePort
        {
            get;
            set;
        }

        public static string SSLBasePort
        { get; set; }

        public static bool LogToFile
        {
            get;
            set;
        }

        public static string LogPath
        {
            get;
            set;
        }

        public static string AssemblyPath
        {
            get;
            set;
        }

        public static int Mins
        {
            get;
            set;
        }

        public static bool IsAS2Default
        {
            get;
            set;
        }

        public static string Subject
        {
            get;
            set;
        }

        public static string FileName
        {
            get;
            set;
        }

        public static string ConnectionString
        {
            get;
            set;
        }
    }
}
