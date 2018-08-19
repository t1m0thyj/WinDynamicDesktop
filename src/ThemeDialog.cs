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

namespace WinDynamicDesktop
{
    public partial class ThemeDialog : Form
    {
        private int maxImageNumber;
        private int previewImage;
        private int selectedIndex;
        private string windowsWallpaper = Directory.GetFiles(@"C:\Windows\Web\Wallpaper\Windows")[0];

        public ThemeDialog()
        {
            InitializeComponent();

            this.FormClosing += OnFormClosing;
        }

        private int GetMaxImageNumber()
        {
            int max = 1;

            if (selectedIndex > 0)
            {
                ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
                max = theme.nightImageList.Length;

                if (!darkModeCheckbox.Checked)
                {
                    max += theme.dayImageList.Length;
                }
            }

            return max;
        }

        private void LoadPreviewImage(int imageNumber)
        {
            pictureBox1.Image?.Dispose();    // Free up memory

            if (selectedIndex == 0)
            {
                pictureBox1.Image = Image.FromFile(windowsWallpaper);
            }
            else
            {
                ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
                int imageId;

                if (darkModeCheckbox.Checked)
                {
                    imageId = theme.nightImageList[imageNumber - 1];
                }
                else if (imageNumber <= theme.dayImageList.Length)
                {
                    imageId = theme.dayImageList[imageNumber - 1];
                }
                else
                {
                    imageId = theme.nightImageList[imageNumber - theme.dayImageList.Length - 1];
                }

                string imageFilename = theme.imageFilename.Replace("*", imageId.ToString());
                pictureBox1.Image = Image.FromFile(Path.Combine("images", imageFilename));
            }

            imageNumberLabel.Text = "Image " + imageNumber + " of " + maxImageNumber;
            firstButton.Enabled = imageNumber > 1;
            previousButton.Enabled = imageNumber > 1;
            nextButton.Enabled = imageNumber < maxImageNumber;
            lastButton.Enabled = imageNumber < maxImageNumber;

            previewImage = imageNumber;
        }

        private void ThemeDialog_Load(object sender, EventArgs e)
        {
            darkModeCheckbox.Checked = JsonConfig.settings.darkMode;

            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(192, 108);
            listView1.LargeImageList = imageList;

            imageList.Images.Add(Image.FromFile(windowsWallpaper));
            listView1.Items.Add("None", 0);

            for (int i = 0; i < ThemeManager.themeSettings.Count; i++)
            {
                ThemeConfig theme = ThemeManager.themeSettings[i];
                int imageId = theme.nightImageList.Last();
                string imageFilename = theme.imageFilename.Replace("*", imageId.ToString());

                imageList.Images.Add(Image.FromFile(Path.Combine("images", imageFilename)));
                listView1.Items.Add(theme.themeName.Replace('_', ' '), i + 1);

                if (theme.themeName == JsonConfig.settings.themeName)
                {
                    listView1.Items[i + 1].Selected = true;
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                selectedIndex = listView1.SelectedIndices[0];
                maxImageNumber = GetMaxImageNumber();
                
                LoadPreviewImage(1);
                okButton.Enabled = true;
            }
            else
            {
                okButton.Enabled = false;
            }
        }

        private void firstButton_Click(object sender, EventArgs e)
        {
            LoadPreviewImage(1);
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            LoadPreviewImage(previewImage - 1);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            LoadPreviewImage(previewImage + 1);
        }

        private void lastButton_Click(object sender, EventArgs e)
        {
            LoadPreviewImage(maxImageNumber);
        }

        private void darkModeCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            maxImageNumber = GetMaxImageNumber();
            LoadPreviewImage(1);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            JsonConfig.settings.themeName = listView1.SelectedItems[0].Name.Replace(' ', '_');
            ThemeManager.isReady = true;

            if (selectedIndex > 0)
            {
                ThemeManager.currentTheme = ThemeManager.themeSettings[selectedIndex - 1];
            }
            else
            {
                ThemeManager.currentTheme = null;
            }

            JsonConfig.settings.darkMode = darkModeCheckbox.Checked;
            MainMenu.darkModeItem.Checked = darkModeCheckbox.Checked;

            if (LocationManager.isReady)
            {
                AppContext.wcsService.HandleNewTheme();

                if (ThemeManager.currentTheme != null)
                {
                    AppContext.wcsService.RunScheduler(true);
                }
                else
                {
                    UwpDesktop.SetWallpaper(windowsWallpaper);
                }
            }

            JsonConfig.SaveConfig();
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (ThemeManager.currentTheme == null)
            {
                DialogResult result = MessageBox.Show("WinDynamicDesktop cannot display" +
                    "wallpapers until you have selected a theme. Are you sure you want " +
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
            else
            {
                pictureBox1.Image?.Dispose();    // Free up memory
            }
        }
    }
}
