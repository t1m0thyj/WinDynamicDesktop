// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public partial class ThemeDialog : Form
    {
        private int selectedIndex;

        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private const string themeLink = "https://windd.info/themes/";
        private readonly string windowsWallpaper = ThemeThumbLoader.GetWindowsWallpaper();

        private WPF.ThemePreviewer previewer;

        public ThemeDialog()
        {
            InitializeComponent();
            int oldButtonWidth = this.importButton.Width;
            Localization.TranslateForm(this);
            this.themeLinkLabel.Left += (this.importButton.Width - oldButtonWidth);

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;
            this.FormClosed += OnFormClosed;

            Rectangle bounds = Screen.FromControl(this).Bounds;
            Size thumbnailSize = ThemeThumbLoader.GetThumbnailSize(this);
            int newWidth = thumbnailSize.Width + SystemInformation.VerticalScrollBarWidth;
            int oldWidth = this.listView1.Size.Width;

            using (Graphics g = this.CreateGraphics())
            {
                newWidth += (int)Math.Ceiling(46 * g.DpiX / 96);
            }

            this.previewerHost.Anchor &= ~AnchorStyles.Left;
            this.listView1.Width = newWidth;
            this.downloadButton.Left += (newWidth - oldWidth) / 2;
            this.applyButton.Left += (newWidth - oldWidth) / 2;
            this.closeButton.Left += (newWidth - oldWidth) / 2;
            this.Width += (newWidth - oldWidth);
            this.previewerHost.Anchor |= AnchorStyles.Left;

            int heightDiff = this.Height - this.previewerHost.Height;
            int widthDiff = this.Width - this.previewerHost.Width;
            int bestHeight = bounds.Height * 5 / 8;
            int bestWidth = (bestHeight - heightDiff) * bounds.Width / bounds.Height + widthDiff;
            this.Size = new Size(bestWidth, bestHeight);
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
                    duplicateThemeNames.Add(ThemeManager.GetThemeName(ThemeManager.themeSettings[themeIndex]));
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

        private void LoadThemes(List<ThemeConfig> themes, string activeTheme = null)
        {
            Size thumbnailSize = ThemeThumbLoader.GetThumbnailSize(this);
            ListViewItem focusedItem = null;

            foreach (ThemeConfig theme in themes.ToList())
            {
                try
                {
                    using (Image thumbnailImage = ThemeThumbLoader.GetThumbnailImage(theme, thumbnailSize, true))
                    {
                        this.Invoke(new Action(() =>
                        {
                            listView1.LargeImageList.Images.Add(thumbnailImage);
                            string itemText = ThemeManager.GetThemeName(theme);
                            if (JsonConfig.settings.favoriteThemes != null &&
                                JsonConfig.settings.favoriteThemes.Contains(theme.themeId))
                            {
                                itemText = "★ " + itemText;
                            }
                            ListViewItem newItem = listView1.Items.Add(itemText, listView1.LargeImageList.Images.Count - 1);
                            newItem.Tag = theme.themeId;

                            if (activeTheme == null || activeTheme == theme.themeId)
                            {
                                focusedItem = newItem;
                            }
                        }));
                    }
                }
                catch (OutOfMemoryException)
                {
                    ThemeLoader.HandleError(new FailedToCreateThumbnail(theme.themeId));
                }
            }

            this.Invoke(new Action(() =>
            {
                listView1.Sort();

                if (focusedItem == null)
                {
                    focusedItem = listView1.Items[0];
                }

                focusedItem.Selected = true;
                listView1.EnsureVisible(focusedItem.Index);

                ThemeThumbLoader.CacheThumbnails(listView1);
            }));
        }

        private void LoadImportedThemes(List<ThemeConfig> themes, ImportDialog importDialog)
        {
            themes.Sort((t1, t2) => t1.themeId.CompareTo(t2.themeId));

            foreach (ThemeConfig theme in themes)
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    if ((string)item.Tag == theme.themeId)
                    {
                        listView1.Items.RemoveAt(item.Index);
                        listView1.LargeImageList.Images[item.ImageIndex].Dispose();
                        break;
                    }

                }
            }

            Task.Run(() =>
            {
                LoadThemes(themes);

                this.Invoke(new Action(() =>
                {
                    importDialog.thumbnailsLoaded = true;
                    importDialog.Close();
                }));
            });
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
                AppContext.wpEngine.RunScheduler(true);
                AppContext.ShowPopup(string.Format(_("New theme applied: {0}"),
                    ThemeManager.GetThemeName(ThemeManager.currentTheme)));
            }
        }

        private void DownloadTheme(ThemeConfig theme, bool applyPending)
        {
            DownloadDialog downloadDialog = new DownloadDialog() { Owner = this, applyPending = applyPending };
            downloadDialog.FormClosed += OnDownloadDialogClosed;
            downloadDialog.Show();
            this.Enabled = false;
            downloadDialog.InitDownload(theme);
        }

        private void UpdateSelectedItem()
        {
            if (listView1.SelectedItems.Count == 0)
            {
                applyButton.Enabled = false;
                return;
            }

            selectedIndex = listView1.SelectedIndices[0];
            bool themeDownloaded = true;
            ThemeConfig theme = null;

            if (selectedIndex > 0)
            {
                string themeId = (string)listView1.Items[selectedIndex].Tag;
                selectedIndex = ThemeManager.themeSettings.FindIndex(t => t.themeId == themeId) + 1;
                theme = ThemeManager.themeSettings[selectedIndex - 1];
                themeDownloaded = ThemeManager.IsThemeDownloaded(theme);
            }

            downloadButton.Enabled = !themeDownloaded;
            applyButton.Enabled = true;

            previewer.ViewModel.PreviewTheme(theme);
        }

        // Code to change ListView appearance from https://stackoverflow.com/a/4463114/5504760
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        private void ThemeDialog_Load(object sender, EventArgs e)
        {
            previewer = new WPF.ThemePreviewer();
            previewerHost.Child = previewer;

            listView1.ContextMenuStrip = contextMenuStrip1;
            listView1.ListViewItemSorter = new CompareByItemText();
            SetWindowTheme(listView1.Handle, "Explorer", null);

            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            Size thumbnailSize = ThemeThumbLoader.GetThumbnailSize(this);
            imageList.ImageSize = thumbnailSize;
            listView1.LargeImageList = imageList;

            imageList.Images.Add(ThemeThumbLoader.ScaleImage(windowsWallpaper, thumbnailSize));
            listView1.Items.Add(_("None"), 0);

            string activeTheme = ThemeManager.currentTheme?.themeId;

            if (activeTheme == null && (JsonConfig.firstRun || JsonConfig.settings.themeName != null))
            {
                activeTheme = "Mojave_Desert";
            }

            Task.Run(new Action(() =>
                LoadThemes(ThemeManager.themeSettings, (activeTheme != null) ? activeTheme : "")));
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedItem();
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            DownloadTheme(ThemeManager.themeSettings[selectedIndex - 1], false);
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
                DownloadTheme(ThemeManager.themeSettings[selectedIndex - 1], true);
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
            var hitTestInfo = listView1.HitTest(listView1.PointToClient(Cursor.Position));
            int itemIndex = hitTestInfo.Item?.Index ?? -1;

            if (itemIndex <= 0)
            {
                e.Cancel = true;
                return;
            }

            string themeId = (string)listView1.Items[itemIndex].Tag;
            ThemeConfig theme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);
            contextMenuStrip1.Items[1].Enabled = ThemeManager.IsThemeDownloaded(theme);

            if (JsonConfig.settings.favoriteThemes == null ||
                !JsonConfig.settings.favoriteThemes.Contains(themeId))
            {
                contextMenuStrip1.Items[0].Text = _("Add to favorites");
            }
            else
            {
                contextMenuStrip1.Items[0].Text = _("Remove from favorites");
            }

            if (ThemeManager.defaultThemes.Contains(themeId))
            {
                contextMenuStrip1.Items[1].Text = _("Delete");
            }
            else
            {
                contextMenuStrip1.Items[1].Text = _("Delete permanently");
            }
        }

        private void favoriteThemeMenuItem_Click(object sender, EventArgs e)
        {
            int itemIndex = listView1.FocusedItem.Index;
            string themeId = (string)listView1.Items[itemIndex].Tag;
            List<string> favoriteThemes = JsonConfig.settings.favoriteThemes?.ToList() ?? new List<string>();

            if (favoriteThemes.Contains(themeId))
            {
                listView1.Items[itemIndex].Text = listView1.Items[itemIndex].Text.Substring(2);
                favoriteThemes.Remove(themeId);
            }
            else
            {
                listView1.Items[itemIndex].Text = "★ " + listView1.Items[itemIndex].Text;
                favoriteThemes.Add(themeId);
            }

            listView1.Sort();
            JsonConfig.settings.favoriteThemes = favoriteThemes.ToArray();
        }

        private void deleteThemeMenuItem_Click(object sender, EventArgs e)
        {
            int itemIndex = listView1.FocusedItem.Index;
            string themeId = (string)listView1.Items[itemIndex].Tag;
            ThemeConfig theme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);

            DialogResult result = MessageDialog.ShowQuestion(string.Format(_("Are you sure you want to remove " +
                "the '{0}' theme?"), ThemeManager.GetThemeName(theme)), _("Question"), true);

            if (result == DialogResult.Yes)
            {
                if (!ThemeManager.defaultThemes.Contains(theme.themeId))
                {
                    int imageIndex = listView1.Items[itemIndex].ImageIndex;
                    listView1.Items.RemoveAt(itemIndex);
                    listView1.Items[itemIndex - 1].Selected = true;
                    listView1.LargeImageList.Images[imageIndex].Dispose();
                }

                Task.Run(() =>
                {
                    ThemeManager.RemoveTheme(theme);

                    if (ThemeManager.defaultThemes.Contains(theme.themeId))
                    {
                        this.Invoke(new Action(() => UpdateSelectedItem()));
                    }
                });
            }
        }

        private void OnDownloadDialogClosed(object sender, FormClosedEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing &&
                ThemeManager.IsThemeDownloaded(ThemeManager.themeSettings[selectedIndex - 1]))
            {
                if (((DownloadDialog)sender).applyPending)
                {
                    ApplySelectedTheme();
                }

                UpdateSelectedItem();
            }

            this.Enabled = !ThemeManager.importMode;
        }

        private void OnImportDialogClosing(object sender, FormClosingEventArgs e)
        {
            ImportDialog importDialog = (ImportDialog)sender;

            if (e.CloseReason == CloseReason.UserClosing && !importDialog.thumbnailsLoaded)
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
            if (e.CloseReason == CloseReason.UserClosing && JsonConfig.firstRun && ThemeManager.currentTheme == null)
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
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            previewer.ViewModel.Stop();
        }
    }

    // Comparer class to make ListView sort by theme name
    // Code from https://stackoverflow.com/a/30536933/5504760
    public class CompareByItemText : IComparer
    {
        public int Compare(object x, object y)
        {
            ListViewItem item1 = (ListViewItem)x;
            ListViewItem item2 = (ListViewItem)y;
            string a = (item1.Tag != null) ? item1.Text : " ";
            string b = (item2.Tag != null) ? item2.Text : " ";
            return a.CompareTo(b);
        }
    }
}
