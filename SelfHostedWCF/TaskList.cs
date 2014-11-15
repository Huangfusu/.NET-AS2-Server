using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SelfHostedWCF
{
    class   TaskList
    {
        private static ConcurrentDictionary<string,Task> dictionary= new ConcurrentDictionary<string,Task>();

        public static void TaskRemove(string messageID)
        {
            Task any;
            dictionary.TryRemove(messageID, out any);
        }

        public static void TaskAdd(string messageID,Task current)
        {

            dictionary.TryAdd(messageID, current); ;
        }

        public static Task TaskGet(string messageID)
        {
            Task any;
            dictionary.TryGetValue(messageID,out any);
            return any;
        }
    }
}
