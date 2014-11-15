using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

namespace AS2_SimulationServer
{
    
    class Program
    {
        
        static void Main(string[] args)
        {
            Intializer();
        }

        
        private static void RunSettings()
        {
            Application.SetCompatibleTextRenderingDefault(false);            
            SettingsForm settings = new SettingsForm();
            Application.EnableVisualStyles();
            Application.Run(new SettingsForm());
        }

        private static void Intializer()
        {
             FormatServerResponse.DisplayMessage("Copyright © 2012 Intel Corporation. All rights reserved \n  Internal Use Only - Do Not Distribute");           
           
            ServiceHost host = new ServiceHost(typeof(AS2_SimulationServer.HTTPServer));
            host.Open();
            FormatServerResponse.DisplayServiceStart();

            Settings.BasePort = host.BaseAddresses[0].Port.ToString().Trim();
            Settings.SSLBasePort = host.BaseAddresses[1].Port.ToString().Trim();

            System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
           
            FormatServerResponse.DisplayClientStart();           
            
           
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();
            Settings.ReceiptDeliveryOption = "http://" + hostName + "." + domainName + ":" + Settings.BasePort + "/Intel";
            
            Settings.SslURL = "https://" + hostName + "." + domainName + ":" + Settings.SSLBasePort + "/Intel";
            Settings.HttpURL = "http://" + hostName + "." + domainName + ":" + Settings.BasePort + "/Intel";
            Settings.IPAddress = "0.0.0.0";                         
            FormatServerResponse.DisplayMessage("SSL url - " + Settings.SslURL);
            FormatServerResponse.DisplayMessage("HTTP url - " + Settings.HttpURL);
            Settings.ExecutionPath =System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            FormatServerResponse.DisplayMessage("Execution Path - " +Settings.ExecutionPath);

            Settings.LogPath = Settings.ExecutionPath.Replace(@"file:\", "") + @"\log.txt";

            Thread thread = new Thread(new ThreadStart(RunSettings));
            
            thread.TrySetApartmentState(ApartmentState.STA);
            thread.Start();

            FormatServerResponse.DisplayServiceStop();
            Console.ReadLine();
            if(Certificates.CheckSSL())
            Certificates.DeleteSSL();
        }
    }
}
