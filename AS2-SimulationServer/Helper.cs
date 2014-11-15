using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intel.SBI.Benchmarking.Utilities;

namespace AS2_SimulationServer
{
    class Helper
    {
        private static DateTime _currentTime;
        private static bool reset;
        
        public Helper()
        {
           
        }
        public static void CaluculateMaxResponse()
        {
          var max  =MessageCounter.collection.Select(item => item.Value.EndTime.Subtract(item.Value.StartTime)).Max();

          var min = MessageCounter.collection.Select(item => item.Value.EndTime.Subtract(item.Value.StartTime)).Min();               

          FormatServerResponse.DisplayMessage("Max Response Time -" +max.ToString("h'h: 'm'm: 's's'"));
          FormatServerResponse.DisplayMessage("Min Response Time -" + min.ToString("h'h: 'm'm: 's's'"));

          
                        
        }
        public static IUtilities GetInboundLogger()
        {
            Type newType = Type.GetType(Settings.AssemblyPath);

            if (newType == null)
                throw new ApplicationException("Unable to load the assembly - " + Settings.AssemblyPath);

            return (Settings.IsAS2Default)? (IUtilities) Activator.CreateInstance(newType, Settings.ConnectionString,
                "<-->", "I") : (IUtilities)Activator.CreateInstance(newType, Settings.ConnectionString,"I",false);
        }

        public static void Start()
        {
            _currentTime = DateTime.Now;
            reset = false;
        }

        public static double  ElapsedMilliseconds
        {
            get
            {
                if (reset)
                    return 0;
                else
                    return (DateTime.Now - _currentTime).TotalMilliseconds;

            }
        }

        public static void Reset()
        {
            reset = true;
        }

        public static TimeSpan Elapsed()
        {
            if (reset)
                    return new TimeSpan(0);
                else
                    return (DateTime.Now - _currentTime);

        }

    }
}
