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

namespace AS2MDNSend
{
    public class MDNSend
    {

        public MemoryStream SyncMDNSend(ref OutgoingWebResponseContext response,string orginalMessageID,string MIC)
        {
            X509Certificate2 cert = GetCertificateFromStore("E4B37177BF164945B02186405AF85D67946DC24E");
            DateTime dt = DateTime.Now;
            
            String divider = "=Part" + dt.ToString("_dd_HHmmss.ffffff");

           
            response.ContentType = "multipart/signed;protocol=\"application/pkcs7-signature\";micalg=sha1;boundary=\"_=4702482448752663Sterling4702482448752663MOKO\"";
           
            response.Headers.Add("Message-Id", "<" + Guid.NewGuid().ToString() + "@" + "mycompanyAS2" + ">");

            response.Headers.Add("Subject", "Signed Message Disposition Notification");
            
            response.Headers.Add("Mime-Version", "1.0");
            response.Headers.Add("AS2-Version", "1.2");
            response.Headers.Add("AS2-From", "mycompanyAS2");
            response.Headers.Add("AS2-To", "Intel");

            StringBuilder part1 = new StringBuilder();
            StringBuilder part2 = new StringBuilder();
            StringBuilder part3 = new StringBuilder();
            StringBuilder part4 = new StringBuilder();
            StringBuilder part5 = new StringBuilder();

        /*    part1.Append("Content-Type: multipart/report;Report-Type=disposition-notification;boundary=\"_=572886430326638Sterling572886430326638MOKO\"");
            part1.Append("\r\n");
            part1.Append("\r\n");
           
            part2.Append("\r\n");
            part2.Append("Your message was successfully received and processed.");
            part2.Append("\r\n");
            part2.Append("\r\n");

            part3.Append("Content-Type: message/disposition-notification");
            part3.Append("\r\n");
            part3.Append("\r\n");
            part3.Append("Original-Recipient: rfc822;mycompanyAS2");
            part3.Append("\r\n");
            part3.Append("Final-Recipient: rfc822;mycompanyAS2");
            part3.Append("\r\n");
            part3.Append(String.Format("Original-Message-ID: {0}",orginalMessageID));            
            part3.Append("\r\n");
            part3.Append(String.Format("Received-Content-MIC: {0},sha1",MIC));
            part3.Append("\r\n");
            part3.Append("Disposition: Automatic-action/mdn-sent-automatically;processed");
            part3.Append("\r\n");
            part3.Append("\r\n");

            part4.Append("\r\n");
            part4.Append("\r\n");

         * */

            part1.Append("Content-Type: multipart/report;Report-Type=disposition-notification;boundary=\"_=572886430326638Sterling572886430326638MOKO\"");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--_=572886430326638Sterling572886430326638MOKO\r\n");
            part1.Append("\r\n");
            part1.Append("Your message was successfully received and processed.");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--_=572886430326638Sterling572886430326638MOKO\r\n");
            part1.Append("Content-Type: message/disposition-notification");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("Original-Recipient: rfc822;mycompanyAS2");
            part1.Append("\r\n");
            part1.Append("Final-Recipient: rfc822;mycompanyAS2");
            part1.Append("\r\n");
            part1.Append(String.Format("Original-Message-ID: {0}", orginalMessageID));
            part1.Append("\r\n");
            part1.Append(String.Format("Received-Content-MIC: {0},sha1", MIC));
            part1.Append("\r\n");
            part1.Append("Disposition: Automatic-action/mdn-sent-automatically;processed");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--_=572886430326638Sterling572886430326638MOKO--\r\n");
           part1.Append("\r\n");
           // part1.Append("\r\n");

            part5.Append("--_=4702482448752663Sterling4702482448752663MOKO\r\n" 
                   +part1.ToString()
                   + "\r\n"
                   + "--_=4702482448752663Sterling4702482448752663MOKO\r\n" 
                   +"Content-Type: application/pkcs7-signature; name= smime.p7s; smime-type=signed-data" + "\r\n"
                   + "Content-Disposition: attachment; filename=\"smime.p7s\"" + "\r\n"
                   + "Content-Transfer-Encoding: base64\r\n"
                   + "\r\n"
                   + SignDetached(Encoding.Default.GetBytes(part1.ToString()),cert)
                   + "\r\n"
                   + "--_=4702482448752663Sterling4702482448752663MOKO--") ;


            //part5.Append("Content-Type: application/pkcs7-signature; name= smime.p7s; smime-type=signed-data" + "\r\n"
            //       + "Content-Disposition: attachment; filename=\"smime.p7s\"" + "\r\n"
            //       + "Content-Transfer-Encoding: base64\r\n"
            //       + "\r\n"
            //       + SignDetached(Encoding.Default.GetBytes(part1.ToString() 
            //       + part2.ToString() + part3.ToString() 
            //       + part4.ToString()), cert)
            //       + "\r\n");

            //string data = "--_=4702482448752663Sterling4702482448752663MOKO\r\n" +
            //    part1.ToString()
            //    + "--_=572886430326638Sterling572886430326638MOKO\r\n"
            //       + part2.ToString()
            //    + "--_=572886430326638Sterling572886430326638MOKO\r\n"
            //       + part3.ToString()
            //   + "--_=572886430326638Sterling572886430326638MOKO--\r\n"
            //       + part4.ToString()
            //    +"--_=4702482448752663Sterling4702482448752663MOKO\r\n" 
            //       + part5.ToString()
            //       + "--_=4702482448752663Sterling4702482448752663MOKO--";

            File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\mdn" + DateTime.Now.ToString("_dd_HHmmss.ffffff") + ".txt", part5.ToString(), Encoding.UTF8);

            return new MemoryStream(Encoding.Default.GetBytes(part5.ToString()));

        }

        public void ASyncMDNSend(string url, string orginalMessageID, string MIC)
        {
            

            Console.WriteLine("Begin Async send =>");

            X509Certificate2 cert = GetCertificateFromStore("E4B37177BF164945B02186405AF85D67946DC24E");
            DateTime dt = DateTime.Now;

            String divider = "=Part" + dt.ToString("_dd_HHmmss.ffffff");

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "multipart/signed;protocol=\"application/pkcs7-signature\";micalg=sha1;boundary=\"_=4702482448752663Sterling4702482448752663MOKO\"";

            request.Headers.Add("Message-Id", "<" + Guid.NewGuid().ToString() + "@" + "mycompanyAS2" + ">");

            request.Headers.Add("Subject", "Signed Message Disposition Notification");

            request.Headers.Add("Mime-Version", "1.0");
            request.Headers.Add("AS2-Version", "1.2");
            request.Headers.Add("AS2-From", "mycompanyAS2");
            request.Headers.Add("AS2-To", "Intel");

            StringBuilder part1 = new StringBuilder();
            
            StringBuilder part5 = new StringBuilder();


            part1.Append("Content-Type: multipart/report;Report-Type=disposition-notification;boundary=\"_=572886430326638Sterling572886430326638MOKO\"");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--_=572886430326638Sterling572886430326638MOKO\r\n");
            part1.Append("\r\n");
            part1.Append("Your message was successfully received and processed.");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--_=572886430326638Sterling572886430326638MOKO\r\n");
            part1.Append("Content-Type: message/disposition-notification");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("Original-Recipient: rfc822;mycompanyAS2");
            part1.Append("\r\n");
            part1.Append("Final-Recipient: rfc822;mycompanyAS2");
            part1.Append("\r\n");
            part1.Append(String.Format("Original-Message-ID: {0}", orginalMessageID));
            part1.Append("\r\n");
            part1.Append(String.Format("Received-Content-MIC: {0},sha1", MIC));
            part1.Append("\r\n");
            part1.Append("Disposition: Automatic-action/mdn-sent-automatically;processed");
            part1.Append("\r\n");
            part1.Append("\r\n");
            part1.Append("--_=572886430326638Sterling572886430326638MOKO--\r\n");
            part1.Append("\r\n");
          
            part5.Append("--_=4702482448752663Sterling4702482448752663MOKO\r\n"
                   + part1.ToString()
                   + "\r\n"
                   + "--_=4702482448752663Sterling4702482448752663MOKO\r\n"
                   + "Content-Type: application/pkcs7-signature; name= smime.p7s; smime-type=signed-data" + "\r\n"
                   + "Content-Disposition: attachment; filename=\"smime.p7s\"" + "\r\n"
                   + "Content-Transfer-Encoding: base64\r\n"
                   + "\r\n"
                   + SignDetached(Encoding.Default.GetBytes(part1.ToString()), cert)
                   + "\r\n"
                   + "--_=4702482448752663Sterling4702482448752663MOKO--");

            byte[] byteData = Encoding.Default.GetBytes(part5.ToString());
            File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\Asyncmdn" + DateTime.Now.ToString("_dd_HHmmss.ffffff") + ".txt", part5.ToString(), Encoding.UTF8);

            Console.WriteLine("Begin Fetch stream --->" + Environment.NewLine);
            Stream sw = request.GetRequestStream();
            Console.WriteLine("Begin write  stream --->" + Environment.NewLine);
            sw.Write(byteData, 0, byteData.Length);

            sw.Close();

            StringBuilder response = new StringBuilder();
            Console.WriteLine("--->Waiting for response" + Environment.NewLine);
            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();

            foreach (string key in resp.Headers.Keys)
            {
                response.AppendLine(String.Format("{0}:{1}", key, resp.Headers[key]));
            }

            StreamReader sr = new StreamReader(resp.GetResponseStream());

            string _responseText = sr.ReadToEnd();
            response.Append(Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(_responseText)));

            File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\Asyncmdn_actually" 
                + dt.ToString("_dd_HHmmss.ffffff") + ".txt", _responseText, Encoding.UTF8);

            sr.Close();
            Console.WriteLine("Begin Async send complete =>");

        }


        private static string SignDetached(byte[] data, X509Certificate2 signingCert)
        {


            ContentInfo content = new ContentInfo(data);

            SignedCms signedMessage = new SignedCms(content, true);

            CmsSigner signer = new CmsSigner(signingCert);


            signedMessage.ComputeSignature(signer);
            byte[] signedBytes = signedMessage.Encode();


            return Convert.ToBase64String(signedBytes);
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

        public static string GenerateMIC(string content)
        {
            HashAlgorithm hash = new SHA1Managed();

            return Convert.ToBase64String(hash.ComputeHash(Encoding.Default.GetBytes(content)));
        }

    }
}
