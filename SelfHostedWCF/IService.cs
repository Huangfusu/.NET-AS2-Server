using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;

namespace SelfHostedWCF
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/Intel", Method = "POST")]
        Stream POST(Stream text);


        [OperationContract]
        [WebInvoke(UriTemplate = "/Intel", Method = "GET")]
        Stream GET();

        
    }
}
