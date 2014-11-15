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
            X509Certificate2 cert = GetCertificateFromStore("E4B37177BF164945B02186405AF85D67946DC24E");

            req.Method = "POST";
            req.ContentType = "application/pkcs7-mime; smime-type=enveloped-data; name=\"smime.p7m\"";
            req.UserAgent = "Sampler.EDI.AS2";
            req.Headers.Add("Message-Id", "<" + Guid.NewGuid().ToString() + " @ " + "mycompanyAS2" + ">");
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

            String divider = "5_13649987.1349781493703";

            StringBuilder sendData = new StringBuilder();
            sendData.Append("MIME-Version: 1.0\r\n");
            sendData.Append("Content-Type: multipart/signed; protocol=\"application/pkcs7-signature\"; micalg=sha1; boundary=\"Part" + divider + "\"\r\n");
            sendData.Append("\r\n");
            foreach (string file in Directory.GetFiles(@"C:\Users\rmd\Documents\Sterling Documents\Sample"))
            {
                FileInfo fileInfo = new FileInfo(file);
                string strHeader = String.Format(
                    "Content-Type: {0}; name=\"{1}\"\r\n"
                    + "Content-Transfer-Encoding: binary\r\n"
                    + "Content-Disposition: attachment; filename=\"{2}\"\r\n"
                    + "\r\n"
                    + "{3}"
                    +"\r\n",
                    "application/EDI-Consent",
                    fileInfo.Name,
                    fileInfo.Name,
                    File.ReadAllText(file)
                    );

                string strData = "--Part" + divider + "\r\n"
                    + strHeader + "\r\n"
                    + "--Part" + divider + "\r\n"
                    + "Content-Type: application/pkcs7-signature; name= smime.p7s; smime-type=signed-data" + "\r\n"
                    + "Content-Disposition: attachment; filename=\"smime.p7s\"" + "\r\n"
                    + "Content-Transfer-Encoding: base64\r\n"
                    + "\r\n"
                    + SignDetached(Encoding.Default.GetBytes(strHeader),cert)
                    +"\r\n";
                sendData.Append(strData);
            }

            sendData.Append("--Part" + divider + "--");

            File.WriteAllBytes(@"C:\Users\rmd\Documents\Sterling Documents\Sample\request.txt", Encoding.Default.GetBytes(sendData.ToString()));


            Console.WriteLine("<---Payload before Encryption --->"+Environment.NewLine);

            Console.WriteLine(sendData.ToString() + Environment.NewLine);

            Console.WriteLine("<---End Payload before Encryption --->" + Environment.NewLine);



          

            Console.WriteLine("<---Begin Payload  Encryption --->" + Environment.NewLine);
          
            byte[] byteData = Encrypt(Encoding.Default.GetBytes(sendData.ToString()), cert);

            Console.WriteLine("<---Begin Fetch stream --->" + Environment.NewLine);
            Stream sw = req.GetRequestStream();
            Console.WriteLine("<---Begin write  stream --->" + Environment.NewLine);
            sw.Write(byteData, 0, byteData.Length);

            sw.Close();

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
           string _responseText = sr.ReadToEnd();
            sr.Close();
            Console.WriteLine(_responseText);
            Console.ReadLine();

        }



        private static string SignDetached(byte[] data, X509Certificate2 signingCert)
        {

            
            ContentInfo content = new ContentInfo(data);

            SignedCms signedMessage = new SignedCms(content,true);            

            CmsSigner signer = new CmsSigner(signingCert);
            //signer.DigestAlgorithm = new Oid("1.3.14.3.2.26");
           
            signedMessage.ComputeSignature(signer);
            byte[] signedBytes = signedMessage.Encode();


            return Convert.ToBase64String(signedBytes);
        }

        private static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {

            // Get the certificate store for the current user.
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            Console.WriteLine("<-- Certificate Start -->"+Environment.NewLine);
            try
            {
                store.Open(OpenFlags.ReadOnly);

                // Place all certificates in an X509Certificate2Collection object.
                X509Certificate2Collection certCollection = store.Certificates;
                // If using a certificate with a trusted root you do not need to FindByTimeValid, instead: 
                // currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, true);
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                foreach (X509Certificate2 cert in currentCerts)
                {
                    Console.WriteLine(Environment.NewLine + cert.SubjectName.Name + ":" + cert.Thumbprint + Environment.NewLine);
                    
                }

                Console.WriteLine("<-- Certificate End -->"+Environment.NewLine);

                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindByThumbprint,thumbprint, false);
                if (signingCert.Count == 0)
                    return null;
                // Return the first certificate in the collection, has the right name and is current. 
                return signingCert[0];
            }
            finally
            {
                store.Close();
            }

        }

        private static byte[] Encrypt(byte[] data, X509Certificate2 encryptingCert)
        {
            // create ContentInfo
            ContentInfo plainContent = new ContentInfo(data);

            // EnvelopedCms represents encrypted data
            EnvelopedCms encryptedData = new EnvelopedCms(plainContent, new AlgorithmIdentifier(new Oid("3DES")));

            // add a recipient
            CmsRecipient recipient = new CmsRecipient(encryptingCert);

            
            // encrypt data with public key of recipient
            encryptedData.Encrypt(recipient);

            // create PKCS #7 byte array
            byte[] encryptedBytes = encryptedData.Encode();

            // return encrypted data
            return encryptedBytes;
        }

    }
}
