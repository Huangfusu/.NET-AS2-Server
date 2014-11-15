using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Web;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.Net;

namespace AS2_SimulationServer
{
    public class MDNSend
    {

        public MemoryStream SyncMDNSend(ref OutgoingWebResponseContext response,PropogationContext context)
        {
            X509Certificate2 cert = Settings.SigningCertificate;
            DateTime dt = DateTime.Now;
            
            String divider1= "=Part" + dt.ToString("_dd_HHmmss.ffffff");
            String divider2 = "=Part" + dt.ToString("_HH_ddmmss.ffffff");

           
            response.ContentType = "multipart/signed;protocol=\"application/pkcs7-signature\";micalg=sha1;boundary=\""+divider1+"\"";
           
            response.Headers.Add("Message-Id", "<" + Guid.NewGuid().ToString() + "@" + Settings.AS2From + ">");

            response.Headers.Add("Subject", "Signed Message Disposition Notification");
            
            response.Headers.Add("Mime-Version", "1.0");
            response.Headers.Add("AS2-Version", "1.2");
            response.Headers.Add("AS2-From", Settings.AS2From);
            response.Headers.Add("AS2-To", Settings.AS2To);

            StringBuilder part1 = new StringBuilder();            
            StringBuilder part5 = new StringBuilder();

       

            part1.Append("Content-Type: multipart/report;Report-Type=disposition-notification;boundary=\""+divider2+"\"");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--"+divider2+"\r\n");
            part1.Append("\r\n");
            part1.Append("Your message was successfully received and processed.");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--"+divider2+"\r\n");
            part1.Append("Content-Type: message/disposition-notification");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("Original-Recipient: rfc822;"+Settings.AS2From);
            part1.Append("\r\n");
            part1.Append("Final-Recipient: rfc822;"+Settings.AS2From);
            part1.Append("\r\n");
            part1.Append(String.Format("Original-Message-ID: {0}", context.OrginalMessageID));
            part1.Append("\r\n");
            part1.Append(String.Format("Received-Content-MIC: {0},sha1", context.MIC));
            part1.Append("\r\n");
            part1.Append("Disposition: Automatic-action/mdn-sent-automatically;processed");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--"+divider2+"--\r\n");
            part1.Append("\r\n");
           

            part5.Append("--"+divider1+"\r\n" 
                   +part1.ToString()
                   + "\r\n"
                   + "--"+divider1+"\r\n" 
                   +"Content-Type: application/pkcs7-signature; name= smime.p7s; smime-type=signed-data" + "\r\n"
                   + "Content-Disposition: attachment; filename=\"smime.p7s\"" + "\r\n"
                   + "Content-Transfer-Encoding: base64\r\n"
                   + "\r\n"
                   + Certificates.SignDetached(Encoding.Default.GetBytes(part1.ToString()),cert)
                   + "\r\n"
                   + "--"+divider1+"--") ;

            if (Settings.Log)
            File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\SyncMDN" + DateTime.Now.ToString("_dd_HHmmss.ffffff") + ".txt", part5.ToString(), Encoding.UTF8);

            return new MemoryStream(Encoding.Default.GetBytes(part5.ToString()));

        }

        public void ASyncMDNSend(PropogationContext context)
        {

            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            FormatServerResponse.AsyncDisplaySuccessMessage("Begin Async send for ID-"+context.OrginalMessageID);

            X509Certificate2 cert = Settings.SigningCertificate;
            DateTime dt = DateTime.Now;

            String divider1 = "=Part" + dt.ToString("_dd_HHmmss.ffffff");
            String divider2 = "=Part" + dt.ToString("_HH_ddmmss.ffffff");

            HttpWebRequest request = WebRequest.Create(context.URL) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "multipart/signed;protocol=\"application/pkcs7-signature\";micalg=sha1;boundary=\""+divider1+"\"";

            request.Headers.Add("Message-Id", "<" + Guid.NewGuid().ToString() + "@" + Settings.AS2From+ ">");

            request.Headers.Add("Subject", "Signed Message Disposition Notification");

            request.Headers.Add("Mime-Version", "1.0");
            request.Headers.Add("AS2-Version", "1.2");
            request.Headers.Add("AS2-From", Settings.AS2From);
            request.Headers.Add("AS2-To", Settings.AS2To);

            StringBuilder part1 = new StringBuilder();
            
            StringBuilder part5 = new StringBuilder();


            part1.Append("Content-Type: multipart/report;Report-Type=disposition-notification;boundary=\""+divider2+"\"");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--"+divider2+"\r\n");
            part1.Append("\r\n");
            part1.Append("Your message was successfully received and processed.");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--"+divider2+"\r\n");
            part1.Append("Content-Type: message/disposition-notification");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("Original-Recipient: rfc822;"+Settings.AS2From);
            part1.Append("\r\n");
            part1.Append("Final-Recipient: rfc822;"+Settings.AS2From);
            part1.Append("\r\n");
            part1.Append(String.Format("Original-Message-ID: {0}", context.OrginalMessageID));
            part1.Append("\r\n");
            part1.Append(String.Format("Received-Content-MIC: {0},sha1", context.MIC));
            part1.Append("\r\n");
            part1.Append("Disposition: Automatic-action/mdn-sent-automatically;processed");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--"+divider2+"--\r\n");
            part1.Append("\r\n");
          
            part5.Append("--"+divider1+"\r\n"
                   + part1.ToString()
                   + "\r\n"
                   + "--"+divider1+"\r\n"
                   + "Content-Type: application/pkcs7-signature; name= smime.p7s; smime-type=signed-data" + "\r\n"
                   + "Content-Disposition: attachment; filename=\"smime.p7s\"" + "\r\n"
                   + "Content-Transfer-Encoding: base64\r\n"
                   + "\r\n"
                   + Certificates.SignDetached(Encoding.Default.GetBytes(part1.ToString()), cert)
                   + "\r\n"
                   + "--"+divider1+"--");

            byte[] byteData = Encoding.Default.GetBytes(part5.ToString());
            

           
            Stream sw = request.GetRequestStream();           
            sw.Write(byteData, 0, byteData.Length);

            sw.Close();


            using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
            {
                StringBuilder response = new StringBuilder();

                foreach (string key in resp.Headers.Keys)
                {
                    response.AppendLine(String.Format("{0}:{1}", key, resp.Headers[key]));
                }


                StreamReader sr = new StreamReader(resp.GetResponseStream());

                string _responseText = sr.ReadToEnd();
                response.Append(Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(_responseText)));

                if (Settings.Log)
                    File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\Asyncmdn"
                        + dt.ToString("_dd_HHmmss.ffffff") + ".txt", _responseText, Encoding.UTF8);

                sr.Close();
                resp.Close();
            }

           

            FormatServerResponse.AsyncDisplaySuccessMessage("Completed  Async send for ID-" + context.OrginalMessageID);
           

        }
        public static string GenerateMIC(string content)
        {
            HashAlgorithm hash = new SHA1Managed();

            return Convert.ToBase64String(hash.ComputeHash(Encoding.Default.GetBytes(content)));
        }

        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, 
            System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }


    }
}
