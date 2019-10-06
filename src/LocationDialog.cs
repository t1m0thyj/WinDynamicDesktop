﻿// This Source Code Form is subject to the terms of the Mozilla Public
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
    public partial class LocationDialog : Form
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;

        public LocationDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;
        }

        public void HandleLocationChange()
        {
            AppContext.wpEngine.RunScheduler();
            this.Close();
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
            locationCheckBox.Visible = UwpDesktop.IsRunningAsUwp();

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
                LocationIQService.GetLocationData(inputBox.Text, this);
            }
            else
            {
                bool locationUpdated = await UwpLocation.UpdateGeoposition();

                if (locationUpdated)
                {
                    HandleLocationChange();
                }
                else
                {
                    MessageBox.Show(_("Failed to get location from Windows location service."),
                        _("Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                DialogResult result = MessageBox.Show(_("WinDynamicDesktop cannot display " +
                    "wallpapers until you have entered a valid location, so that it can " +
                    "calculate sunrise and sunset times for your location. Are you sure you " +
                    "want to cancel and quit the program?"), _("Question"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

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
