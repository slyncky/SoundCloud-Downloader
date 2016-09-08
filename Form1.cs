using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace SoundCloud_Downloader
{
    public partial class Form1 : Form
    {
        string songName;
        string clientId = SoundCloud_Downloader.Properties.Settings.Default.ClientID;
        public Form1()
        {
            InitializeComponent();
            btnGO.Left = (this.ClientSize.Width - btnGO.Width) / 2;
            string line;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            StreamReader file = new StreamReader(path+@"Settings.txt");

            int lenghtOfArray = 0;
            while ((line = file.ReadLine()) != null)
            {
                txtDownloadFolder.Text = line.Substring(line.LastIndexOf("=")+1);
                lenghtOfArray++;
            }
            file.Close();
        }

        private void btnGO_Click(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(doDownload);
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerAsync();
        }
        private void doDownload(object sender, DoWorkEventArgs e)
        {
            lblStatus.Invoke(new MethodInvoker(delegate { lblStatus.Text = ""; }));
            if (!Directory.Exists(txtDownloadFolder.Text))
            {
                //lblStatus.Text = "Please correct the download directory";
                lblStatus.Invoke(new MethodInvoker(delegate { lblStatus.Text = "Please correct the download directory"; }));
                lblStatus.ForeColor = Color.Red;
                setLablePosition();
            }
            else if (txtURL.Text == "")
            {
                //lblStatus.Text = "Please enter a valid URL";
                lblStatus.Invoke(new MethodInvoker(delegate { lblStatus.Text = "Please enter a valid URL"; }));
                lblStatus.ForeColor = Color.Red;
                setLablePosition();
            }
            else
            {
                try
                {

                    songName = getStreamURL();
                    //webBrowser1.Navigate(new Uri(streamURL + "?client_id=2a9ccd31d93cec53b96e4ab9ee8f9db1"));
                    //txtURL.Clear();


                }
                catch (Exception ex)
                {
                    //lblStatus.Text = ex.Message;
                    lblStatus.Invoke(new MethodInvoker(delegate { lblStatus.Text = ex.Message; }));
                    lblStatus.ForeColor = Color.Red;
                    setLablePosition();
                }

            }
        }
      
        public void setLablePosition()
        {
            //lblStatus.Left = (this.ClientSize.Width - lblStatus.Width) / 2;
            lblStatus.Invoke(new MethodInvoker(delegate { lblStatus.Left = (this.ClientSize.Width - lblStatus.Width) / 2; }));
        }
 
        public string getStreamURL()
        {
                //get the JSON return form the URL
                WebClient wc = new WebClient();
                var x = wc.DownloadString(@"https://api.soundcloud.com/resolve?url=" + txtURL.Text.ToString() + @"&_status_format=json&client_id=" + clientId);

                //deserialize the JSON return
                var jobject = JsonConvert.DeserializeObject<RootObject>(x);

                string streamURL = jobject.stream_url;
                streamURL = streamURL + "?client_id=" + clientId;
                //Clipboard.SetText(jobject.title);


                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(streamURL);
                req.AllowAutoRedirect = false;
                req.Timeout = 10000;
                req.Method = "HEAD";

                HttpWebResponse webResponse;
                using (webResponse = (HttpWebResponse)req.GetResponse())
                {
                    if ((int)webResponse.StatusCode >= 300 && (int)webResponse.StatusCode <= 399)
                    {
                        string uriString = webResponse.Headers["Location"];
                        webResponse.Close();
                        WebClient client = new WebClient();
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                         client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                

                         client.DownloadFileAsync (new Uri (uriString), txtDownloadFolder.Text+@"\"+jobject.title+".mp3");

                         //progressBar1.Visible = true;
                         progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Visible = true; }));
                         //lblStatus.Text = "Downloading...";
                         lblStatus.Invoke(new MethodInvoker(delegate { lblStatus.Text = "Downloading..."; }));
                         lblStatus.ForeColor = Color.Red;
                         setLablePosition();
                         //btnGO.Enabled = false;
                         btnGO.Invoke(new MethodInvoker(delegate { btnGO.Enabled = false; }));
                         
                    }
                }



                return jobject.title;
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            //progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString()); }));
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            lblStatus.Invoke(new MethodInvoker(delegate { lblStatus.Text = "Download Completed = " + songName; }));
            //lblStatus.Text = "Download Completed = " + songName;
            lblStatus.ForeColor = Color.Green;
            setLablePosition();
            //progressBar1.Visible = false;
            progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Visible = false; }));
            //btnGO.Enabled = true;
            btnGO.Invoke(new MethodInvoker(delegate { btnGO.Enabled = true; }));
        }


        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //webBrowser1.ShowSaveAsDialog();
        }

        private void webBrowser1_FileDownload(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtURL.Text = "https://soundcloud.com/wittytunes/samuel-dan-gimme-original-mix";
            txtURL.ForeColor = Color.Gray;
            btnGO.Focus();
        }

        private void txtURL_Enter(object sender, EventArgs e)
        {

        }

        private void txtURL_Click(object sender, EventArgs e)
        {
            txtURL.Text = "";
            txtURL.ForeColor = Color.Black;
        }

        private void btnDownloadDirectory_Click(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtDownloadFolder.Text = folderBrowserDialog1.SelectedPath;
                StreamWriter writer = new StreamWriter(path + "Settings.txt");
                writer.WriteLine("DLDirectory="+txtDownloadFolder.Text);
                writer.Close();
            }
        }

    }

    public class User
    {
        public int id { get; set; }
        public string kind { get; set; }
        public string permalink { get; set; }
        public string username { get; set; }
        public string last_modified { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
        public string avatar_url { get; set; }
    }

    public class RootObject
    {
        public string kind { get; set; }
        public int id { get; set; }
        public string created_at { get; set; }
        public int user_id { get; set; }
        public int duration { get; set; }
        public bool commentable { get; set; }
        public string state { get; set; }
        public int original_content_size { get; set; }
        public string last_modified { get; set; }
        public string sharing { get; set; }
        public string tag_list { get; set; }
        public string permalink { get; set; }
        public bool streamable { get; set; }
        public string embeddable_by { get; set; }
        public bool downloadable { get; set; }
        public string purchase_url { get; set; }
        public object label_id { get; set; }
        public string purchase_title { get; set; }
        public string genre { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string label_name { get; set; }
        public string release { get; set; }
        public string track_type { get; set; }
        public string key_signature { get; set; }
        public string isrc { get; set; }
        public object video_url { get; set; }
        public object bpm { get; set; }
        public object release_year { get; set; }
        public object release_month { get; set; }
        public object release_day { get; set; }
        public string original_format { get; set; }
        public string license { get; set; }
        public string uri { get; set; }
        public User user { get; set; }
        public string permalink_url { get; set; }
        public string artwork_url { get; set; }
        public string waveform_url { get; set; }
        public string stream_url { get; set; }
        public int playback_count { get; set; }
        public int download_count { get; set; }
        public int favoritings_count { get; set; }
        public int comment_count { get; set; }
        public string attachments_uri { get; set; }
        public string policy { get; set; }
    }
}
