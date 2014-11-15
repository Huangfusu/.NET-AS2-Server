using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;


namespace AS2_SimulationServer
{
    class SendEDIMessage
    {
        public void SendEDI(PropogationContext context)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                HttpWebRequest req = WebRequest.Create(context.URL) as HttpWebRequest;

                DataStruct data = new DataStruct();
                var logger = Helper.GetInboundLogger();
                if (Settings.Timeout > 0)
                    req.Timeout = Settings.Timeout;
                DateTime dt = DateTime.Now;
                X509Certificate2 encryptioncert = Settings.EncryptionCertificate;
                X509Certificate2 signingcert = Settings.SigningCertificate;
                string GUID=Guid.NewGuid().ToString("N");
                string messageID = "<" + GUID + "@" + Settings.AS2From + ">";

                req.Method = "POST";
                req.ContentType = "application/pkcs7-mime; smime-type=enveloped-data; name=\"smime.p7m\"";
                req.UserAgent = "Sampler.EDI.AS2";
                req.Headers.Add("Message-Id", messageID);
                req.Headers.Add("From", Settings.Email);
                if(!Settings.IsAS2Default && Settings.Subject.Contains("{0}"))
                req.Headers.Add("Subject", String.Format(Settings.Subject,GUID));
                else
                req.Headers.Add("Subject", Settings.Subject);
               // req.Headers.Add("Date", DateTime.Now.ToUniversalTime().ToString("ddd, dd MMM yyyy hh:mm:ss ") + "GMT");
                req.Date = DateTime.Now.ToUniversalTime();
                req.Headers.Add("Mime-Version", "1.0");
                req.Headers.Add("AS2-Version", "1.2");
                req.Headers.Add("AS2-From", Settings.AS2From);
                req.Headers.Add("AS2-To", Settings.AS2To);
                if (Settings.IsAsync)
                    req.Headers.Add("Receipt-Delivery-Option", Settings.ReceiptDeliveryOption);
                req.Headers.Add("Content-Disposition", "attachment; filename=\"smime.p7m\"");
                req.Headers.Add("Disposition-notification-To", Settings.ReceiptDeliveryOption);
                req.Headers.Add("Disposition-notification-options", "signed-receipt-protocol=optional,pkcs7-signature;signed-receipt-micalg=optional,sha1");


                String divider = "=Part" + dt.ToString("_dd_HHmmss.ffffff");

                StringBuilder sendData = new StringBuilder();
                sendData.Append("MIME-Version: 1.0\r\n");
                sendData.Append("Content-Type: multipart/signed; protocol=\"application/pkcs7-signature\"; micalg=sha1; boundary=\"" + divider + "\"\r\n");
                sendData.Append("\r\n");
                StringBuilder strHeader = new StringBuilder();

                FormatServerResponse.AsyncDisplayMessage("Create MIME Content");

                strHeader.Append(String.Format(
                        "Content-Type: {0}; name=\"{1}\"\r\n"
                        + "Content-Transfer-Encoding: binary\r\n"
                        + "Content-Disposition: attachment; filename=\"{2}\"\r\n"
                        + "\r\n",
                        "application/EDI-Consent",
                        Settings.FileName,
                        Settings.FileName));

                try
                {
                    foreach (string file in Directory.GetFiles(context.Folder))
                    {
                        /* FileInfo fileInfo = new FileInfo(file);
                         strHeader.Append(String.Format("{0}"
                             + "\r\n",
                             File.ReadAllText(file)
                             ));*/



                        strHeader.Append(String.Format("{0}"
                            + "\r\n",(Settings.IsAS2Default) ? logger.generateStringFromEDIFile(file):logger.generateMiddlewareMessage(GUID,file)
                            ));
                    }
                }
                catch (Exception ex)
                {
                    FormatServerResponse.AsyncDisplayErrorMessage(ex.Message);
                    return;
                }

                string strData = "--" + divider + "\r\n"
                       + strHeader + "\r\n"
                       + "--" + divider + "\r\n"
                       + "Content-Type: application/pkcs7-signature; name= smime.p7s; smime-type=signed-data" + "\r\n"
                       + "Content-Disposition: attachment; filename=\"smime.p7s\"" + "\r\n"
                       + "Content-Transfer-Encoding: base64\r\n"
                       + "\r\n"
                       + Certificates.SignDetached(Encoding.Default.GetBytes(strHeader.ToString()), signingcert)
                       + "\r\n";
                sendData.Append(strData);

                sendData.Append("--" + divider + "--");

                StringBuilder request = new StringBuilder();

                foreach (string key in req.Headers.Keys)
                {
                    request.AppendLine(String.Format("{0}:{1}", key, req.Headers[key]));
                }
                request.Append(sendData);

                if (Settings.Log)
                {
                    File.WriteAllBytes(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\request" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", Encoding.Default.GetBytes(request.ToString()));


                    FormatServerResponse.DisplayMessage("Log Payload before Encryption");

                    FormatServerResponse.DisplayMessage("Begin Payload  Encryption");
                }

                byte[] byteData = Certificates.Encrypt(Encoding.Default.GetBytes(sendData.ToString()), encryptioncert);
                try
                {

                    data.StartTime = DateTime.Now;
                    data.Type = 'S';


                    MessageCounter.collection.AddOrUpdate(messageID, data, (key, oldValue) => data);
                    FormatServerResponse.AsyncDisplayMessage("Fetch stream");
                    MessageCounter.IncrementConnection();
                    Stream sw = req.GetRequestStream();
                    FormatServerResponse.AsyncDisplayMessage("Write to stream");
                    sw.Write(byteData, 0, byteData.Length);
                    sw.Close();


                    StringBuilder response = new StringBuilder();
                    FormatServerResponse.AsyncDisplayMessage("Waiting for response");


                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {

                        foreach (string key in resp.Headers.Keys)
                        {
                            response.AppendLine(String.Format("{0}:{1}", key, resp.Headers[key]));
                        }

                        StreamReader sr = new StreamReader(resp.GetResponseStream());
                        MessageCounter.DecrementConnection();

                        string _responseText = sr.ReadToEnd();
                        response.Append(Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(_responseText)));

                        if (Settings.Log)
                            File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\mdn_actually" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", _responseText, Encoding.UTF8);

                        sr.Close();

                        if (!Settings.IsAsync)
                        {
                            data.EndTime = DateTime.Now;
                            MessageCounter.collection.AddOrUpdate(messageID, data, (key, oldValue) => data);

                            if (_responseText.Contains("Your message was successfully received and processed."))
                            {

                                FormatServerResponse.AsyncDisplaySuccessMessage("State:Success");
                            }
                            else
                            {
                                FormatServerResponse.AsyncDisplayErrorMessage("State:Error CHECK mdn" + dt.ToString("_dd_HHmmss.ffffff"));
                            }

                        }

                        resp.Close();
                    }

                    if (Settings.Log)
                        File.WriteAllText(@"C:\Users\rmd\Documents\Sterling Documents\Sample\log\mdn" + dt.ToString("_dd_HHmmss.ffffff") + ".txt", response.ToString());

                    try
                    {
                        if (Settings.NoSQL)
                            logger.LogStartTime(data.StartTime);
                    }
                    catch (Exception ex)
                    {
                        FormatServerResponse.AsyncDisplayErrorMessage(ex.Message);
                    }

                }
                catch (Exception ex)
                {
                    MessageCounter.DecrementConnection();
                    FormatServerResponse.AsyncDisplayErrorMessage(ex.Message);
                    if (Settings.LogToFile)
                        Logger.Log(String.Format("{0},{1},{2}", messageID, ex.Message, DateTime.Now.ToString("o")));
                }
            }
            catch (Exception ex)
            {
                FormatServerResponse.AsyncDisplayErrorMessage(ex.Message);
            }
                    
        }
        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        
    }
}
