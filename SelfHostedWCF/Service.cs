using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel.Web;
using System.Security;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Threading.Tasks;

namespace SelfHostedWCF
{
    class Service:IService
    {
        public Stream POST(Stream  as2Message)
        {
            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;



            WebHeaderCollection collection = request.Headers;          

            Console.WriteLine("-----> Message received");

            DateTime dt = DateTime.Now;
           

            try
            {

                

                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = as2Message.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                     buffer= ms.ToArray();
                }

                EnvelopedCms cms = new EnvelopedCms();
                
                cms.Decode(buffer);               
                
               cms.Decrypt();
               string content= Encoding.UTF8.GetString(cms.ContentInfo.Content);
                
                StringReader stringReader = new StringReader(content);
                stringReader.ReadLine();
                string line = stringReader.ReadLine();
                string[] split = line.Split('"');
                string divider = split[2];
                stringReader.ReadLine();
                stringReader.ReadLine();
                StringBuilder builder = new StringBuilder();
                builder.Append(line = stringReader.ReadLine());
                while ((line = stringReader.ReadLine()) != null)
                {
                    if (line.Contains("--" + divider))
                        break;
                    else
                    builder.Append("\r\n"+line);
                }

                string data = builder.ToString();

              var response =  WebOperationContext.Current.OutgoingResponse;

             string mic =AS2MDNSend.MDNSend.GenerateMIC(data);
             string messageID = collection["Message-ID"] as string;
             AS2MDNSend.MDNSend generateMDN = new AS2MDNSend.MDNSend();

             

              StringBuilder fullResponse = new StringBuilder();

              foreach (string key in collection.Keys)
              {
                  fullResponse.AppendLine(String.Format("{0}:{1}", key, collection[key]));
              }

              fullResponse.Append(Encoding.UTF8.GetString(cms.ContentInfo.Content));

             File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\FromSterlingPOSTDeCRYPTED" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", Encoding.UTF8.GetString(cms.ContentInfo.Content), Encoding.UTF8);

             File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\FromSterlingPOSTDeCRYPTEDFull" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", fullResponse.ToString(), Encoding.UTF8);
             File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\ExtractedContent" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", data , Encoding.UTF8);
             //return generateMDN.SyncMDNSend(ref response, messageID, mic);

             Task task= Task.Factory.StartNew(() => generateMDN.ASyncMDNSend("http://vmsdasbi02.amr.corp.intel.com:30033/as2",messageID,mic));

             TaskList.TaskAdd("async_" + messageID, task);

             response.StatusDescription = "EDI Message was received";
             return null;
            
            }
            catch (Exception)
            {
                StreamReader reader = new StreamReader(as2Message, true);
                StringBuilder fullResponse = new StringBuilder();

                foreach (string key in collection.Keys)
                {
                    fullResponse.AppendLine(String.Format("{0}:{1}", key, collection[key]));
                }

                fullResponse.Append(reader.ReadToEnd());
                File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\ExceptionSterlingPOST" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", fullResponse.ToString(), Encoding.UTF8);
            }
            return null;
          
        }
        public Stream GET()
        {
            string output =
 "<HTML>" +
    "<HEAD>" +
       "<TITLE>" +
          "AS2 EDI Service" +
      "</TITLE>" +
    "</HEAD>" +
 "<BODY>" +
    "<H1>Hi</H1>" +
    "<P>This is for AS2 EDI Post</P> " +
"</BODY>" +
 "</HTML>";

            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(output));

           OutgoingWebResponseContext response =WebOperationContext.Current.OutgoingResponse;

           response.ContentType = "text/html";
            return stream;
        }

        private static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {


            X509Store store = new X509Store(StoreLocation.CurrentUser);
            Console.WriteLine("Certificate Fetch Start -->" + Environment.NewLine);
            try
            {
                store.Open(OpenFlags.ReadOnly);


                X509Certificate2Collection certCollection = store.Certificates;

                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);




                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (signingCert.Count == 0)

                    return null;

                return signingCert[0];
            }
            finally
            {
                store.Close();
            }

        }
    }
}
