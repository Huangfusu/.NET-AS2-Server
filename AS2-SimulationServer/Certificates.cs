using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.Diagnostics;

namespace AS2_SimulationServer
{
    class Certificates
    {
        public static string PrivateKeyThumbprint
        { get; set; }

        public static string PublicKeyThumbprint
        { get; set; }

        public static string SSLThumbprint
        { get; set; }

        public static string SignDetached(byte[] data, X509Certificate2 signingCert)
        {


            ContentInfo content = new ContentInfo(data);

            SignedCms signedMessage = new SignedCms(content, true);

            CmsSigner signer = new CmsSigner(signingCert);


            signedMessage.ComputeSignature(signer);
            byte[] signedBytes = signedMessage.Encode();


            return Convert.ToBase64String(signedBytes);
        }

        public static X509Certificate2 GetPublicCertificateFromStore()
        {


            X509Store store = new X509Store(StoreLocation.CurrentUser);
            
            try
            {
                store.Open(OpenFlags.ReadOnly);


                X509Certificate2Collection certCollection = store.Certificates;

                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);




                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindByThumbprint, PublicKeyThumbprint, false);
                if (signingCert.Count == 0)

                    return null;

                return signingCert[0];
            }
            finally
            {
                store.Close();
            }

        }

        public static X509Certificate2 GetPrivateCertificateFromStore()
        {


            X509Store store = new X509Store(StoreLocation.CurrentUser);

            try
            {
                store.Open(OpenFlags.ReadOnly);


                X509Certificate2Collection certCollection = store.Certificates;

                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);




                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindByThumbprint, PrivateKeyThumbprint, false);
                if (signingCert.Count == 0)

                    return null;

                return signingCert[0];
            }
            finally
            {
                store.Close();
            }

        }

        public static byte[] Encrypt(byte[] data, X509Certificate2 encryptingCert)
        {

            ContentInfo plainContent = new ContentInfo(data);


            EnvelopedCms encryptedData = new EnvelopedCms(plainContent, new AlgorithmIdentifier(new Oid("3DES")));


            CmsRecipient recipient = new CmsRecipient(encryptingCert);



            encryptedData.Encrypt(recipient);


            byte[] encryptedBytes = encryptedData.Encode();


            return encryptedBytes;
        }

        public static string[] GetCertificateStore()
        {
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            
                store.Open(OpenFlags.ReadOnly);                
                X509Certificate2Collection certCollection = store.Certificates;                
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                string[] collection = new string[currentCerts.Count];
                int i=0;
                foreach (X509Certificate2 cert in currentCerts)
                {
                   collection[i]=cert.Thumbprint;
                    i++;
                }

                return collection;               
        }

        public static string[] GetCertificateStoreSLL()
        {
            X509Store store = new X509Store(StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = store.Certificates;
            X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
            string[] collection = new string[currentCerts.Count];
            int i = 0;
            foreach (X509Certificate2 cert in currentCerts)
            {
                collection[i] = cert.Thumbprint;
                i++;
            }

            return collection;
        }

        public static void OpenSSL()
        {
            DeleteSSL();

            string arguments = "http add sslcert ipport="+Settings.IPAddress+":"+Settings.SSLBasePort+" certhash="+SSLThumbprint+" appid={00112233-4455-6677-8899-AABBCCDDEEFF}";

            ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", arguments);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            var process = Process.Start(procStartInfo);

            FormatServerResponse.DisplaySuccessMessage(process.StandardOutput.ReadToEnd().Trim());

            process.WaitForExit();

            


        }

        public static void DeleteSSL()
        {
            
                string arguments = "http delete sslcert ipport=" + Settings.IPAddress + ":"+Settings.SSLBasePort;
                ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", arguments);

                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                var process = Process.Start(procStartInfo);

                String result = process.StandardOutput.ReadToEnd().Trim();

                process.WaitForExit();
               
            
        }

        public static bool CheckSSL()
        {
            string arguments = "http show sslcert";
            ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", arguments);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            var process = Process.Start(procStartInfo);
            String result =process.StandardOutput.ReadToEnd().Trim();
           
            process.WaitForExit();
            return (result.Contains("0.0.0.0:"+Settings.SSLBasePort)) ? true : false;
        }
    }
}
