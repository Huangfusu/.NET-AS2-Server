using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SelfHostedWCF
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a ServiceHost for the CaseStudyService type.
            System.ServiceModel.ServiceHost serviceHost = new
                System.ServiceModel.ServiceHost(typeof(SelfHostedWCF.Service));


            // Open the ServiceHost to create listeners and start listening for messages.
            serviceHost.Open();
            Console.WriteLine("Services are ready & running.");
            OpenSSL();
            Console.WriteLine();
            Console.ReadLine();
            DeleteSSL();
        }

        private static void OpenSSL()
        {
            DeleteSSL();
           
            string arguments = "http add sslcert ipport=127.0.0.1:9003 certhash=e4b37177bf164945b02186405af85d67946dc24e appid={00112233-4455-6677-8899-AABBCCDDEEFF} ";
            ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", arguments);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            var process=Process.Start(procStartInfo);

            process.WaitForExit();

            Console.WriteLine(process.StandardOutput.ReadToEnd());
           

        }

        private static void DeleteSSL()
        {
            string arguments = "http delete sslcert ipport=127.0.0.1:9003";
            ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", arguments);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            var process = Process.Start(procStartInfo);

            process.WaitForExit();

            //Console.WriteLine(process.StandardOutput.ReadToEnd());


        }
    }
}
