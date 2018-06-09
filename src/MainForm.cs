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
using System.Reflection;
using System.Runtime.InteropServices;

namespace WinDynamicDesktop
{
    public partial class MainForm : Form
    {
        // Hack to show watermark in textbox from https://stackoverflow.com/questions/2487104
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam,
            [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public MainForm()
        {
            InitializeComponent();
        }

        public void AppendToLog(string lineText, bool addNewLine = true)
        {
            if (addNewLine)
            {
                lineText += Environment.NewLine;
            }
            logTextBox.AppendText(lineText);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SendMessage(textBox1.Handle, 0x1501, 1, "Enter your location...");
            logTextBox.Text = "Welcome to WinDynamicDesktop " +
                Assembly.GetExecutingAssembly().GetName().Version.ToString() + "!" +
                Environment.NewLine;
            logTextBox.Text += "Enter your location in the textbox and click Set Location." +
                Environment.NewLine;
            logTextBox.Text += "Once a valid location has been entered, this window will" +
                Environment.NewLine;
            logTextBox.Text += "close and the program will run in the background." +
                Environment.NewLine;
            logTextBox.Text += "You can still access it from the icon in your system tray." +
                Environment.NewLine;
            /*Wallpaper.Set(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "images",
                "mojave_dynamic_1.jpeg")), Wallpaper.Style.Stretched);*/
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            /*if (!Directory.Exists("images"))
            {
                if (!File.Exists("images.zip"))
                {
                    DownloadImagesZip();
                }
                else
                {
                    ExtractImagesZip();
                }
            }*/
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
