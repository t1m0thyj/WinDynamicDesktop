using System;
using System.Drawing;
using System.Windows.Forms;
using Windows.System.Power;

namespace WinDynamicDesktop
{
    public partial class BrightnessDialog : Form
    {

        private static string[] brightnessValueItems = new string[]
        {
            "100", "95", "90", "85", "80", "75", "70", "65", "60", "55", "50",
            "45", "40", "35", "30", "25", "20", "15", "10", "5", "0"
        };
        public BrightnessDialog()
        {
            InitializeComponent();
            CheckDDCSupport();
            DisableComboBox();

            this.Font = SystemFonts.MessageBoxFont;
            this.enableAutoBrightnesscheckBox.Checked = false;
            this.setBrightnessAutocheckBox.Enabled = false;
            this.setBrightnessCustomcheckBox.Enabled = false;
            this.showBrightnessNotificationToastcheckBox.Enabled = false;

        }

        private void BrightnessDialog_Load(object sender, EventArgs e)
        {
            ///
            /// Populate Combobox items
            ///
            this.sunriseComboBox.Items.AddRange(brightnessValueItems);
            this.dayComboBox.Items.AddRange(brightnessValueItems);
            this.sunsetComboBox.Items.AddRange(brightnessValueItems);
            this.nightComboBox.Items.AddRange(brightnessValueItems);
            this.allDayComboBox.Items.AddRange(brightnessValueItems);
            this.allNightComboBox.Items.AddRange(brightnessValueItems);
            ///
            /// Load Checkbox States from JSON config
            ///
            this.enableAutoBrightnesscheckBox.Checked = JsonConfig.settings.useAutoBrightness;
            this.setBrightnessAutocheckBox.Checked = JsonConfig.settings.useAutoMode;
            this.setBrightnessCustomcheckBox.Checked = JsonConfig.settings.useCustomMode;
            this.showBrightnessNotificationToastcheckBox.Checked = JsonConfig.settings.showBrightnessChangeNotificationToast;
            ///
            /// Load Combobox Values from JSON config
            ///
            this.allNightComboBox.Text = JsonConfig.settings.allNightBrightness.ToString();
            this.allDayComboBox.Text = JsonConfig.settings.allNightBrightness.ToString();
            this.sunriseComboBox.Text = JsonConfig.settings.sunriseBrightness.ToString();
            this.dayComboBox.Text = JsonConfig.settings.dayBrightness.ToString();
            this.sunsetComboBox.Text = JsonConfig.settings.sunsetBrightness.ToString();
            this.nightComboBox.Text = JsonConfig.settings.nightBrightness.ToString();
        }

        private void CheckDDCSupport()
        {
            if (!BrightnessManager.IsDDCSupported())
            {
                setBrightnessGroupBox.Enabled = false;
                ddcSupportLabel.Text = BrightnessManager.GetDDCStatus();
                currentDisplayBrightnessValueLabel.Text = BrightnessManager.CurrentDisplayBrightnessValue();
                ddcErrorTextBox.Visible = true;
            }
            else
            {
                ddcSupportLabel.Text = BrightnessManager.GetDDCStatus();
                currentDisplayBrightnessValueLabel.Text = BrightnessManager.CurrentDisplayBrightnessValue();
            }
        }

        private void EnableAutoBrightnesscheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!enableAutoBrightnesscheckBox.Checked)
            {
                setBrightnessAutocheckBox.Enabled = false;
                setBrightnessCustomcheckBox.Enabled = false;

                setBrightnessAutocheckBox.Checked = false;
                setBrightnessCustomcheckBox.Checked = false;

                showBrightnessNotificationToastcheckBox.Checked = false;
                showBrightnessNotificationToastcheckBox.Enabled = false;

                DisableComboBox();
            }
            else
            {
                setBrightnessAutocheckBox.Enabled = true;
                setBrightnessCustomcheckBox.Enabled = true;

                setBrightnessAutocheckBox.Checked = true;
                setBrightnessAutocheckBox.Checked = true;

                showBrightnessNotificationToastcheckBox.Checked = JsonConfig.settings.showBrightnessChangeNotificationToast;
                showBrightnessNotificationToastcheckBox.Enabled = true;
            }
        }

        private void SetBrightnessAutocheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (setBrightnessAutocheckBox.Checked)
            {
                setBrightnessCustomcheckBox.Checked = false;
                DisableComboBox();
            }
        }

        private void SetBrightnessCustomcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (setBrightnessCustomcheckBox.Checked)
            {
                setBrightnessAutocheckBox.Checked = false;
                EnableComboBox();
            }
            else
            {
                DisableComboBox();
            }
        }

        private void DisableComboBox()
        {
            foreach (Control control in setBrightnessGroupBox.Controls)
            {
                if (control is ComboBox)
                {
                    control.Enabled = false;
                }
            }
        }

        private void EnableComboBox()
        {
            foreach (Control control in setBrightnessGroupBox.Controls)
            {
                if (control is ComboBox)
                {
                    control.Enabled = true;
                }
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
            currentDisplayBrightnessValueLabel.Text = BrightnessManager.CurrentDisplayBrightnessValue();
        }

        private void SaveConfig()
        {
            if (!enableAutoBrightnesscheckBox.Checked)
            {
                JsonConfig.settings.useAutoBrightness = false;
                JsonConfig.settings.useAutoMode = false;
                JsonConfig.settings.useCustomMode = false;
                JsonConfig.settings.showBrightnessChangeNotificationToast = false;
            }
            else
            {

                if (!setBrightnessAutocheckBox.Checked && !setBrightnessCustomcheckBox.Checked)
                {
                    MessageBox.Show("Please select an option", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    JsonConfig.settings.useAutoBrightness = true;

                    if (!setBrightnessAutocheckBox.Checked)
                    {
                        JsonConfig.settings.useAutoMode = false;
                    }
                    else
                    {
                        JsonConfig.settings.useAutoMode = true;
                    }


                    if (!setBrightnessCustomcheckBox.Checked)
                    {
                        JsonConfig.settings.useCustomMode = false;
                    }

                    else
                    {
                        JsonConfig.settings.useCustomMode = true;
                        JsonConfig.settings.allDayBrightness = Int32.Parse(allDayComboBox.Text);
                        JsonConfig.settings.allNightBrightness = Int32.Parse(allNightComboBox.Text);
                        JsonConfig.settings.sunriseBrightness = Int32.Parse(sunriseComboBox.Text);
                        JsonConfig.settings.dayBrightness = Int32.Parse(dayComboBox.Text);
                        JsonConfig.settings.sunsetBrightness = Int32.Parse(sunsetComboBox.Text);
                        JsonConfig.settings.nightBrightness = Int32.Parse(nightComboBox.Text);
                    }

                    if (!showBrightnessNotificationToastcheckBox.Checked)
                    {
                        JsonConfig.settings.showBrightnessChangeNotificationToast = false;
                    }
                    else
                    {
                        JsonConfig.settings.showBrightnessChangeNotificationToast = true;
                    }

                }
            }
        }
    }
}
