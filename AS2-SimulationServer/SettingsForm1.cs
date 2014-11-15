/*
 * Some
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace AS2_SimulationServer
{
    public partial class SettingsForm : Form
    {
        Stopwatch watch = new Stopwatch();
        const string client_start = "Client Start";
        const string client_stop = "Client Stop";
        DateTime clientStartTime;

        public SettingsForm()
        {
            InitializeComponent();
            var pos = this.PointToScreen(label19.Location);
            pos = pictureBox1.PointToClient(pos);
            label19.Parent = pictureBox1;
            label19.Location = pos;
            label19.BackColor = Color.Transparent;

        }

        
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            
            try
            {
                
                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.Load(Settings.ExecutionPath + @"\Defaults.xml");

                TxtAS2From.Text = xmlDoc.SelectSingleNode("/Defaults/as2From[1]").InnerText;
                TxtEmail.Text = xmlDoc.SelectSingleNode("/Defaults/companyEmail[1]").InnerText;
                TxtAS2To.Text = xmlDoc.SelectSingleNode("/Defaults/as2To[1]").InnerText;
                TxtUrl.Text = xmlDoc.SelectSingleNode("/Defaults/sendURL[1]").InnerText;
                TxtMaxFolder.Text = xmlDoc.SelectSingleNode("/Defaults/maxFolder[1]").InnerText;
                TxtAvgFolder.Text = xmlDoc.SelectSingleNode("/Defaults/avgFolder[1]").InnerText;
                TxtMinFolder.Text = xmlDoc.SelectSingleNode("/Defaults/minFolder[1]").InnerText;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); };

            foreach (string item in Certificates.GetCertificateStore())
            {
                TxtPubThumbprint.AutoCompleteCustomSource.Add(item);
                TxtPvtThumbprint.AutoCompleteCustomSource.Add(item);
            }
            foreach (string item in Certificates.GetCertificateStoreSLL())
            {
                TxtSSL.AutoCompleteCustomSource.Add(item);
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            if (String.IsNullOrEmpty(TxtPubThumbprint.Text))
            {
                errorProvider1.SetError(TxtPubThumbprint, "Cannot be null");
                return;
            }
            if (String.IsNullOrEmpty(TxtPvtThumbprint.Text))
            {
                errorProvider1.SetError(TxtPvtThumbprint, "Cannot be null");
                return;
            }
            if (checkSSL.Checked && String.IsNullOrEmpty(TxtSSL.Text))
            {
                errorProvider1.SetError(TxtSSL, "Cannot be null");
                return;
            }

            
         
            Certificates.PrivateKeyThumbprint = TxtPvtThumbprint.Text;
            Certificates.PublicKeyThumbprint = TxtPubThumbprint.Text;
            Certificates.SSLThumbprint = TxtSSL.Text;
            Settings.AS2From = TxtAS2From.Text;
            Settings.AS2To = TxtAS2To.Text;
            Settings.Email = TxtEmail.Text;
            Settings.AvgFolder = TxtAvgFolder.Text;
            Settings.MaxFolder = TxtMaxFolder.Text;
            Settings.MinFolder = TxtMinFolder.Text;
            Settings.URL = TxtUrl.Text;
            Settings.IsAsync = checkMDN.Checked;
            Settings.AvgMessagePerMin = Convert.ToInt32(numericAvg.Value);
            Settings.MaxMessagePerMin = Convert.ToInt32(numericMax.Value);
            Settings.MinMessagePerMin = Convert.ToInt32(numericMin.Value);
            Settings.Timeout = Convert.ToInt32(numericTimeout.Value)*1000;
            Settings.Duration = Convert.ToInt32(numericDuration.Value)*1000*60;
            Settings.Log = CheckLog.Checked;            
            Settings.EncryptionCertificate=Certificates.GetPublicCertificateFromStore();
            Settings.SigningCertificate = Certificates.GetPrivateCertificateFromStore();
            Settings.NoSQL = checkSQLLog.Checked;

            if (checkSSL.Checked)
            {
                Certificates.OpenSSL();
                Settings.ReceiptDeliveryOption = Settings.SslURL;
            }
            else
            {
                if (Certificates.CheckSSL())
                 Certificates.DeleteSSL();
                Settings.ReceiptDeliveryOption = Settings.HttpURL;
            }

            FormatServerResponse.DisplayMessage("Updated settings");
            FormatServerResponse.DisplayMessage("From - "+Settings.AS2From);
            FormatServerResponse.DisplayMessage("To - "+Settings.AS2To);
            FormatServerResponse.DisplayMessage("Email - "+Settings.Email);
            FormatServerResponse.DisplayMessage("Selected Files Folder for Avg messages - "+Settings.AvgFolder);
            FormatServerResponse.DisplayMessage("Selected Files Folder for Min messages - " + Settings.MinFolder);
            FormatServerResponse.DisplayMessage("Selected Files Folder for Max messages - " + Settings.MaxFolder);
            FormatServerResponse.DisplayMessage("URL - "+Settings.URL);
            FormatServerResponse.DisplayMessage("Signing Cert Thumbprint - " + Certificates.PrivateKeyThumbprint);
            FormatServerResponse.DisplayMessage("Encryption Cert Thumbprint - " + Certificates.PublicKeyThumbprint);
            FormatServerResponse.DisplayMessage("SSL Cert Thumbprint - " + Certificates.SSLThumbprint);
            FormatServerResponse.DisplayMessage("Is Async MDN - " +checkMDN.Checked.ToString());
            FormatServerResponse.DisplayMessage("Is SSL enabled - " + checkSSL.Checked.ToString());
            FormatServerResponse.DisplayMessage("Number of Averge sized message per minute - " + Settings.AvgMessagePerMin);
            FormatServerResponse.DisplayMessage("Number of Min sized message per minute - " + Settings.MinMessagePerMin);
            FormatServerResponse.DisplayMessage("Number of Max sized message per minute - " + Settings.MaxMessagePerMin);
            FormatServerResponse.DisplayMessage("Response URL - " + Settings.ReceiptDeliveryOption);
            FormatServerResponse.DisplayMessage("Duration of the run in milliseconds - " + Settings.Duration);
            FormatServerResponse.DisplayMessage("Timeout in sec - " + Settings.Timeout/1000);            
            FormatServerResponse.DisplayMessage("Log files - " + Settings.Log.ToString());
            FormatServerResponse.DisplayMessage("Log to SQL-" + Settings.NoSQL);
            button3.Visible = true;
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            switch (button3.Text)
            {
                case client_start:
                    if (!backgroundWorker.IsBusy)
                    {
                        MessageCounter.Reset();
                        watch.Start();
                        backgroundWorker.RunWorkerAsync();
                        timer.Start();
                        button3.Text = client_stop;                       
                    }
                    break;
                case client_stop:
                    backgroundWorker.CancelAsync();
                    button3.Text = client_start;
                    break;


            }
        }

        private static void PublishonAS2(object url)
        {
            FormatServerResponse.DisplayMessage("Publish EDI message");
            PropogationContext context = new PropogationContext();
            context.URL = url as string;
            context.Folder = Settings.AvgFolder;
            SendEDIMessage request = new SendEDIMessage();            
            request.SendEDI(context);
            FormatServerResponse.DisplayMessage("Number of messages published - " + MessageCounter.IncrementAvgMessageSent());
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                FormatServerResponse.DisplayErrorMessage("Client publish stopped");
            else
                FormatServerResponse.DisplaySuccessMessage("Client completed");

            button3.Text = client_start;
            timer.Stop();
            watch.Reset();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            clientStartTime = DateTime.Now;
            Stopwatch monitor= new Stopwatch();
            int loopCount = 0;
            monitor.Start();
            var worker = sender as BackgroundWorker;

            string[] urlLis = Settings.URL.Split('|');

            while (watch.ElapsedMilliseconds < Settings.Duration && !e.Cancel)
            {
               e.Cancel= worker.CancellationPending;

                if (monitor.ElapsedMilliseconds > 60000*loopCount)
                {
                    FormatServerResponse.DisplaySuccessMessage("Entered loop - " + (loopCount + 1));

                    if (!String.IsNullOrEmpty(Settings.AvgFolder))
                    {
                        if (!e.Cancel)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                FormatServerResponse.DisplayMessage("Starting Avg message publish");

                                for(int i =0; i< Settings.AvgMessagePerMin ; i++)
                                {
                                    if (e.Cancel)
                                        break;

                                    for(int j=0; j<urlLis.Count(); j++)
                                    {
                                        if (!String.IsNullOrEmpty(urlLis[j]))
                                        {
                                            FormatServerResponse.DisplayMessage("Publish EDI message to " + urlLis[j]);
                                            PropogationContext context = new PropogationContext();
                                            context.URL = urlLis[j];
                                            context.Folder = Settings.AvgFolder;
                                            Task.Factory.StartNew(() =>
                                                {
                                                    if (!e.Cancel)
                                                    {
                                                        SendEDIMessage request = new SendEDIMessage();
                                                        request.SendEDI(context);
                                                        FormatServerResponse.DisplayMessage("Number of Avg messages published - " + MessageCounter.IncrementAvgMessageSent());
                                                        UpdateTime(DateTime.Now);
                                                    }
                                                });
                                        }
                                    }
                                }
                            });
                        }
                    }

                    if (!String.IsNullOrEmpty(Settings.MinFolder))
                    {
                        if (!e.Cancel)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                FormatServerResponse.DisplayMessage("Starting Min message publish");
                                for(int i=0; i <Settings.MinMessagePerMin;i++)
                                {
                                    if (e.Cancel)
                                        break;
                                    for (int j = 0; j < urlLis.Count(); j ++)
                                    {
                                        if (!String.IsNullOrEmpty(urlLis[j]))
                                        {
                                            FormatServerResponse.DisplayMessage("Publish EDI message to " + urlLis[j]);
                                            PropogationContext context = new PropogationContext();
                                            context.URL = urlLis[j];
                                            context.Folder = Settings.MinFolder;
                                            Task.Factory.StartNew(() =>
                                                {
                                                    if (!e.Cancel)
                                                    {
                                                        SendEDIMessage request = new SendEDIMessage();
                                                        request.SendEDI(context);
                                                        FormatServerResponse.DisplayMessage("Number of Min  messages published - " + MessageCounter.IncrementMinMessageSent());
                                                        UpdateTime(DateTime.Now);
                                                    }
                                                });
                                        }
                                    }
                                }
                            });
                        }
                    }

                    if (!String.IsNullOrEmpty(Settings.MaxFolder))
                    {
                        if (!e.Cancel)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                FormatServerResponse.DisplayMessage("Starting Max message publish");
                                for(int i=0; i<Settings.MaxMessagePerMin ; i++)
                                {
                                    if (e.Cancel)
                                        break;
                                    for (int j = 0; j < urlLis.Count(); j ++)
                                    {
                                        if (!String.IsNullOrEmpty(urlLis[j]))
                                        {
                                            FormatServerResponse.DisplayMessage("Publish EDI message to " + urlLis[j]);
                                            PropogationContext context = new PropogationContext();
                                            context.URL = urlLis[j];
                                            context.Folder = Settings.MaxFolder;

                                            Task.Factory.StartNew(() =>
                                                {
                                                    if (!e.Cancel)
                                                    {
                                                        SendEDIMessage request = new SendEDIMessage();
                                                        request.SendEDI(context);
                                                        FormatServerResponse.DisplayMessage("Number of Max messages published - " + MessageCounter.IncrementMaxMessageSent());
                                                        UpdateTime(DateTime.Now);
                                                    }
                                                });
                                        }
                                    }
                                }
                            });
                        }
                    }

                    loopCount++;
                }

                if (!e.Cancel)                    
                Thread.Sleep(1000);
                else
                    break;

            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            LblDuration.Text = watch.Elapsed.ToString("h'h: 'm'm: 's's'");
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Helper.CaluculateMaxResponse();
        }

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormatServerResponse.DisplayServiceStop();
        }

       


        void UpdateTime(DateTime time)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(delegate()
                {
                    LblClientComplete.Text = ((DateTime)time - clientStartTime).ToString("h'h: 'm'm: 's's'");
                }));
            else
            {
                LblClientComplete.Text = ((DateTime)time - clientStartTime).ToString("h'h: 'm'm: 's's'");
            }
        }

        private void timerConnections_Tick(object sender, EventArgs e)
        {
            LblConn.Text = MessageCounter.ConcurrentConnection.ToString();
        }

       
       

            
    }
}
