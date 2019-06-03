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

            int bestWidth = (ThemeThumbLoader.GetThumbnailSize(this).Width + 30) * 2 +
                SystemInformation.VerticalScrollBarWidth;
            int oldWidth = this.imageListView1.Size.Width;
            this.imageListView1.Size = new Size(bestWidth, this.imageListView1.Height);
            this.Size = new Size(this.Width + bestWidth - oldWidth, this.Height);
            this.CenterToScreen();
        }

        public void ImportThemes(List<string> themePaths)
        {
            ImportDialog importDialog = new ImportDialog() { Owner = this };
            importDialog.FormClosing += OnImportDialogClosing;
            importDialog.Show();
            importDialog.InitImport(themePaths);
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

        private int GetMaxImageNumber()
        {
            int max = 1;

            if (selectedIndex > 0)
            {
                ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
                max = ThemeManager.GetThemeImageList(theme).Count;
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
                pictureBox1.Image = ThemeThumbLoader.ScaleImage(windowsWallpaper,
                    new Size(width, height));
            }
            else
            {
                ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
                int imageId = ThemeManager.GetThemeImageList(theme)[imageNumber - 1];
                string imageFilename = theme.imageFilename.Replace("*", imageId.ToString());
                pictureBox1.Image = ThemeThumbLoader.ScaleImage(Path.Combine("themes",
                    theme.themeId, imageFilename), new Size(width, height));
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

        private void LoadImportedThemes(List<ThemeConfig> themes, ImportDialog importDialog)
        {
            themes.Sort((t1, t2) => t1.themeId.CompareTo(t2.themeId));
            Size thumbnailSize = ThemeThumbLoader.GetThumbnailSize(this);
            ImageListViewItem newItem = null;

            Task.Run(() =>
            {
            for (int i = 0; i < themes.Count; i++)
            {
                this.Invoke(new Action(() => EnsureThemeNotDuplicated(themes[i].themeId)));

                string themeName = ThemeManager.GetThemeName(themes[i]);
                themeNames.Add(themeName);
                themeNames.Sort();
                int itemIndex = themeNames.IndexOf(themeName) + 1;

                using (Image thumbnailImage = ThemeThumbLoader.GetThumbnailImage(themes[i],
                    thumbnailSize, false))
                {
                    this.Invoke(new Action(() =>
                    {
                        imageListView1.Items.Insert(itemIndex,
                            ThemeManager.GetThemeName(themes[i]), thumbnailImage);
                        newItem = imageListView1.Items[itemIndex];
                        newItem.Tag = themes[i].themeId;
                    }));
                }
            }

            if (newItem != null)
            {
                this.Invoke(new Action(() =>
                {
                    newItem.Selected = true;
                    imageListView1.EnsureVisible(newItem.Index);
                }));
            }

            importDialog.thumbnailsLoaded = true;
            this.Invoke(new Action(() => importDialog.Close()));
            });
        }

        private void SetThemeDownloaded(bool themeDownloaded)
        {
            pictureBox1.Visible = themeDownloaded;
            firstButton.Visible = themeDownloaded;
            previousButton.Visible = themeDownloaded;
            imageNumberLabel.Visible = themeDownloaded;
            nextButton.Visible = themeDownloaded;
            lastButton.Visible = themeDownloaded;
            downloadButton.Visible = !themeDownloaded;
        }

        private void UpdateSelectedItem()
        {
            if (imageListView1.SelectedItems.Count > 0)
            {
                selectedIndex = imageListView1.SelectedItems[0].Index;
                int imageNumber = 1;
                bool themeDownloaded = true;

                if (selectedIndex > 0)
                {
                    string themeId = (string)imageListView1.Items[selectedIndex].Tag;
                    selectedIndex = ThemeManager.themeSettings.FindIndex(
                        t => t.themeId == themeId) + 1;
                    ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
                    themeDownloaded = ThemeManager.IsThemeDownloaded(theme);

                    if (themeDownloaded)
                    {
                        SolarData solarData = SunriseSunsetService.GetSolarData(DateTime.Today);
                        imageNumber = ThemeManager.GetThemeImageList(theme).IndexOf(
                            AppContext.wpEngine.GetImageData(solarData, theme).Item1) + 1;
                    }
                }

                SetThemeDownloaded(themeDownloaded);
                creditsLabel.Text = GetCreditsText();

                if (themeDownloaded)
                {
                    maxImageNumber = GetMaxImageNumber();
                    LoadPreviewImage(imageNumber);
                }

                applyButton.Enabled = themeDownloaded;
            }
            else
            {
                applyButton.Enabled = false;
            }
        }

        private void ThemeDialog_Load(object sender, EventArgs e)
        {
            imageListView1.ContextMenuStrip = contextMenuStrip1;
            imageListView1.SetRenderer(new ThemeListViewRenderer());

            Size thumbnailSize = ThemeThumbLoader.GetThumbnailSize(this);
            imageListView1.ThumbnailSize = thumbnailSize;
            imageListView1.Items.Add(_("None"), ThemeThumbLoader.ScaleImage(windowsWallpaper,
                thumbnailSize));

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

            Task.Run(new Action(() => {
                for (int i = 0; i < ThemeManager.themeSettings.Count; i++)
                {
                    ThemeConfig theme = ThemeManager.themeSettings[i];
                    string themeName = ThemeManager.GetThemeName(theme);
                    themeNames.Add(themeName);
                    themeNames.Sort();
                    int itemIndex = themeNames.IndexOf(themeName) + 1;
                    
                    using (Image thumbnailImage = ThemeThumbLoader.GetThumbnailImage(theme,
                        thumbnailSize, true))
                    {
                        this.Invoke(new Action(() => {
                            imageListView1.Items.Insert(itemIndex, themeName, thumbnailImage);
                            imageListView1.Items[itemIndex].Tag = theme.themeId;
                        }));
                    }

                    if (theme.themeId == currentTheme)
                    {
                        focusItem = imageListView1.Items[itemIndex];
                    }
                }

                if (focusItem != null)
                {
                    this.Invoke(new Action(() => {
                        focusItem.Selected = true;
                        imageListView1.EnsureVisible(focusItem.Index);
                    }));
                }
            }));
        }

        private void imageListView1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateSelectedItem();
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

        private void downloadButton_Click(object sender, EventArgs e)
        {
            DownloadDialog downloadDialog = new DownloadDialog() { Owner = this };
            downloadDialog.FormClosed += OnDownloadDialogClosed;
            downloadDialog.Show();
            this.Enabled = false;
            downloadDialog.InitDownload(ThemeManager.themeSettings[selectedIndex - 1]);
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

        private void OnDownloadDialogClosed(object sender, FormClosedEventArgs e)
        {
            if (ThemeManager.IsThemeDownloaded(ThemeManager.themeSettings[selectedIndex - 1]))
            {
                UpdateSelectedItem();
            }

            this.Enabled = !ThemeManager.importMode;
        }

        private void OnImportDialogClosing(object sender, FormClosingEventArgs e)
        {
            ImportDialog importDialog = (ImportDialog)sender;

            if (!importDialog.thumbnailsLoaded)
            {
                e.Cancel = true;
                LoadImportedThemes(ThemeManager.importedThemes, importDialog);
            }
            else
            {
                ThemeManager.importedThemes.Clear();
                this.Enabled = !ThemeManager.downloadMode;
            }
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
