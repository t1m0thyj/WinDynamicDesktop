using System;
using System.Drawing;
using System.Management;
using System.Windows.Forms;
using Windows.System.Power;

namespace WinDynamicDesktop
{
    public partial class BrightnessDialog : Form
    {

        ManagementScope scope = new ManagementScope("root\\WMI");
        EventQuery queryWmiMonitorBrightnessEvent = new EventQuery("Select* From WmiMonitorBrightnessEvent");
        ManagementEventWatcher brightnessEventWatcher;

        public BrightnessDialog()
        {
            InitializeComponent();
            CheckDDCSupport();
            DisableNumericUpDown();
            PolarAllDayNightConfig();

            this.Font = SystemFonts.MessageBoxFont;
            this.showBrightnessNotificationToastcheckBox.Enabled = false;
            this.allDaynumericUpDown.Enabled = false;
            this.allNightnumericUpDown.Enabled = false;
        }

        private void BrightnessDialog_Load(object sender, EventArgs e)
        {
            ///
            /// Load Radio Button and Checkbox State from JSON config
            ///
            if (!JsonConfig.settings.useAutoBrightness && !JsonConfig.settings.useCustomAutoBrightness)
            {
                this.disableAutoBrightnessRadioButton.Checked = true;
            }
            else if (JsonConfig.settings.useAutoBrightness && !JsonConfig.settings.useCustomAutoBrightness)
            {
                this.enableAutoBrightnessRadioButton.Checked = true;
            }
            else if (JsonConfig.settings.useCustomAutoBrightness && !JsonConfig.settings.useAutoBrightness)
            {
                this.setCustomAutoBrightnessRadioButton.Checked = true;
            }

            this.showBrightnessNotificationToastcheckBox.Checked = JsonConfig.settings.showBrightnessChangeNotificationToast;
            ///
            /// Load NumericUpDown Values from JSON config
            ///
            this.sunriseNumericUpDown.Value = JsonConfig.settings.sunriseBrightness;
            this.dayNumericUpDown.Value = JsonConfig.settings.dayBrightness;
            this.sunsetNumericUpDown.Value = JsonConfig.settings.sunsetBrightness;
            this.nightNumericUpDown.Value = JsonConfig.settings.nightBrightness;
            this.allDaynumericUpDown.Value = JsonConfig.settings.allDayBrightness;
            this.allNightnumericUpDown.Value = JsonConfig.settings.allNightBrightness;
        }

        private void BrightnessDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (BrightnessManager.IsDDCSupported())
            {
                brightnessEventWatcher.Stop();
                brightnessEventWatcher.EventArrived -= new EventArrivedEventHandler(WmiEventHandler);
            }
        }

        private void CheckDDCSupport()
        {
            if (!BrightnessManager.IsDDCSupported())
            {
                setBrightnessGroupBox.Enabled = false;
                currentDisplayBrightnessValueLabel.Text = BrightnessManager.CurrentDisplayBrightnessValue();
                ddcErrorTextBox.Visible = true;
            }
            else
            {
                currentDisplayBrightnessValueLabel.Text = BrightnessManager.CurrentDisplayBrightnessValue();
                MonitorCurrentDisplayBrightnessValue();
            }
        }

        private void MonitorCurrentDisplayBrightnessValue()
        {
            brightnessEventWatcher = new ManagementEventWatcher(scope, queryWmiMonitorBrightnessEvent);
            brightnessEventWatcher.EventArrived += new EventArrivedEventHandler(WmiEventHandler);
            brightnessEventWatcher.Start();
        }

        private void WmiEventHandler(object sender, EventArrivedEventArgs e)
        {
            this.Invoke(new Action(() =>
                currentDisplayBrightnessValueLabel.Text = e.NewEvent.Properties["Brightness"].Value.ToString() + "%"
            ));
        }

        private void DisableNumericUpDown()
        {
            foreach (Control control in setBrightnessGroupBox.Controls)
            {
                if (control is NumericUpDown)
                {
                    control.Enabled = false;
                }
            }
        }

        private void EnableNumericUpDown()
        {
            foreach (Control control in setBrightnessGroupBox.Controls)
            {
                if (control is NumericUpDown)
                {
                    control.Enabled = true;
                    PolarAllDayNightConfig();
                }
            }
        }

        private void PolarAllDayNightConfig()
        {
            if (JsonConfig.settings.IsPolarAllDay)
            {
                this.allDaynumericUpDown.Enabled = true;
                this.allNightnumericUpDown.Enabled = false;
            }
            else if (JsonConfig.settings.IsPolarAllNight)
            {
                this.allDaynumericUpDown.Enabled = false;
                this.allNightnumericUpDown.Enabled = true;
            }
            else if (!JsonConfig.settings.IsPolarAllDay && !JsonConfig.settings.IsPolarAllNight)
            {
                this.allDaynumericUpDown.Visible = false;
                this.allNightnumericUpDown.Visible = false;
                this.label8.Visible = false;
                this.label9.Visible = false;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            SaveConfig();
            AppContext.wpEngine.RunScheduler(true);
        }

        private void SaveConfig()
        {
            // Radio Button and Checkbox Config
            JsonConfig.settings.useAutoBrightness = disableAutoBrightnessRadioButton.Checked;
            JsonConfig.settings.useAutoBrightness = enableAutoBrightnessRadioButton.Checked;
            JsonConfig.settings.useCustomAutoBrightness = setCustomAutoBrightnessRadioButton.Checked;
            JsonConfig.settings.showBrightnessChangeNotificationToast = showBrightnessNotificationToastcheckBox.Checked;

            // NumericUpDown Config
            JsonConfig.settings.sunriseBrightness = Convert.ToInt32(sunriseNumericUpDown.Value);
            JsonConfig.settings.dayBrightness = Convert.ToInt32(dayNumericUpDown.Value);
            JsonConfig.settings.sunsetBrightness = Convert.ToInt32(sunsetNumericUpDown.Value);
            JsonConfig.settings.nightBrightness = Convert.ToInt32(nightNumericUpDown.Value);
            JsonConfig.settings.allDayBrightness = Convert.ToInt32(allDaynumericUpDown.Value);
            JsonConfig.settings.allNightBrightness = Convert.ToInt32(allNightnumericUpDown.Value);

        }

        private void DisableAutoBrightnessRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            showBrightnessNotificationToastcheckBox.Checked = false;
            showBrightnessNotificationToastcheckBox.Enabled = false;
            DisableNumericUpDown();
        }

        private void EnableAutoBrightnessRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            showBrightnessNotificationToastcheckBox.Enabled = true;
            DisableNumericUpDown();

        }

        private void SetCustomAutoBrightnessRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            showBrightnessNotificationToastcheckBox.Enabled = true;
            EnableNumericUpDown();
        }

    }
}
