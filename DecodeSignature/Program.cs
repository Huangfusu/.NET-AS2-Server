using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace DecodeSignature
{
    class Program
    {
        static void Main(string[] args)
        {
            X509Certificate2 cert = GetCertificateFromStore("E4B37177BF164945B02186405AF85D67946DC24E");

            string value = System.IO.File.ReadAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\LOG\FromSterlingPOST_29_205203.718857.txt", Encoding.UTF8);

           
            
        }

        static byte[] VerifyAndRemoveSignature(byte[] data, X509Certificate2 cert)
        {
            // create SignedCms
            SignedCms signedMessage = new SignedCms();
           ;

            Console.WriteLine(signedMessage.Certificates.Count.ToString());

           //signedMessage.

            // deserialize PKCS #7 byte array
           signedMessage.Decode(data);


            
            //signedMessage.CheckSignature(true)
            foreach (X509Certificate2 cer in signedMessage.Certificates)
            {
                Console.WriteLine(cer.Thumbprint);
            }

            // check signature
            // false checks signature and certificate
            // true only checks signature
            signedMessage.CheckSignature(false);

            // access signature certificates (if needed)
            foreach (SignerInfo signer in signedMessage.SignerInfos)
            {
                Console.WriteLine("Subject: {0}",
                  signer.Certificate.Subject);
            }

            // return plain data without signature
            return signedMessage.ContentInfo.Content;
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
