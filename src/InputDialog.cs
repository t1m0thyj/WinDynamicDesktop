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
            if (JsonConfig.settings.location != null)
            {
                inputBox.Text = JsonConfig.settings.location;
            }
            else
            {
                okButton.Enabled = false;
            }
        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            okButton.Enabled = inputBox.TextLength > 0;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            okButton.Enabled = false;

            LocationIQService service = new LocationIQService();
            LocationIQData data = service.GetLocationData(inputBox.Text);

            if (data != null)
            {
                JsonConfig.settings.location = inputBox.Text;
                JsonConfig.settings.latitude = data.lat;
                JsonConfig.settings.longitude = data.lon;
                JsonConfig.SaveConfig();

                wcsService.StartScheduler(true);

                MessageBox.Show("Location set successfully to: " + data.display_name +
                    Environment.NewLine + "Latitude = " + data.lat + ", Longitude = " + data.lon,
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            else
            {
                MessageBox.Show("The location you entered was invalid, or you are not connected to " +
                    "the Internet", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            okButton.Enabled = true;
        }
    }
}
