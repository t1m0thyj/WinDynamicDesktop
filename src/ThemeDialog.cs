// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

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
using Manina.Windows.Forms;

namespace WinDynamicDesktop
{
    public partial class ThemeDialog : Form
    {
        private int maxImageNumber;
        private int previewImage;
        private int selectedIndex;
        private List<string> themeNames = new List<string>();

        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private const string themeLink =
            "https://github.com/t1m0thyj/WinDynamicDesktop/wiki/Community-created-themes";
        private readonly string windowsWallpaper = Directory.GetFiles(
            Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Web\Wallpaper\Windows"))[0];

        public ThemeDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;

            int bestWidth = (GetThumbnailSize().Width + 30) * 2 +
                SystemInformation.VerticalScrollBarWidth;
            int oldWidth = this.imageListView1.Size.Width;
            this.imageListView1.Size = new Size(bestWidth, this.imageListView1.Height);
            this.Size = new Size(this.Width + bestWidth - oldWidth, this.Height);
            this.CenterToScreen();
        }

        public void ImportThemes(List<string> themePaths)
        {
            ProgressDialog importDialog = new ProgressDialog() { Owner = this };
            importDialog.FormClosing += OnImportDialogClosing;
            importDialog.Show();
            importDialog.InitImport(themePaths);
        }

        private Size GetThumbnailSize()
        {
            int scaledWidth;

            using (Graphics g = this.CreateGraphics())
            {
                scaledWidth = (int)(192 * g.DpiX / 96);
            }

            return new Size(scaledWidth, scaledWidth * 9 / 16);
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

        private Image GetThumbnailImage(ThemeConfig theme, Size size, bool useCache = true)
        {
            string thumbnailPath = Path.Combine("themes", theme.themeId, "thumbnail.png");

            if (useCache && File.Exists(thumbnailPath))
            {
                Image cachedImage = Image.FromFile(thumbnailPath);

                if (cachedImage.Size == size)
                {
                    return cachedImage;
                }
                else
                {
                    cachedImage.Dispose();
                    File.Delete(thumbnailPath);
                }
            }

            int imageId1;
            int imageId2;

            if (theme.dayHighlight.HasValue)
            {
                imageId1 = theme.dayHighlight.Value;
            }
            else
            {
                imageId1 = theme.dayImageList[theme.dayImageList.Length / 2];
            }

            if (theme.nightHighlight.HasValue)
            {
                imageId2 = theme.nightHighlight.Value;
            }
            else
            {
                imageId2 = theme.nightImageList[theme.nightImageList.Length / 2];
            }

            string imageFilename1 = theme.imageFilename.Replace("*", imageId1.ToString());
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

        private string GetCreditsText()
        {
            if (selectedIndex > 0)
            {
                ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];

                if (theme.imageCredits != null)
                {
                    return string.Format(_("Image Credits: {0}"), theme.imageCredits);
                }
            }
            else
            {
                return _("Image Credits: Microsoft");
            }

            return "";
        }

        private List<int> GetImageList(ThemeConfig theme)
        {
            List<int> imageList = new List<int>();
            imageList.AddRange(theme.sunriseImageList);
            imageList.AddRange(theme.dayImageList);
            imageList.AddRange(theme.sunsetImageList);
            imageList.AddRange(theme.nightImageList);
            return imageList;
        }

        private int GetMaxImageNumber()
        {
            int max = 1;

            if (selectedIndex > 0)
            {
                ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
                max = GetImageList(theme).Count;
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
                int imageId = GetImageList(theme)[imageNumber - 1];
                string imageFilename = theme.imageFilename.Replace("*", imageId.ToString());
                pictureBox1.Image = ShrinkImage(Path.Combine("themes", theme.themeId,
                    imageFilename), width, height);
            }

            imageNumberLabel.Text = string.Format(_("Image {0} of {1}"), imageNumber,
                maxImageNumber);
            firstButton.Enabled = imageNumber > 1;
            previousButton.Enabled = imageNumber > 1;
            nextButton.Enabled = imageNumber < maxImageNumber;
            lastButton.Enabled = imageNumber < maxImageNumber;

            previewImage = imageNumber;
        }

        private void EnsureThemeNotDuplicated(string themeId)
        {
            foreach (ImageListViewItem item in imageListView1.Items)
            {
                if ((string)item.Tag == themeId)
                {
                    int itemIndex = item.Index;
                    imageListView1.Items.RemoveAt(itemIndex);
                    themeNames.RemoveAt(itemIndex - 1);
                    break;
                }
            }
        }

        private void LoadImportedThemes(List<ThemeConfig> themes)
        {
            themes.Sort((t1, t2) => t1.themeId.CompareTo(t2.themeId));
            Size thumbnailSize = GetThumbnailSize();
            ImageListViewItem newItem = null;

            for (int i = 0; i < themes.Count; i++)
            {
                EnsureThemeNotDuplicated(themes[i].themeId);

                string themeName = ThemeManager.GetThemeName(themes[i]);
                themeNames.Add(themeName);
                themeNames.Sort();
                int itemIndex = themeNames.IndexOf(themeName) + 1;

                imageListView1.Items.Insert(itemIndex, ThemeManager.GetThemeName(themes[i]),
                    GetThumbnailImage(themes[i], thumbnailSize, false));
                newItem = imageListView1.Items[itemIndex];
                newItem.Tag = themes[i].themeId;
            }

            if (newItem != null)
            {
                newItem.Selected = true;
                imageListView1.EnsureVisible(newItem.Index);
            }
        }

        private void ThemeDialog_Load(object sender, EventArgs e)
        {
            imageListView1.ContextMenuStrip = contextMenuStrip1;
            imageListView1.SetRenderer(new ThemeListViewRenderer());

            Size thumbnailSize = GetThumbnailSize();
            imageListView1.ThumbnailSize = thumbnailSize;
            imageListView1.Items.Add(_("None"), ShrinkImage(windowsWallpaper, thumbnailSize.Width,
                thumbnailSize.Height));

            string currentTheme = ThemeManager.currentTheme?.themeId;
            ImageListViewItem focusItem = null;

            if (currentTheme == null)
            {
                if (JsonConfig.firstRun || JsonConfig.settings.themeName != null)
                {
                    currentTheme = "Mojave_Desert";
                }
                else
                {
                    focusItem = imageListView1.Items[0];
                }
            }

            for (int i = 0; i < ThemeManager.themeSettings.Count; i++)
            {
                ThemeConfig theme = ThemeManager.themeSettings[i];
                string themeName = ThemeManager.GetThemeName(theme);
                themeNames.Add(themeName);
                themeNames.Sort();

                int itemIndex = themeNames.IndexOf(themeName) + 1;
                imageListView1.Items.Insert(itemIndex, themeName,
                    GetThumbnailImage(theme, thumbnailSize));
                imageListView1.Items[itemIndex].Tag = theme.themeId;

                if (theme.themeId == currentTheme)
                {
                    focusItem = imageListView1.Items[itemIndex];
                }
            }

            if (focusItem != null)
            {
                focusItem.Selected = true;
                imageListView1.EnsureVisible(focusItem.Index);
            }
        }

        private void imageListView1_SelectionChanged(object sender, EventArgs e)
        {
            if (imageListView1.SelectedItems.Count > 0)
            {
                selectedIndex = imageListView1.SelectedItems[0].Index;
                int imageNumber = 1;

                if (selectedIndex > 0)
                {
                    string themeId = (string)imageListView1.Items[selectedIndex].Tag;
                    selectedIndex = ThemeManager.themeSettings.FindIndex(
                        t => t.themeId == themeId) + 1;

                    SolarData solarData = SunriseSunsetService.GetSolarData(DateTime.Today);
                    ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
                    imageNumber = GetImageList(theme).IndexOf(
                        AppContext.wpEngine.GetImageData(solarData, theme).Item1) + 1;
                }

                creditsLabel.Text = GetCreditsText();
                maxImageNumber = GetMaxImageNumber();
                LoadPreviewImage(imageNumber);

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

            if (selectedIndex == 0)
            {
                WallpaperApi.SetWallpaper(windowsWallpaper);
            }
            else
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
            imageListView1.HitTest(imageListView1.PointToClient(Cursor.Position),
                out var hitTestInfo);
            int itemIndex = hitTestInfo.ItemIndex;

            if (itemIndex <= 0 || ThemeManager.defaultThemes.Contains(
                (string)imageListView1.Items[itemIndex].Tag))
            {
                e.Cancel = true;
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int itemIndex = imageListView1.SelectedItems[0].Index;
            string themeId = (string)imageListView1.Items[itemIndex].Tag;
            ThemeConfig theme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);

            DialogResult result = MessageBox.Show(string.Format(_("Are you sure you want to " +
                "remove the '{0}' theme?"), ThemeManager.GetThemeName(theme)), _("Question"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                imageListView1.Items.RemoveAt(itemIndex);
                imageListView1.Items[itemIndex - 1].Selected = true;
                themeNames.RemoveAt(itemIndex - 1);

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
                DialogResult result = MessageBox.Show(_("WinDynamicDesktop cannot dynamically " +
                    "update your wallpaper until you have selected a theme. Are you sure you " +
                    "want to continue without a theme selected?"), _("Question"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.Show();
                }
            }
        }
    }
}
