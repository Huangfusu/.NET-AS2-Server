using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace AS2MDNHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpWebRequest req = WebRequest.Create("http://vmsdasbi02.amr.corp.intel.com:30033/as2") as HttpWebRequest;
            DateTime dt = DateTime.Now;
            X509Certificate2 cert = GetCertificateFromStore("E4B37177BF164945B02186405AF85D67946DC24E");
            String divider = "=Part" + dt.ToString("_dd_HHmmss.ffffff"); 

            req.Method = "POST";
            req.ContentType = "multipart/signed;protocol=\"application/pkcs7-signature\";micalg=sha1;boundary=\"_=8873530361393734Sterling8873530361393734MOKO\"";
            req.UserAgent = "Sampler.EDI.AS2";
            req.Headers.Add("Message-Id", "<" + Guid.NewGuid().ToString() + "@" + "mycompanyAS2" + ">");
            
            req.Headers.Add("Subject", "Signed Message Disposition Notification");

            req.Headers.Add("Mime-Version", "1.0");
            req.Headers.Add("AS2-Version", "1.2");
            req.Headers.Add("AS2-From", "mycompanyAS2");
            req.Headers.Add("AS2-To", "Intel");
            

            //String divider = "5_13649987.1349781493703";

            StringBuilder content= new StringBuilder();
           
            content.Append("Content-Type: multipart/report;Report-Type=disposition-notification;boundary=\"_=14745086776574212Sterling14745086776574212MOKO\"");
            content.Append("\r\n");
            content.Append( "\r\n");
            content.Append("--_=14745086776574212Sterling14745086776574212MOKO");
            content.Append("\r\n");
            content.Append("\r\n");
            content.Append("Your message was successfully received and processed.");
            content.Append("\r\n");
            content.Append("\r\n");
            content.Append("--_=14745086776574212Sterling14745086776574212MOKO");
            content.Append("\r\n");
            content.Append("Content-Type: message/disposition-notification");
            content.Append("\r\n");
            content.Append("\r\n");
            content.Append("Original-Recipient: rfc822;Intel");
            content.Append("\r\n");
            content.Append("Final-Recipient: rfc822;Intel");
            content.Append("\r\n");
            content.Append("Original-Message-ID: <115dfd49-293c-434e-a5bf-8f8e1ae44d8f@mycompanyAS2>");
            content.Append("\r\n");
            content.Append("Received-Content-MIC: qaa5se5e/w8/OCCqBdYf/wVzR60=,sha1");
            content.Append("\r\n");
            content.Append("Disposition: Automatic-action/mdn-sent-automatically;processed");
            content.Append("\r\n");
            content.Append("\r\n");
            content.Append("\r\n");
            content.Append("--_=14745086776574212Sterling14745086776574212MOKO");
            content.Append("\r\n");



            String sendData = String.Format("--_=8873530361393734Sterling8873530361393734MOKO"
                + "\r\n"
                + "{0}"
                + "--_=8873530361393734Sterling8873530361393734MOKO"
                + "\r\n"
                + "Content-Type: Application/pkcs7-signature;name=EDIINTSIG.p7s"
                + "\r\n"
                + "Content-Transfer-Encoding: base64"
                + "\r\n"
                + "\r\n"
                + "{1}"
                + "\r\n"
                + "--_=8873530361393734Sterling8873530361393734MOKO--",
                content.ToString(),
                SignDetached(Encoding.Default.GetBytes(content.ToString()), cert));

            StringBuilder request = new StringBuilder();
            foreach (string key in req.Headers.Keys)
            {
                request.AppendLine(String.Format("{0}:{1}", key, req.Headers[key]));
            }
            request.Append(sendData);
            File.WriteAllBytes(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\request_mdn" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", Encoding.Default.GetBytes(request.ToString()));

            byte[] byteData = Encoding.Default.GetBytes(sendData.ToString());

            Console.WriteLine("Begin Fetch stream --->" + Environment.NewLine);
            Stream sw = req.GetRequestStream();
            Console.WriteLine("Begin write  stream --->" + Environment.NewLine);
            sw.Write(byteData, 0, byteData.Length);

            sw.Close();

            StringBuilder response = new StringBuilder();
            Console.WriteLine("--->Waiting for response" + Environment.NewLine);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            foreach (string key in resp.Headers.Keys)
            {
                response.AppendLine(String.Format("{0}:{1}", key, resp.Headers[key]));
            }

            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string _responseText = sr.ReadToEnd();
            response.Append(_responseText);
            ;


            sr.Close();
            if (_responseText.Contains("Your message was successfully received and processed."))
            {
                Console.WriteLine("---> State:Success" + Environment.NewLine);
            }
            else
            {
                Console.WriteLine("---> State:Error CHECK mdn" + dt.ToString("_dd_HHmmss.ffffff") + Environment.NewLine);
            }


            File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\respnse2MDN" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", response.ToString());
            Console.WriteLine("____________________________________________");
            Console.WriteLine();
            Console.WriteLine(" Completed process, Press any Key to exit");
            Console.WriteLine("____________________________________________ ");

            Console.ReadLine();
                   
        }


        private static string SignDetached(byte[] data, X509Certificate2 signingCert)
        {


            ContentInfo content = new ContentInfo(data);

            SignedCms signedMessage = new SignedCms();

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

        private static string GenerateMIC(byte[] content)
        {
            HashAlgorithm hash = new SHA1Managed();

            return Convert.ToBase64String(hash.ComputeHash(content));
        }

    }
}
