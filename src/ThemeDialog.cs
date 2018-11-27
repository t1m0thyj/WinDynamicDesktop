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
using DarkUI.Forms;

namespace WinDynamicDesktop
{
    public partial class ThemeDialog : DarkForm
    {
        private int maxImageNumber;
        private int previewImage;
        private int selectedIndex;
        private ThemeConfig tempTheme;

        private const string themeLink = 
            "https://github.com/t1m0thyj/WinDynamicDesktop/wiki/Community-created-themes";
        private readonly string windowsWallpaper = Directory.GetFiles(
            @"C:\Windows\Web\Wallpaper\Windows")[0];

        public ThemeDialog()
        {
            InitializeComponent();

            this.FormClosing += OnFormClosing;
            listView1.ContextMenuStrip = contextMenuStrip1;
            listView1.ListViewItemSorter = new CompareByIndex(listView1);
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

        private async void LoadImportedTheme()
        {
            ThemeManager.themeSettings.Add(tempTheme);
            ThemeManager.themeSettings.Sort((t1, t2) => t1.themeName.CompareTo(t2.themeName));

            List<ThemeConfig> missingThemes = ThemeManager.FindMissingThemes();
            bool isInstalled = missingThemes.IndexOf(tempTheme) == -1;

            if (isInstalled)
            {
                int itemIndex = ThemeManager.themeSettings.IndexOf(tempTheme) + 1;
                listView1.LargeImageList.Images.Add(GetThumbnailImage(tempTheme, 192, 108));
                listView1.Items.Insert(itemIndex, tempTheme.themeName.Replace('_', ' '),
                    listView1.LargeImageList.Images.Count - 1);
                listView1.Items[itemIndex].Selected = true;
            }
            else
            {
                DarkMessageBox.ShowWarning("Failed to install the '" +
                    tempTheme.themeName.Replace('_', ' ') + "' theme.", "Error");
                await Task.Run(() => ThemeManager.RemoveTheme(tempTheme));
            }

            tempTheme = null;
        }

        private void ThemeDialog_Load(object sender, EventArgs e)
        {
            darkModeCheckbox.Checked = JsonConfig.settings.darkMode;
            string currentTheme = ThemeManager.currentTheme?.themeName;

            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(192, 108);
            listView1.LargeImageList = imageList;

            imageList.Images.Add(ShrinkImage(windowsWallpaper, 192, 108));
            listView1.Items.Add("None", 0);

            if (currentTheme == null)
            {
                if (JsonConfig.firstRun || JsonConfig.settings.themeName != null)
                {
                    currentTheme = "Mojave_Desert";
                }
                else
                {
                    listView1.Items[0].Selected = true;
                }
            }

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

        private void importButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            string themePath = openFileDialog1.FileName;
            tempTheme = ThemeManager.ImportTheme(themePath);

            if (tempTheme == null)
            {
                return;
            }
            else if (Path.GetExtension(themePath) == ".zip")
            {
                LoadImportedTheme();
            }
            else
            {
                ProgressDialog downloadDialog = new ProgressDialog();
                downloadDialog.FormClosed += OnDownloadDialogClosed;
                downloadDialog.Show();

                this.Enabled = false;
                downloadDialog.LoadQueue(new List<ThemeConfig>() { tempTheme });
                downloadDialog.DownloadNext();
            }
        }

        private void themeLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(themeLink);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            okButton.Enabled = false;

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
                WallpaperApi.SetWallpaper(windowsWallpaper);
            }

            okButton.Enabled = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var hitTestInfo = listView1.HitTest(listView1.PointToClient(Cursor.Position));
            int itemIndex = hitTestInfo.Item?.Index ?? -1;

            if (itemIndex <= 0 || ThemeManager.defaultThemes.Contains(
                ThemeManager.themeSettings[itemIndex - 1].themeName))
            {
                e.Cancel = true;
            }
        }

        private async void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int itemIndex = listView1.FocusedItem.Index;
            ThemeConfig theme = ThemeManager.themeSettings[itemIndex - 1];

            DialogResult result = DarkMessageBox.ShowWarning("Are you sure you want to remove " +
                "the '" + theme.themeName.Replace('_', ' ') + "' theme?", "Question",
                DarkDialogButton.YesNo);

            if (result == DialogResult.Yes)
            {
                listView1.Items.RemoveAt(itemIndex);
                listView1.Items[itemIndex - 1].Selected = true;

                await Task.Run(() => ThemeManager.RemoveTheme(theme));
            }
        }

        private void OnDownloadDialogClosed(object sender, EventArgs e)
        {
            this.Enabled = true;
            LoadImportedTheme();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (JsonConfig.firstRun && ThemeManager.currentTheme == null)
            {
                DialogResult result = DarkMessageBox.ShowWarning("WinDynamicDesktop cannot " +
                    "dynamically update your wallpaper until you have selected a theme. Are you " +
                    "sure you want to continue without a theme selected?", "Question",
                    DarkDialogButton.YesNo);

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.Show();
                }
            }
        }
    }

    // Class to force ListView control to sort correctly
    // Code from https://stackoverflow.com/a/30536933/5504760
    public class CompareByIndex : System.Collections.IComparer
    {
        private readonly ListView _listView;

        public CompareByIndex(ListView listView)
        {
            this._listView = listView;
        }

        public int Compare(object x, object y)
        {
            int i = this._listView.Items.IndexOf((ListViewItem)x);
            int j = this._listView.Items.IndexOf((ListViewItem)y);
            return i - j;
        }
    }
}
