// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace WinDynamicDesktop
{
    public partial class ScheduleDialog : Form
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private bool hasLocationPermission = false;

        public ScheduleDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;
        }

        public void HandleScheduleChange()
        {
            AppContext.wpEngine.RunScheduler();
            this.Close();
        }

        private void UpdateGuiState()
        {
            locationLabel.Enabled = radioButton1.Checked;
            locationBox.Enabled = radioButton1.Checked;

            locationPermissionLabel.Enabled = radioButton2.Checked;
            grantPermissionButton.Enabled = radioButton2.Checked;
            checkPermissionButton.Enabled = radioButton2.Checked;

            sunriseTimeLabel.Enabled = radioButton3.Checked;
            sunriseTimePicker.Enabled = radioButton3.Checked;
            sunsetTimeLabel.Enabled = radioButton3.Checked;
            sunsetTimePicker.Enabled = radioButton3.Checked;
            sunriseSunsetDurationLabel.Enabled = radioButton3.Checked;
            sunriseSunsetDurationBox.Enabled = radioButton3.Checked;

            if (radioButton2.Enabled)
            {
                if (!hasLocationPermission)
                {
                    locationPermissionLabel.ForeColor = SystemColors.ControlText;
                    locationPermissionLabel.Text = _("Click below to grant permission to access location");
                }
                else
                {
                    locationPermissionLabel.ForeColor = Color.Green;
                    locationPermissionLabel.Text = _("✓ Location access allowed");
                }
            }

            sunriseTimePicker.MaxDate = sunsetTimePicker.Value.AddHours(-1);
            sunsetTimePicker.MinDate = sunriseTimePicker.Value.AddHours(1);

            bool isInputValid = radioButton3.Checked;
            if (radioButton1.Checked)
            {
                isInputValid = locationBox.TextLength > 0;
            }
            else if (radioButton2.Checked)
            {
                isInputValid = hasLocationPermission;
            }
            okButton.Enabled = isInputValid;
        }

        private void UpdateLocationState()
        {
            if (hasLocationPermission)
            {
                UpdateGuiState();
            }
            else
            {
                locationPermissionLabel.ForeColor = Color.Red;
                locationPermissionLabel.Text = _("✗ Location access denied");
            }
        }

        private void InputDialog_Load(object sender, EventArgs e)
        {
            if (JsonConfig.settings.location != null)
            {
                locationBox.Text = JsonConfig.settings.location;
            }

            radioButton2.Enabled = UwpDesktop.IsRunningAsUwp();
            hasLocationPermission = UwpLocation.HasAccess();

            if (JsonConfig.settings.sunriseTime != null && JsonConfig.settings.sunsetTime != null)
            {
                sunriseTimePicker.Value = DateTime.Parse(JsonConfig.settings.sunriseTime);
                sunsetTimePicker.Value = DateTime.Parse(JsonConfig.settings.sunsetTime);
            }
            else
            {
                sunriseTimePicker.Value = DateTime.Today.AddHours(6);
                sunsetTimePicker.Value = DateTime.Today.AddHours(18);
            }

            sunriseTimePicker.MinDate = sunriseTimePicker.Value.Date;
            sunriseTimePicker.MaxDate = sunsetTimePicker.Value.Date.AddHours(24);

            if (JsonConfig.settings.sunriseSunsetDuration > 0)
            {
                sunriseSunsetDurationBox.Value = JsonConfig.settings.sunriseSunsetDuration;
            }

            if (JsonConfig.settings.useWindowsLocation)
            {
                radioButton2.Checked = true;
            }
            else if (JsonConfig.settings.dontUseLocation)
            {
                radioButton3.Checked = true;
            }

            UpdateGuiState();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGuiState();  // TODO Can these 3 be combined?
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGuiState();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGuiState();
        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            UpdateGuiState();
        }

        private async void grantPermissionButton_Click(object sender, EventArgs e)
        {
            UpdateLocationState();
            await UwpLocation.RequestAccess();
        }

        private void checkPermissionButton_Click(object sender, EventArgs e)
        {
            hasLocationPermission = UwpLocation.HasAccess();
            UpdateLocationState();
        }

        private void sunriseTimePicker_ValueChanged(object sender, EventArgs e)
        {
            UpdateGuiState();
        }

        private void sunsetTimePicker_ValueChanged(object sender, EventArgs e)
        {
            UpdateGuiState();
        }

        private async void okButton_Click(object sender, EventArgs e)
        {
            okButton.Enabled = false;
            JsonConfig.settings.useWindowsLocation = radioButton2.Checked;
            JsonConfig.settings.dontUseLocation = radioButton3.Checked;

            if (radioButton1.Checked)
            {
                LocationIQService.GetLocationData(locationBox.Text, this);
            }
            else if (radioButton2.Checked)
            {
                bool locationUpdated = await UwpLocation.UpdateGeoposition();

                if (locationUpdated)
                {
                    HandleScheduleChange();
                }
                else
                {
                    MessageDialog.ShowWarning(_("Failed to get location from Windows location " +
                        "service."), _("Error"));
                }
            }
            else if (radioButton3.Checked)
            {
                JsonConfig.settings.sunriseTime = sunriseTimePicker.Value.ToLongTimeString();
                JsonConfig.settings.sunsetTime = sunsetTimePicker.Value.ToLongTimeString();
                JsonConfig.settings.sunriseSunsetDuration = (int)sunriseSunsetDurationBox.Value;
                this.Close();
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
                DialogResult result = MessageDialog.ShowQuestion(_("WinDynamicDesktop cannot " +
                    "display wallpapers until you have entered a valid location, so that it can " +
                    "calculate sunrise and sunset times for your location. Are you sure you " +
                    "want to cancel and quit the program?"), _("Question"), true);

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
