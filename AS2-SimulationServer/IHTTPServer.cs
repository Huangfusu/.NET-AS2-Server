using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.IO;

namespace AS2_SimulationServer
{
    [ServiceContract]
    interface IHTTPServer
    {

        [OperationContract]
        [WebInvoke(UriTemplate = "/Intel", Method = "POST")]
        Stream POST(Stream text);


        [OperationContract]
        [WebInvoke(UriTemplate = "/Intel", Method = "GET")]
        Stream GET();
    }
}
