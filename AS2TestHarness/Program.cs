using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;

namespace AS2TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            X509Certificate2 cert = GetCertificateFromStore("E=eaiadmin@intel.com, CN=b2btpaqa.intel.com");

            

            string postData = "UNA:+.? 'UNB+UNOC:2+BangOlufsen+047897855XLT+120123:1214+ORDERS00094++ORDERS'UNH+1+ORDERS:D:97A:UN'BGM+105+3503837083/4501302080+9'DTM+137:20120123:102'DTM+4:20120123:102'NAD+BY+buyerAzlan::90'CTA+OC+:Steven Buchanan'COM+441904695061:TE'NAD+ST+4001++Tech Data Espana SA+C/Avenida del Rio Henares 52+ALOVERA - GUADALAJARA++19208+ES'CUX+2:USD:9'LIN+10++BX80623I32100:MF::90'PIA+1+2100573:BP::92'IMD+F++:::CORE I3-2100/3.10 GHZ 3M LGA1155'QTY+21:120:EA'DTM+2:20120123:102'PRI+CAL:117:TU'LIN+20++BX80623I72600K:MF::90'PIA+1+2115481:BP::92'IMD+F++:::CORE I7-2600K/3.4GHZ LGA1155 8MB'QTY+21:25:EA'DTM+2:20120123:102'PRI+CAL:305:TU'LIN+30++BOXDH67GDB3:MF::90'PIA+1+2164946:BP::92'IMD+F++:::MB BOXDH67GD/UATX 1155 H67 DDR3-133'QTY+21:5:EA'DTM+2:20120123:102'PRI+CAL:88:TU'LIN+40++BOXDH61WWB3:MF::90'PIA+1+2164950:BP::92'IMD+F++:::MB BOXDH61WW/UATX 1155 H61 DDR3-133'QTY+21:20:EA'DTM+2:20120123:102'PRI+CAL:61:TU'LIN+50++BX80623G620:MF::90'PIA+1+2201621:BP::92'IMD+F++:::PENTIUM G620/2.6 GHZ LGA1155 3MB'QTY+21:55:EA'DTM+2:20120123:102'PRI+CAL:60:TU'LIN+60++BX80571E3400:MF::90'PIA+1+1897349:BP::92'IMD+F++:::CELERON E3400/2.6GHZ FSB800 1MB'QTY+21:65:EA'DTM+2:20120123:102'PRI+CAL:39:TU'LIN+70++EXPI9301CTBLK:MF::90'PIA+1+1595930:BP::92'IMD+F++:::PRO/1000 CT DESKTOP ADAPTER PCIEX B'QTY+21:20:EA'DTM+2:20120123:102'PRI+CAL:25:TU'UNS+S'UNT+53+1'UNZ+1+ORDERS00094'";

            byte[] bytes=SignDetached(Encoding.Default.GetBytes(postData), cert);

            Console.WriteLine(Convert.ToBase64String(bytes));

            Console.ReadLine();

            return;

            try
            {
                // Create a request using a URL that can receive a post. 
                WebRequest request = WebRequest.Create("http://vmsdasbi02:30033/as2");
                // Set the Method property of the request to POST.
                request.Method = "POST";

                request.Headers.Add("as2-version", "1.2");
                request.Headers.Add("as2-from", "mycompanyAS2");
                request.Headers.Add("as2-to", "Intel");
                request.Headers.Add("message-id", "<mendelson_opensource_AS2-1349850101314-0@mycompanyAS2_AS2Ident>");
                request.Headers.Add("subject", "AS2 message");
                request.Headers.Add("recipient-address", "http://vmsdasbi02:30033/as2");
                request.Headers.Add("From", "as2@company.com");

                request.Headers.Add("TEDate", "Wed, 10 Oct 2012 11:51:41 IST");
                request.Headers.Add("receipt-delivery-option", "http://www.company.com:8080/as2/HttpReceiver");
                request.Headers.Add("disposition-notification-to", "http://www.company.com:8080/as2/HttpReceiver");
                request.Headers.Add("filename", "EDIFACT.txt");

                request.Headers.Add("content-disposition", "attachment; filename=\"smime.p7m\"");
                request.Headers.Add("ediint-features", "multiple-attachments");
                request.Headers.Add("CEMMIME-Version", "1.0");


                // Create POST data and convert it to a byte array.
               
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "multipart/signed; protocol=\"application/pkcs7-signature\"; micalg=sha1;  boundary=\"----=_Part_4_13649987.1349781493703\"";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                Console.WriteLine("Response Status:{0}", ((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.
                Console.WriteLine("Response:{0}", responseFromServer);
                // Clean up the streams.
                reader.Close();
                dataStream.Close();
                response.Close();
                

                Console.ReadKey();
            }
            catch (Exception e)
            {
            }
        }

        private static byte[] SignDetached(byte[] data, X509Certificate2 signingCert)
        {
            

            ContentInfo content = new ContentInfo(data);

            SignedCms signedMessage = new SignedCms(content, true);

           
            CmsSigner signer = new CmsSigner(signingCert);
            signedMessage.ComputeSignature(signer);
            byte[] signedBytes = signedMessage.Encode();

            
            return signedBytes;
        }

        private static X509Certificate2 GetCertificateFromStore(string certName)
        {

            // Get the certificate store for the current user.
            X509Store store = new X509Store(StoreLocation.CurrentUser);
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
                    Console.WriteLine(cert.SubjectName.Name);
                }
                Console.ReadLine();
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, false);
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


    }

}
