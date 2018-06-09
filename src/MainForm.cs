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
using Newtonsoft.Json;
using RestSharp;

namespace WinDynamicDesktop
{
    using LocationIQResponse = IRestResponse<List<LocationIQData>>;

    public partial class MainForm : Form
    {
        // Hack to show watermark in textbox from https://stackoverflow.com/questions/2487104
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam,
            [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public LocationConfig config;

        public MainForm(LocationConfig configObj)
        {
            config = configObj;
            InitializeComponent();
        }

        public void AppendToLog(string lineText, bool addNewLine = true)
        {
            if (addNewLine)
            {
                lineText += Environment.NewLine;
            }
            logOutput.AppendText(lineText);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SendMessage(locationInput.Handle, 0x1501, 1, "Enter your location...");
            if (config.Location != null)
            {
                locationInput.Text = config.Location;
            }

            AppendToLog("Welcome to WinDynamicDesktop " +
                Assembly.GetExecutingAssembly().GetName().Version.ToString() + "!");
            /*Wallpaper.Set(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "images",
                "mojave_dynamic_1.jpeg")), Wallpaper.Style.Stretched);*/
        }

        private void setLocationButton_Click(object sender, EventArgs e)
        {
            LocationIQService service = new LocationIQService();
            LocationIQResponse response = service.GetLocationData(locationInput.Text);
            if (response.Data.Count > 0)
            {
                LocationIQData data = response.Data[0];
                AppendToLog("Location set successfully to: " + data.display_name);
                AppendToLog("Latitude = " + data.lat + ", Longitude= " + data.lon);

                config.Location = locationInput.Text;
                config.Latitude = data.lat;
                config.Longitude = data.lon;

                File.WriteAllText("settings.conf", JsonConvert.SerializeObject(config));
            }
            else
            {
                MessageBox.Show("The location you entered was invalid. Dynamic wallpaper changing " +
                    "will not work until you have entered a valid location.", "Error");
            }
            
            //SunriseSunsetService service2 = new SunriseSunsetService();
            //SunriseSunsetData data2 = service2.GetWeatherData(data.lat, data.lon);
            //MessageBox.Show(data2.results.sunrise + ", " + data2.results.sunset);
        }
    }
}
