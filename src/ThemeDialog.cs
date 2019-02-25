using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO;

namespace WinDynamicDesktop
{
    public partial class ThemeDialog : Form
    {
        private int maxImageNumber;
        private int previewImage;
        private int selectedIndex;

        private const string themeLink =
            "https://github.com/t1m0thyj/WinDynamicDesktop/wiki/Community-created-themes";
        private readonly string windowsWallpaper = Directory.GetFiles(
            @"C:\Windows\Web\Wallpaper\Windows")[0];

        public ThemeDialog()
        {
            InitializeComponent();

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;

            int extraWidth = GetThumbnailSize(false).Width * 2 - 384;

            if (extraWidth > 0)
            {
                this.listView1.Size = new Size(this.listView1.Width + extraWidth,
                    this.listView1.Height);
                this.Size = new Size(this.Width + extraWidth, this.Height);
            }
        }

        public void ImportThemes(List<string> themePaths)
        {
            ProgressDialog importDialog = new ProgressDialog();
            importDialog.FormClosing += OnImportDialogClosing;
            importDialog.Show();
            importDialog.InitImport(themePaths);
        }

        private Size GetThumbnailSize(bool scaleFont = true)
        {
            int scaledWidth;

            using (Graphics g = this.CreateGraphics())
            {
                scaledWidth = (int)(192 * g.DpiX / 96);
            }

            if (scaleFont)
            {
                scaledWidth = (int)(scaledWidth * this.AutoScaleDimensions.Width / 7);
            }

            if (scaledWidth > 256)
            {
                scaledWidth = 256;
            }

            Size scaledSize = new Size(scaledWidth, scaledWidth * 9 / 16);
            return scaledSize;
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

        private Image GetThumbnailImage(ThemeConfig theme, Size size)
        {
            string thumbnailPath = Path.Combine("themes", theme.themeId, "thumbnail.png");

            if (File.Exists(thumbnailPath))
            {
                return Image.FromFile(thumbnailPath);
            }

            int imageId1 = theme.dayImageList[theme.dayImageList.Length / 2];
            string imageFilename1 = theme.imageFilename.Replace("*", imageId1.ToString());

            int imageId2 = theme.nightImageList[theme.nightImageList.Length / 2];
            string imageFilename2 = theme.imageFilename.Replace("*", imageId2.ToString());

            using (var bmp1 = ShrinkImage(Path.Combine("themes", theme.themeId, imageFilename1),
                size.Width, size.Height))
            {
                Bitmap bmp2 = ShrinkImage(Path.Combine("themes", theme.themeId, imageFilename2),
                    size.Width, size.Height);

                using (Graphics g = Graphics.FromImage(bmp2))
                {
                    g.DrawImage(bmp1, 0, 0, new Rectangle(0, 0, bmp1.Width / 2, bmp1.Height),
                        GraphicsUnit.Pixel);
                }

                bmp2.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Png);

                return bmp2;
            }
        }

        private string GetThemeName(ThemeConfig theme)
        {
            if (theme.displayName != null)
            {
                return theme.displayName;
            }

            return theme.themeId.Replace('_', ' ');
        }

        private string GetCreditsText()
        {
            if (selectedIndex > 0)
            {
                ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];

                if (theme.imageCredits != null)
                {
                    return "Image Credits: " + theme.imageCredits;
                }
            }
            else
            {
                return "Image Credits: Microsoft";
            }

            return "";
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
                    max += theme.sunriseImageList.Length + theme.dayImageList.Length +
                        theme.sunsetImageList.Length;
                }
            }

            return max;
        }

        private void LoadPreviewImage(int imageNumber)
        {
            int width = pictureBox1.Size.Width;
            int height = pictureBox1.Size.Height;
            Rectangle screen = Screen.FromControl(this).Bounds;

            if ((screen.Y / (double)screen.X) <= (height / (double)width))
            {
                height = width * 9 / 16;
            }
            else
            {
                width = height * 16 / 9;
            }

            if (selectedIndex == 0)
            {
                pictureBox1.Image = ShrinkImage(windowsWallpaper, width, height);
            }
            else
            {
                ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
                int imageId;

                if (!darkModeCheckbox.Checked)
                {
                    List<int> imageList = new List<int>();
                    imageList.AddRange(theme.sunriseImageList);
                    imageList.AddRange(theme.dayImageList);
                    imageList.AddRange(theme.sunsetImageList);
                    imageList.AddRange(theme.nightImageList);
                    imageId = imageList[imageNumber - 1];
                }
                else
                {
                    imageId = theme.nightImageList[imageNumber - 1];
                }

                string imageFilename = theme.imageFilename.Replace("*", imageId.ToString());
                pictureBox1.Image = ShrinkImage(Path.Combine("themes", theme.themeId,
                    imageFilename), width, height);
            }

            imageNumberLabel.Text = "Image " + imageNumber + " of " + maxImageNumber;
            firstButton.Enabled = imageNumber > 1;
            previousButton.Enabled = imageNumber > 1;
            nextButton.Enabled = imageNumber < maxImageNumber;
            lastButton.Enabled = imageNumber < maxImageNumber;

            previewImage = imageNumber;
        }

        private void EnsureThemeNotDuplicated(string themeId)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                if ((string)item.Tag == themeId)
                {
                    int imageIndex = item.ImageIndex;
                    listView1.Items.Remove(item);
                    listView1.LargeImageList.Images[imageIndex].Dispose();
                    break;
                }
            }
        }

        private void LoadImportedThemes(List<ThemeConfig> themes)
        {
            themes.Sort((t1, t2) => t1.themeId.CompareTo(t2.themeId));
            List<ThemeConfig> missingThemes = ThemeManager.FindMissingThemes();
            Size thumbnailSize = GetThumbnailSize();
            ListViewItem newItem = null;

            for (int i = 0; i < themes.Count; i++)
            {
                if (missingThemes.IndexOf(themes[i]) == -1)
                {
                    EnsureThemeNotDuplicated(themes[i].themeId);

                    listView1.LargeImageList.Images.Add(GetThumbnailImage(themes[i],
                        thumbnailSize));
                    newItem = listView1.Items.Add(GetThemeName(themes[i]),
                        listView1.LargeImageList.Images.Count - 1);
                    newItem.Tag = themes[i].themeId;
                }
                else
                {
                    MessageBox.Show("Failed to download images for the " +
                        GetThemeName(themes[i]) + " theme.", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    Task.Run(() => ThemeManager.RemoveTheme(themes[i]));
                }
            }

            if (newItem != null)
            {
                listView1.Sort();
                newItem.Selected = true;
                newItem.EnsureVisible();
            }
        }

        private void ThemeDialog_Load(object sender, EventArgs e)
        {
            listView1.ContextMenuStrip = contextMenuStrip1;
            listView1.ListViewItemSorter = new CompareByItemText(listView1);

            darkModeCheckbox.Checked = JsonConfig.settings.darkMode;
            applyButton.Enabled = LocationManager.isReady;

            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            Size thumbnailSize = GetThumbnailSize();
            imageList.ImageSize = thumbnailSize;
            listView1.LargeImageList = imageList;

            imageList.Images.Add(ShrinkImage(windowsWallpaper, thumbnailSize.Width,
                thumbnailSize.Height));
            ListViewItem newItem = listView1.Items.Add("None", 0);

            string currentTheme = ThemeManager.currentTheme?.themeId;

            if (currentTheme == null)
            {
                if (JsonConfig.firstRun || JsonConfig.settings.themeName != null)
                {
                    currentTheme = "Mojave_Desert";
                }
                else
                {
                    newItem.Selected = true;
                }
            }

            for (int i = 0; i < ThemeManager.themeSettings.Count; i++)
            {
                ThemeConfig theme = ThemeManager.themeSettings[i];
                imageList.Images.Add(GetThumbnailImage(theme, thumbnailSize));
                newItem = listView1.Items.Add(GetThemeName(theme), i + 1);
                newItem.Tag = theme.themeId;

                if (theme.themeId == currentTheme)
                {
                    newItem.Selected = true;
                }
            }

            listView1.Sort();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                selectedIndex = listView1.SelectedIndices[0];
                if (selectedIndex > 0)
                {
                    string themeId = (string)listView1.Items[selectedIndex].Tag;
                    selectedIndex = ThemeManager.themeSettings.FindIndex(
                        t => t.themeId == themeId) + 1;
                }

                creditsLabel.Text = GetCreditsText();
                maxImageNumber = GetMaxImageNumber();
                LoadPreviewImage(1);

                applyButton.Enabled = true;
            }
            else
            {
                applyButton.Enabled = false;
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

            ImportThemes(openFileDialog1.FileNames.ToList());

            openFileDialog1.InitialDirectory = Path.GetDirectoryName(openFileDialog1.FileNames[0]);
            openFileDialog1.FileName = "";
        }

        private void themeLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(themeLink);
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            applyButton.Enabled = false;

            if (selectedIndex > 0)
            {
                ThemeManager.currentTheme = ThemeManager.themeSettings[selectedIndex - 1];
            }
            else
            {
                ThemeManager.currentTheme = null;
            }

            JsonConfig.settings.themeName = ThemeManager.currentTheme?.themeId;
            JsonConfig.settings.darkMode = darkModeCheckbox.Checked;
            MainMenu.darkModeItem.Checked = JsonConfig.settings.darkMode;

            if (selectedIndex == 0)
            {
                WallpaperApi.SetWallpaper(windowsWallpaper);
            }
            else if (LocationManager.isReady)
            {
                AppContext.wpEngine.RunScheduler();
            }

            applyButton.Enabled = true;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var hitTestInfo = listView1.HitTest(listView1.PointToClient(Cursor.Position));
            int itemIndex = hitTestInfo.Item?.Index ?? -1;

            if (itemIndex <= 0 || ThemeManager.defaultThemes.Contains(
                (string)listView1.Items[itemIndex].Tag))
            {
                e.Cancel = true;
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int itemIndex = listView1.FocusedItem.Index;
            string themeId = (string)listView1.Items[itemIndex].Tag;
            ThemeConfig theme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);

            DialogResult result = MessageBox.Show("Are you sure you want to remove the " +
                GetThemeName(theme) + " theme?", "Question", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                int imageIndex = listView1.Items[itemIndex].ImageIndex;
                listView1.Items.RemoveAt(itemIndex);
                listView1.Items[itemIndex - 1].Selected = true;
                listView1.LargeImageList.Images[imageIndex].Dispose();

                Task.Run(() => ThemeManager.RemoveTheme(theme));
            }
        }

        private void OnImportDialogClosing(object sender, FormClosingEventArgs e)
        {
            LoadImportedThemes(ThemeManager.importedThemes);
            ThemeManager.importedThemes.Clear();

            this.Enabled = true;
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
                    this.Show();
                }
            }
        }
    }

    // Class to force ListView control to sort correctly
    // Code from https://stackoverflow.com/a/30536933/5504760
    public class CompareByItemText : IComparer
    {
        private readonly ListView _listView;

        public CompareByItemText(ListView listView)
        {
            this._listView = listView;
        }

        public int Compare(object x, object y)
        {
            ListViewItem item1 = (ListViewItem)x;
            ListViewItem item2 = (ListViewItem)y;
            string a = (item1.Tag != null) ? item1.Text : "\0";
            string b = (item2.Tag != null) ? item2.Text : "\0";
            return a.CompareTo(b);
        }
    }
}
