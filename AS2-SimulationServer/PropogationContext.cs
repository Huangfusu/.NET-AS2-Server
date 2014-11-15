using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace AS2_SimulationServer
{
    public class PropogationContext
    {
        public String OrginalMessageID
        { get; set; }

        public String MIC
        { get; set; }

        public String URL
        { get; set; }

        public String Folder
        { get; set; }

       


    }
}
