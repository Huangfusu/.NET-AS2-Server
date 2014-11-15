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
using System.ServiceModel;
using System.Threading;


namespace AS2_SimulationServer
{
    [ServiceBehavior(
        UseSynchronizationContext = false,
        InstanceContextMode= InstanceContextMode.PerCall,
        ConcurrencyMode= ConcurrencyMode.Multiple
        )]
    class HTTPServer:IHTTPServer
    {
        public Stream POST(Stream as2Message)
        {
            try
            {
                IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                WebHeaderCollection collection = request.Headers;

                if (collection["Subject"].Contains("Signed Message Disposition Notification"))
                {
                    FormatServerResponse.AsyncDisplaySuccessMessage("Received Async MDN - " + MessageCounter.IncrementAsyncMessageRcv());

                    var mdn = new StreamReader(as2Message);
                    string lookfor = "Original-Message-ID:";
                    string line = string.Empty;
                    while (!mdn.EndOfStream)
                    {
                        if ((line = mdn.ReadLine()).StartsWith(lookfor))
                        {
                            line = line.Substring(lookfor.Length).Trim();
                            FormatServerResponse.AsyncDisplaySuccessMessage(line);
                            break;
                        }


                    }

                    if (MessageCounter.collection.ContainsKey(line))
                    {
                        var data = MessageCounter.collection[line];
                        data.EndTime = DateTime.Now;

                        MessageCounter.collection.AddOrUpdate(line, data, (key, oldvalue) => data);
                    }
                    return null;
                }

                FormatServerResponse.AsyncDisplayMessage("Received EDI message");
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
                        buffer = ms.ToArray();
                    }

                    string content = DecryptMessage(buffer);

                    StringReader stringReader = new StringReader(content);
                    stringReader.ReadLine();
                    string divider = stringReader.ReadLine().Split('"')[2];
                    stringReader.ReadLine();
                    stringReader.ReadLine();

                    StringBuilder builder = new StringBuilder();
                    string line = string.Empty;
                    builder.Append(line = stringReader.ReadLine());
                    while ((line = stringReader.ReadLine()) != null)
                    {
                        if (line.Contains("--" + divider))
                            break;
                        else
                            builder.Append("\r\n" + line);
                    }

                    string data = builder.ToString();
                    var response = WebOperationContext.Current.OutgoingResponse;
                    PropogationContext context = new PropogationContext();

                    context.MIC = MDNSend.GenerateMIC(data);
                    context.OrginalMessageID = collection["Message-ID"] as string;
                    MDNSend generateMDN = new MDNSend();
                    if (collection.AllKeys.Contains("Receipt-Delivery-Option"))
                    {
                        context.URL = collection.Get("Receipt-Delivery-Option");
                        response.StatusDescription = "EDI Message was received";

                        SendAsyncMDN.AsyncSend(context);
                        if(!Settings.IsAS2Default)
                        Helper.GetInboundLogger().LogEndTime(data, DateTime.Now);

                        return null;
                    }
                    else
                    {
                        FormatServerResponse.AsyncDisplaySuccessMessage("Sending Sync MDN");
                        if (!Settings.IsAS2Default)
                        Helper.GetInboundLogger().LogEndTime(data, DateTime.Now);
                        return generateMDN.SyncMDNSend(ref response, context);
                    }

                }
                catch (Exception ex)
                {
                    FormatServerResponse.AsyncDisplayErrorMessage(ex.Message);
                }
                return null;
            }
            catch (Exception ex)
            {
                FormatServerResponse.DisplayErrorMessage(ex.Message);
                return null;
            }
        }

        private static string DecryptMessage(byte[] buffer)
        {
            EnvelopedCms cms = new EnvelopedCms();
            cms.Decode(buffer);
            cms.Decrypt();
            return Encoding.UTF8.GetString(cms.ContentInfo.Content);
        }
        public Stream GET()
        {
            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;

            FormatServerResponse.AsyncDisplayMessage("GET request served to User agent " + request.UserAgent);

            string output =
                "<HTML>" +
                    "<HEAD>" +
                    "<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" />"+
                        "<TITLE>" +
                            "AS2 EDI Trading Partner Simulator" +
                        "</TITLE>" +
                    "</HEAD>" +
                    "<BODY>" +
                    
                        "<H1>AS2 EDI Trading Partner Simulator</H1>" +
                            "<P>"+
                                "This tool simulates TP commuincation over AS2 Protocol and generates Sync and Async MDNs as response."+
                            "</P> " +
                            "<H3>Copyright © 2012 Intel Corporation. All rights reserved</H3>"+
                             "<H4>Internal Use Only - Do Not Distribute</H4>"+
                    "</BODY>" +
                "</HTML>";

            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(output));
            OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
            response.ContentType = "text/html";
            return stream;

        }


    /*    private static void SendAsyncMDN(Object context)
        {
            MDNSend generateMDN = new MDNSend();
            generateMDN.ASyncMDNSend(context as PropogationContext);
        }
     * */
    }
}
