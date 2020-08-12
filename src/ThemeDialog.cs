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
        private int previewImage;
        private int selectedIndex;
        private List<string> themeNames = new List<string>();

        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private const string themeLink = "https://windd.info/themes/";
        private readonly string windowsWallpaper = ThemeThumbLoader.GetWindowsWallpaper();

        public ThemeDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;

            Size thumbnailSize = ThemeThumbLoader.GetThumbnailSize(this);
            int newWidth = (thumbnailSize.Width + 35) * 3 + SystemInformation.VerticalScrollBarWidth;
            int newHeight = (thumbnailSize.Height + this.Font.Height + 40) * 2;
            int oldWidth = this.imageListView1.Size.Width;
            int oldHeight = this.imageListView1.Size.Height;
            this.Size = new Size(this.Width + newWidth - this.imageListView1.Size.Width,
                this.Height + newHeight - this.imageListView1.Size.Height);
            this.CenterToScreen();
        }

        public void ImportThemes(List<string> themePaths)
        {
            this.Enabled = false;
            List<string> duplicateThemeNames = new List<string>();

            foreach (string themePath in themePaths)
            {
                string themeId = Path.GetFileNameWithoutExtension(themePath);
                int themeIndex = ThemeManager.themeSettings.FindIndex(t => t.themeId == themeId);

                if (themeIndex != -1)
                {
                    duplicateThemeNames.Add(ThemeManager.GetThemeName(
                        ThemeManager.themeSettings[themeIndex]));
                }
            }

            if (duplicateThemeNames.Count > 0)
            {
                DialogResult result = MessageDialog.ShowQuestion(string.Format(_("The following themes are already " +
                    "installed:\n\t{0}\n\nDo you want to overwrite them?"), string.Join("\n\t", duplicateThemeNames)),
                    _("Question"), true);

                if (result != DialogResult.Yes)
                {
                    this.Enabled = true;
                    return;
                }
            }

            ImportDialog importDialog = new ImportDialog() { Owner = this };
            importDialog.FormClosing += OnImportDialogClosing;
            importDialog.Show();
            importDialog.InitImport(themePaths);
        }

        private string GetCreditsText()
        {
            string themeAuthor;

            if (selectedIndex > 0)
            {
                themeAuthor = ThemeManager.GetThemeAuthor(ThemeManager.themeSettings[selectedIndex - 1]);
            }
            else
            {
                themeAuthor = "Microsoft";
            }

            return (themeAuthor != null) ? string.Format(_("Image Credits: {0}"), themeAuthor) : "";
        }

        private void LoadPreviewImage(Image image)
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

            pictureBox1.Image?.Dispose();
            pictureBox1.Image = ThemeThumbLoader.ScaleImage(image, new Size(width, height));
        }

        private void LoadPreviewImage(int imageNumber)
        {
            ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
            int imageId = ThemeManager.GetThemeImageList(theme)[imageNumber - 1];
            string imageFilename = theme.imageFilename.Replace("*", imageId.ToString());
            LoadPreviewImage(new Bitmap(Path.Combine("themes", theme.themeId, imageFilename)));
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

                    using (Image thumbnailImage = ThemeThumbLoader.GetThumbnailImage(themes[i], thumbnailSize, false))
                    {
                        this.Invoke(new Action(() =>
                        {
                            imageListView1.Items.Insert(itemIndex, ThemeManager.GetThemeName(themes[i]),
                                thumbnailImage);
                            newItem = imageListView1.Items[itemIndex];
                            newItem.Tag = themes[i].themeId;
                        }));
                    }
                }

                if (newItem != null)
                {
                    this.Invoke(new Action(() =>
                    {
                        imageListView1.ClearSelection();
                        newItem.Selected = true;
                        imageListView1.EnsureVisible(newItem.Index);
                    }));
                }

                ThemeThumbLoader.CacheThumbnails(imageListView1.Items);
                importDialog.thumbnailsLoaded = true;
                this.Invoke(new Action(() => importDialog.Close()));
            });
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
                    selectedIndex = ThemeManager.themeSettings.FindIndex(t => t.themeId == themeId) + 1;
                    ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
                    nameLabel.Text = ThemeManager.GetThemeName(theme);
                    themeDownloaded = ThemeManager.IsThemeDownloaded(theme);

                    if (themeDownloaded)
                    {
                        SolarData solarData = SunriseSunsetService.GetSolarData(DateTime.Today);
                        imageNumber = ThemeManager.GetThemeImageList(theme).IndexOf(
                            AppContext.wpEngine.GetImageData(solarData, theme).imageId) + 1;
                        LoadPreviewImage(imageNumber);
                    }
                    else
                    {
                        LoadPreviewImage((Image)Properties.Resources.ResourceManager.GetObject(themeId + "_thumbnail"));
                    }
                } else
                {
                    nameLabel.Text = _("Windows Default");
                    LoadPreviewImage(new Bitmap(windowsWallpaper));
                }

                creditsLabel.Text = GetCreditsText();
                downloadLabel.Visible = selectedIndex > 0 && !themeDownloaded;
                previewLinkLabel.Visible = selectedIndex > 0 && themeDownloaded;
                applyButton.Enabled = true;
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
            imageListView1.Items.Add(_("None"), ThemeThumbLoader.ScaleImage(windowsWallpaper, thumbnailSize));

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
                    
                    using (Image thumbnailImage = ThemeThumbLoader.GetThumbnailImage(theme, thumbnailSize, true))
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

                ThemeThumbLoader.CacheThumbnails(imageListView1.Items);
            }));
        }

        private void ApplySelectedTheme()
        {
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
                WallpaperShuffler.AddThemeToHistory(ThemeManager.currentTheme.themeId);
                AppContext.wpEngine.RunScheduler();
                AppContext.ShowPopup(string.Format(_("New theme applied: {0}"),
                    ThemeManager.GetThemeName(ThemeManager.currentTheme)));
            }
        }

        private void imageListView1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateSelectedItem();
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                ImportThemes(openFileDialog1.FileNames.ToList());

                openFileDialog1.InitialDirectory = Path.GetDirectoryName(openFileDialog1.FileNames[0]);
                openFileDialog1.FileName = "";
            }
        }

        private void themeLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(themeLink);
        }

        private void previewLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ThemeConfig theme = ThemeManager.themeSettings[selectedIndex - 1];
            ThemePreviewer.LaunchPreview(theme, previewImage);
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            applyButton.Enabled = false;
            bool themeDownloaded = true;

            if (selectedIndex > 0)
            {
                themeDownloaded = ThemeManager.IsThemeDownloaded(ThemeManager.themeSettings[selectedIndex - 1]);
            }

            if (!themeDownloaded)
            {
                DownloadDialog downloadDialog = new DownloadDialog() { Owner = this };
                downloadDialog.FormClosed += OnDownloadDialogClosed;
                downloadDialog.Show();
                this.Enabled = false;
                downloadDialog.InitDownload(ThemeManager.themeSettings[selectedIndex - 1]);
            }
            else
            {
                ApplySelectedTheme();
            }

            applyButton.Enabled = true;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            imageListView1.HitTest(imageListView1.PointToClient(Cursor.Position), out var hitTestInfo);
            int itemIndex = hitTestInfo.ItemIndex;
            string themeId = (string)imageListView1.Items[itemIndex].Tag;

            if (itemIndex == 0)
            {
                e.Cancel = true;
            }
            else if (ThemeManager.defaultThemes.Contains(themeId))
            {
                ThemeConfig theme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);

                if (ThemeManager.IsThemeDownloaded(theme))
                {
                    contextMenuStrip1.Items[0].Text = _("Delete");
                }
                else
                {
                    contextMenuStrip1.Items[0].Text = _("Download");
                }
            }
            else
            {
                contextMenuStrip1.Items[0].Text = _("Delete permanently");
            }
        }

        private void themeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int itemIndex = imageListView1.SelectedItems[0].Index;
            string themeId = (string)imageListView1.Items[itemIndex].Tag;
            ThemeConfig theme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);

            if (ThemeManager.IsThemeDownloaded(theme))
            {
                DialogResult result = MessageDialog.ShowQuestion(string.Format(_("Are you sure you want to remove the " +
                "'{0}' theme?"), ThemeManager.GetThemeName(theme)), _("Question"), true);

                if (result == DialogResult.Yes)
                {
                    if (!ThemeManager.defaultThemes.Contains(theme.themeId))
                    {
                        imageListView1.Items.RemoveAt(itemIndex);
                        imageListView1.Items[itemIndex - 1].Selected = true;
                        themeNames.RemoveAt(itemIndex - 1);
                    }

                    Task.Run(() => {
                        ThemeManager.RemoveTheme(theme);

                        if (ThemeManager.defaultThemes.Contains(theme.themeId))
                        {
                            this.Invoke(new Action(() => UpdateSelectedItem()));
                        }
                    });
                }
            }
            else
            {
                DownloadDialog downloadDialog = new DownloadDialog() { Owner = this };
                downloadDialog.FormClosed += OnDownloadDialogClosed2;
                downloadDialog.Show();
                this.Enabled = false;
                downloadDialog.InitDownload(theme);
            }
        }

        private void OnDownloadDialogClosed(object sender, FormClosedEventArgs e)
        {
            if (ThemeManager.IsThemeDownloaded(ThemeManager.themeSettings[selectedIndex - 1]))
            {
                ApplySelectedTheme();
                UpdateSelectedItem();
            }

            this.Enabled = !ThemeManager.importMode;
        }

        private void OnDownloadDialogClosed2(object sender, FormClosedEventArgs e)
        {

            // TODO Investigate cleaner way to define this function considering there is similar one above
            if (ThemeManager.IsThemeDownloaded(ThemeManager.themeSettings[selectedIndex - 1]))
            {
                UpdateSelectedItem();
            }

            this.Enabled = true;
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
                DialogResult result = MessageDialog.ShowQuestion(_("WinDynamicDesktop cannot dynamically update your " +
                    "wallpaper until you have selected a theme. Are you sure you want to continue without a theme " +
                    "selected?"), _("Question"), true);

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            pictureBox1.Image?.Dispose();
        }
    }
}
