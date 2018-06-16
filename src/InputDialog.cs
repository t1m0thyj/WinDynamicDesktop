using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public partial class InputDialog : Form
    {
        internal WallpaperChangeScheduler wcsService;

        public InputDialog()
        {
            InitializeComponent();
        }

        private void InputDialog_Load(object sender, EventArgs e)
        {
            if (JsonConfig.settings.Location != null)
            {
                inputBox.Text = JsonConfig.settings.Location;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            LocationIQService service = new LocationIQService();
            LocationIQData data = service.GetLocationData(inputBox.Text);

            if (data != null)
            {
                JsonConfig.settings.Location = inputBox.Text;
                JsonConfig.settings.Latitude = data.lat;
                JsonConfig.settings.Longitude = data.lon;
                JsonConfig.SaveConfig();

                wcsService.StartScheduler();

                MessageBox.Show("Location set successfully to: " + data.display_name +
                    Environment.NewLine + "Latitude = " + data.lat + Environment.NewLine +
                    "Longitude = " + data.lon, "Success", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.Close();
            }
            else
            {
                MessageBox.Show("The location you entered was invalid, or you are not connected to " +
                    "the Internet", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
