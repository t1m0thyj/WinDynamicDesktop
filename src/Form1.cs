using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace WinDynamicDesktop
{
    public partial class Form1 : Form
    {
        private Uri imagesZipUri = new Uri("https://files.rb.gd/mojave_dynamic.zip");

        public Form1()
        {
            InitializeComponent();
        }

        public void updateLogLine(string newLine)
        {
            string[] lines = logTextBox.Lines;
            lines[lines.Length - 1] = newLine;
            logTextBox.Lines = lines;
        }

        public void downloadImagesZip()
        {
            logTextBox.Text += "Downloading images.zip (39.0 MB)...0%";

            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += client_DownloadProgressChanged;
                client.DownloadFileCompleted += client_DownloadFileCompleted;
                client.DownloadFileAsync(imagesZipUri, "images.zip");
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            updateLogLine("Downloading images.zip (39.0 MB)..." + e.ProgressPercentage + "%");
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            updateLogLine("Downloading images.zip (39.0 MB)...done" + Environment.NewLine);
            extractImagesZip();
        }

        public void extractImagesZip()
        {
            logTextBox.Text += "Extracting images.zip...";
            ZipFile.ExtractToDirectory("images.zip", "images");
            logTextBox.Text += "done" + Environment.NewLine;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!Directory.Exists("images"))
            {
                if (!File.Exists("images.zip"))
                {
                    downloadImagesZip();
                }
                else
                {
                    extractImagesZip();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LocationIQService service = new LocationIQService();
            LocationIQPlace place = service.getLocationData(textBox1.Text);
            MessageBox.Show(place.lat.ToString() + place.lon.ToString());
        }
    }
}
