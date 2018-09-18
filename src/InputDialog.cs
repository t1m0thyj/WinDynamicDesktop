using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WinDynamicDesktop
{
    public partial class InputDialog : Form
    {
        public InputDialog()
        {
            InitializeComponent();

            this.FormClosing += OnFormClosing;
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

            locationCheckBox.Checked = JsonConfig.settings.useWindowsLocation;
            locationCheckBox.Enabled = UwpDesktop.IsRunningAsUwp();
        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            okButton.Enabled = inputBox.TextLength > 0;
        }

        private async void locationCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            locationCheckBox.Enabled = false;

            if (locationCheckBox.Checked)
            {
                locationCheckBox.CheckedChanged -= locationCheckBox_CheckedChanged;
                locationCheckBox.Checked = false;

                bool accessGranted = await UwpLocation.RequestAccess(this);

                if (accessGranted)
                {
                    JsonConfig.settings.useWindowsLocation = true;
                    locationCheckBox.Checked = true;
                    inputBox.Enabled = false;
                    okButton.Enabled = true;
                }

                locationCheckBox.CheckedChanged += locationCheckBox_CheckedChanged;
            }
            else
            {
                JsonConfig.settings.useWindowsLocation = false;
                inputBox.Enabled = true;
            }

            locationCheckBox.Enabled = true;
            JsonConfig.SaveConfig();
        }

        private async void okButton_Click(object sender, EventArgs e)
        {
            okButton.Enabled = false;

            if (!locationCheckBox.Checked)
            {
                LocationIQData data = LocationIQService.GetLocationData(inputBox.Text);

                if (data != null)
                {
                    JsonConfig.settings.location = inputBox.Text;
                    JsonConfig.settings.latitude = data.lat;
                    JsonConfig.settings.longitude = data.lon;
                    JsonConfig.SaveConfig();

                    if (ThemeManager.isReady)
                    {
                        AppContext.wcsService.RunScheduler();
                    }

                    MessageBox.Show("Location set successfully to: " + data.display_name +
                        "\n(Latitude = " + data.lat + ", Longitude = " + data.lon + ")", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.Close();
                }
                else
                {
                    MessageBox.Show("The location you entered was invalid, or you are not " +
                        "connected to the Internet.", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            else
            {
                bool locationUpdated = await UwpLocation.UpdateGeoposition();

                if (locationUpdated)
                {
                    if (ThemeManager.isReady)
                    {
                        AppContext.wcsService.RunScheduler();
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to get location from Windows location service.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            okButton.Enabled = true;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (JsonConfig.settings.location == null && !locationCheckBox.Checked)
            {
                DialogResult result = MessageBox.Show("WinDynamicDesktop cannot display " +
                    "wallpapers until you have entered a valid location, so that it can " +
                    "calculate sunrise and sunset times for your location. Are you sure you want " +
                    "to cancel and quit the program?", "Question", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    Environment.Exit(0);
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
