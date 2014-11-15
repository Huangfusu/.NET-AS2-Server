using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
namespace AS2_SimulationServer
{
    class MessageCounter
    {
        private static int _avgMessageSentCounter;     

        private static int _maxMessageSentCounter;       

        private static int _minMessageSentCounter;        

        private static int _asyncMessageRcvCounter;      

        private static int noConcurrentConnection;

        private static int noThreads;

        public static ConcurrentDictionary<string,DataStruct> collection=new ConcurrentDictionary<string,DataStruct>();

       
        public static int IncrementAvgMessageSent()
        {
            System.Threading.Interlocked.Increment(ref _avgMessageSentCounter);
                return _avgMessageSentCounter;

            
        }

        public static int IncrementMaxMessageSent()
        {

            System.Threading.Interlocked.Increment(ref _maxMessageSentCounter);                
            return _maxMessageSentCounter;
            
        }

        public static int IncrementMinMessageSent()
        {
            
               System.Threading.Interlocked.Increment(ref  _minMessageSentCounter);
                return _minMessageSentCounter;
        }

        public static int IncrementAsyncMessageRcv()
        {
            
                System.Threading.Interlocked.Increment(ref _asyncMessageRcvCounter);
                return _asyncMessageRcvCounter;

            
        }

        public static void Reset()
        {
            _avgMessageSentCounter = 0;
            _minMessageSentCounter = 0;
            _maxMessageSentCounter = 0;
            _asyncMessageRcvCounter = 0;            
            noConcurrentConnection = 0;
            collection.Clear();
        }

        public static void IncrementConnection()
        {
            System.Threading.Interlocked.Increment(ref noConcurrentConnection);
        }

        public static void DecrementConnection()
        {
            System.Threading.Interlocked.Decrement(ref noConcurrentConnection);
        }

        public static int ConcurrentConnection
        {
            get
            {
                return noConcurrentConnection;
            }
        }

       /* public static void IncrementThread()
        {
            System.Threading.Interlocked.Increment(ref noThreads);
        }

       public static void DecrementThread()
        {
            System.Threading.Interlocked.Decrement(ref noThreads);
        }

        public static int ActiveThreads()
        {
            return noThreads;
        }
        */
        public static int TotalMessages()
        {
            return (_minMessageSentCounter + _avgMessageSentCounter +_maxMessageSentCounter);
        }
    }
}
