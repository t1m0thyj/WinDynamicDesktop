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
using System.Reflection;

namespace WinDynamicDesktop
{
    public partial class MainForm : Form
    {
        private Uri imagesZipUri = new Uri("https://files.rb.gd/mojave_dynamic.zip");

        public MainForm()
        {
            InitializeComponent();
        }

        public void UpdateLogLine(string newLine)
        {
            string[] lines = logTextBox.Lines;
            lines[lines.Length - 1] = newLine;
            logTextBox.Lines = lines;
        }

        public void DownloadImagesZip()
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
            UpdateLogLine("Downloading images.zip (39.0 MB)..." + e.ProgressPercentage + "%");
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            UpdateLogLine("Downloading images.zip (39.0 MB)...done" + Environment.NewLine);
            ExtractImagesZip();
        }

        public void ExtractImagesZip()
        {
            logTextBox.Text += "Extracting images.zip...";
            ZipFile.ExtractToDirectory("images.zip", "images");
            logTextBox.Text += "done" + Environment.NewLine;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            logTextBox.Text = "Welcome to WinDynamicDesktop " +
                Assembly.GetExecutingAssembly().GetName().Version.ToString() + "!";
            /*Wallpaper.Set(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "images",
                "mojave_dynamic_1.jpeg")), Wallpaper.Style.Stretched);*/
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!Directory.Exists("images"))
            {
                if (!File.Exists("images.zip"))
                {
                    DownloadImagesZip();
                }
                else
                {
                    ExtractImagesZip();
                }
            }
        }

        private void setLocationButton_Click(object sender, EventArgs e)
        {
            LocationIQService service = new LocationIQService();
            LocationIQData data = service.GetLocationData(textBox1.Text);
            MessageBox.Show(data.lat + ", " + data.lon);
            SunriseSunsetService service2 = new SunriseSunsetService();
            SunriseSunsetData data2 = service2.GetWeatherData(data.lat, data.lon);
            MessageBox.Show(data2.results.sunrise + ", " + data2.results.sunset);
        }
    }
}
