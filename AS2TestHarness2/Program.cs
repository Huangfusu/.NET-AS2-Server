using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;

namespace AS2TestHarness2
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpWebRequest req = WebRequest.Create("http://vmsdasbi02.amr.corp.intel.com:30033/as2") as HttpWebRequest;
            DateTime dt = DateTime.Now;
            X509Certificate2 cert = GetCertificateFromStore("E4B37177BF164945B02186405AF85D67946DC24E");

            req.Method = "POST";
            req.ContentType = "application/pkcs7-mime; smime-type=enveloped-data; name=\"smime.p7m\"";
            req.UserAgent = "Sampler.EDI.AS2";
            req.Headers.Add("Message-Id", "<" + Guid.NewGuid().ToString() + "@" + "mycompanyAS2" + ">");
            req.Headers.Add("From", "as2@company.com");
            req.Headers.Add("Subject", "AS2 message");

            req.Headers.Add("Mime-Version", "1.0");
            req.Headers.Add("AS2-Version", "1.2");
            req.Headers.Add("AS2-From", "mycompanyAS2");
            req.Headers.Add("AS2-To", "Intel");
            //req.Headers.Add("Content-Transfer-Encoding", "base64");
            req.Headers.Add("Content-Disposition", "attachment; filename=\"smime.p7m\"");
            req.Headers.Add("Disposition-notification-To", "http://www.company.com:8080/as2/HttpReceiver");
            req.Headers.Add("Disposition-notification-options", "signed-receipt-protocol=optional,pkcs7-signature;signed-receipt-micalg=optional,sha1");

            //String divider = "5_13649987.1349781493703";
            String divider = "=Part"+dt.ToString("_dd_HHmmss.ffffff"); 

            StringBuilder sendData = new StringBuilder();
            sendData.Append("MIME-Version: 1.0\r\n");
            sendData.Append("Content-Type: multipart/signed; protocol=\"application/pkcs7-signature\"; micalg=sha1; boundary=\"" + divider + "\"\r\n");
            sendData.Append("\r\n");
            StringBuilder strHeader=new StringBuilder();
            Console.WriteLine("Create MIME Content --->" + Environment.NewLine);

            strHeader.Append(String.Format(
                    "Content-Type: {0}; name=\"{1}\"\r\n"
                    + "Content-Transfer-Encoding: binary\r\n"
                    + "Content-Disposition: attachment; filename=\"{2}\"\r\n"
                    + "\r\n",
                    "application/EDI-Consent",
                    "EDI.txt",
                    "EDI.txt" ));

            foreach (string file in Directory.GetFiles(@"C:\Users\rmd\Documents\Sterling Documents\Sample"))
            {
                FileInfo fileInfo = new FileInfo(file);
                strHeader.Append(String.Format("{0}"
                    + "\r\n",                    
                    File.ReadAllText(file)
                    ));
               
               
            }
            string strData = "--" + divider + "\r\n"
                   + strHeader + "\r\n"
                   + "--" + divider + "\r\n"
                   + "Content-Type: application/pkcs7-signature; name= smime.p7s; smime-type=signed-data" + "\r\n"
                   + "Content-Disposition: attachment; filename=\"smime.p7s\"" + "\r\n"
                   + "Content-Transfer-Encoding: base64\r\n"
                   + "\r\n"
                   + SignDetached(Encoding.Default.GetBytes(strHeader.ToString()), cert)
                   + "\r\n";
            sendData.Append(strData);

            sendData.Append("--" + divider + "--");

            StringBuilder request = new StringBuilder();

            foreach (string key in req.Headers.Keys)
            {
                request.AppendLine(String.Format("{0}:{1}", key, req.Headers[key]));
            }
            request.Append(sendData);
            File.WriteAllBytes(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\request"+dt.ToString("_dd_HHmmss.ffffff")+".txt", Encoding.Default.GetBytes(request.ToString()));


            Console.WriteLine("Log Payload before Encryption --->"+Environment.NewLine);

            //Console.WriteLine(sendData.ToString() + Environment.NewLine);        

          

            Console.WriteLine("Begin Payload  Encryption --->" + Environment.NewLine);
          
            byte[] byteData = Encrypt(Encoding.Default.GetBytes(sendData.ToString()), cert);

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
            response.Append(Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(_responseText)));
            
            File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\mdn_actually" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", _responseText,Encoding.UTF8);
            
            sr.Close();
            if (_responseText.Contains("Your message was successfully received and processed."))
            {
                Console.WriteLine("---> State:Success"+Environment.NewLine);
            }
            else
            {
                Console.WriteLine("---> State:Error CHECK mdn" + dt.ToString("_dd_HHmmss.ffffff") + Environment.NewLine);
            }


            File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\mdn" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", response.ToString());
            Console.WriteLine("____________________________________________");
            Console.WriteLine();
            Console.WriteLine(" Completed process, Press any Key to exit");
            Console.WriteLine("____________________________________________ ");

            Console.ReadLine();
        }



        private static string SignDetached(byte[] data, X509Certificate2 signingCert)
        {

            
            ContentInfo content = new ContentInfo(data);

            SignedCms signedMessage = new SignedCms(content,true);            

            CmsSigner signer = new CmsSigner(signingCert);
            
           
            signedMessage.ComputeSignature(signer);
            byte[] signedBytes = signedMessage.Encode();


            return Convert.ToBase64String(signedBytes);
        }

        private static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {

            
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            Console.WriteLine("Certificate Fetch Start -->"+Environment.NewLine);
            try
            {
                store.Open(OpenFlags.ReadOnly);

                
                X509Certificate2Collection certCollection = store.Certificates;
               
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                
               
                 

                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindByThumbprint,thumbprint, false);
                if (signingCert.Count == 0)

                    return null;
                
                return signingCert[0];
            }
            finally
            {
                store.Close();
            }

        }

        private static byte[] Encrypt(byte[] data, X509Certificate2 encryptingCert)
        {
            
            ContentInfo plainContent = new ContentInfo(data);

            
            EnvelopedCms encryptedData = new EnvelopedCms(plainContent, new AlgorithmIdentifier(new Oid("3DES")));

            
            CmsRecipient recipient = new CmsRecipient(encryptingCert);

            
        
            encryptedData.Encrypt(recipient);

         
            byte[] encryptedBytes = encryptedData.Encode();

            
            return encryptedBytes;
        }

        

    }
}
