using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WinDynamicDesktop
{
    public partial class MainForm : Form
    {
        // Hack to show watermark in textbox from https://stackoverflow.com/questions/2487104
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam,
            [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        internal WallpaperChangeScheduler wcsService;

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
            logOutput.AppendText(lineText);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SendMessage(locationInput.Handle, 0x1501, 1, "Enter your location...");
            if (JsonConfig.settings.Location != null)
            {
                locationInput.Text = JsonConfig.settings.Location;
            }

            AppendToLog("Welcome to WinDynamicDesktop Mojave Edition!");
        }

        private void setLocationButton_Click(object sender, EventArgs e)
        {
            LocationIQService service = new LocationIQService();
            LocationIQData data = service.GetLocationData(locationInput.Text);

            if (data != null)
            {
                AppendToLog("Location set successfully to: " + data.display_name);
                AppendToLog("Latitude = " + data.lat + ", Longitude= " + data.lon);

                JsonConfig.settings.Location = locationInput.Text;
                JsonConfig.settings.Latitude = data.lat;
                JsonConfig.settings.Longitude = data.lon;
                JsonConfig.SaveConfig();

                wcsService.StartScheduler();
            }
            else
            {
                MessageBox.Show("The location you entered was invalid, or you are not connected to " +
                    "the Internet", "Error");
            }
        }
    }
}
