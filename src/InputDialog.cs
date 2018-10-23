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
        public InputDialog()
        {
            InitializeComponent();

            this.FormClosing += OnFormClosing;
        }

        private void UpdateGuiState()
        {
            inputBox.Enabled = !locationCheckBox.Checked;
            okButton.Enabled = inputBox.TextLength > 0 || locationCheckBox.Checked;
        }

        private void InputDialog_Load(object sender, EventArgs e)
        {
            if (JsonConfig.settings.location != null)
            {
                inputBox.Text = JsonConfig.settings.location;
            }

            locationCheckBox.Checked = JsonConfig.settings.useWindowsLocation;
            locationCheckBox.Enabled = UwpDesktop.IsRunningAsUwp();

            UpdateGuiState();
            locationCheckBox.CheckedChanged += locationCheckBox_CheckedChanged;
        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            UpdateGuiState();
        }

        private async void locationCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            locationCheckBox.Enabled = false;
            okButton.Enabled = false;

            if (locationCheckBox.Checked)
            {
                locationCheckBox.CheckedChanged -= locationCheckBox_CheckedChanged;
                locationCheckBox.Checked = false;

                bool hasAccess = await UwpLocation.RequestAccess();

                if (hasAccess)
                {
                    JsonConfig.settings.useWindowsLocation = true;
                    locationCheckBox.Checked = true;
                }

                locationCheckBox.CheckedChanged += locationCheckBox_CheckedChanged;
            }
            else
            {
                JsonConfig.settings.useWindowsLocation = false;
            }

            locationCheckBox.Enabled = true;
            UpdateGuiState();
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

                    this.Hide();

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
                this.Hide();
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

                    this.Show();
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
            if (JsonConfig.settings.latitude == null || JsonConfig.settings.longitude == null)
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
