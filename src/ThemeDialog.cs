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

        private Bitmap ShrinkImage(string filename, int width, int height)
        {
            // Image scaling code from https://stackoverflow.com/a/7677163/5504760
            using (var tempImage = Image.FromFile(filename))
            {
                Bitmap bmp = new Bitmap(width, height);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(tempImage, new Rectangle(0, 0, bmp.Width, bmp.Height));
                }

                return bmp;
            }
        }

        private Bitmap GetThumbnailImage(ThemeConfig theme, int width, int height)
        {
            int imageId1 = theme.dayImageList[(theme.dayImageList.Length + 1) / 2];
            string imageFilename1 = theme.imageFilename.Replace("*", imageId1.ToString());

            int imageId2 = theme.nightImageList[(theme.nightImageList.Length + 1) / 2];
            string imageFilename2 = theme.imageFilename.Replace("*", imageId2.ToString());

            using (var bmp1 = ShrinkImage(Path.Combine("images", imageFilename1), width, height))
            {
                Bitmap bmp2 = ShrinkImage(Path.Combine("images", imageFilename2), width, height);

                using (Graphics g = Graphics.FromImage(bmp2))
                {
                    g.DrawImage(bmp1, 0, 0, new Rectangle(0, 0, bmp1.Width / 2, bmp1.Height),
                        GraphicsUnit.Pixel);
                }

                return bmp2;
            }
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
            int w = pictureBox1.Size.Width;
            int h = pictureBox1.Size.Height;

            if (selectedIndex == 0)
            {
                pictureBox1.Image = ShrinkImage(windowsWallpaper, w, h);
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
                pictureBox1.Image = ShrinkImage(Path.Combine("images", imageFilename), w, h);
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
            string currentTheme = JsonConfig.settings.themeName ?? "Mojave_Desert";

            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(192, 108);
            listView1.LargeImageList = imageList;

            imageList.Images.Add(ShrinkImage(windowsWallpaper, 192, 108));
            listView1.Items.Add("None", 0);

            for (int i = 0; i < ThemeManager.themeSettings.Count; i++)
            {
                ThemeConfig theme = ThemeManager.themeSettings[i];
                imageList.Images.Add(GetThumbnailImage(theme, 192, 108));
                listView1.Items.Add(theme.themeName.Replace('_', ' '), i + 1);

                if (theme.themeName == currentTheme)
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
            string themeName = listView1.SelectedItems[0].Text.Replace(' ', '_');
            JsonConfig.settings.themeName = themeName;
            JsonConfig.settings.darkMode = darkModeCheckbox.Checked;

            if (selectedIndex > 0)
            {
                ThemeManager.currentTheme = ThemeManager.themeSettings[selectedIndex - 1];
            }
            else
            {
                ThemeManager.currentTheme = null;
            }

            MainMenu.darkModeItem.Checked = JsonConfig.settings.darkMode;
            this.Hide();

            if (selectedIndex > 0)
            {
                AppContext.wcsService.LoadImageLists();

                if (LocationManager.isReady)
                {
                    AppContext.wcsService.RunScheduler();
                }
            }
            else
            {
                UwpDesktop.GetHelper().SetWallpaper(windowsWallpaper);
            }

            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (JsonConfig.firstRun && ThemeManager.currentTheme == null)
            {
                DialogResult result = MessageBox.Show("WinDynamicDesktop cannot dynamically " +
                    "update your wallpaper until you have selected a theme. Are you sure you " +
                    "want to continue without a theme selected?", "Question",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
