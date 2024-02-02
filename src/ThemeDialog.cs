﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsDisplayAPI.DisplayConfig;

namespace WinDynamicDesktop
{
    public partial class ThemeDialog : Form
    {
        private int selectedIndex;

        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private const string themeLink = "https://windd.info/themes/";
        private readonly string windowsWallpaper = ThemeThumbLoader.GetWindowsWallpaper(false);
        private readonly string windowsLockScreen = ThemeThumbLoader.GetWindowsWallpaper(true);

        private WPF.ThemePreviewer previewer;

        public ThemeDialog()
        {
            InitializeComponent();
            DarkUI.ThemeForm(this);
            int oldButtonWidth = this.importButton.Width;
            Localization.TranslateForm(this);
            this.themeLinkLabel.Left += (this.importButton.Width - oldButtonWidth);

            this.FormClosing += OnFormClosing;
            this.FormClosed += OnFormClosed;

            this.toolStrip1.Renderer = new CustomToolStripRenderer();
            this.meatballButton.Image = GetMeatballIcon();

            Rectangle bounds = Screen.FromControl(this).Bounds;
            Size thumbnailSize = ThemeThumbLoader.GetThumbnailSize(this);
            int newWidth = thumbnailSize.Width + SystemInformation.VerticalScrollBarWidth;
            int oldWidth = this.listView1.Size.Width;

            using (Graphics g = this.CreateGraphics())
            {
                newWidth += (int)Math.Ceiling(46 * g.DpiX / 96);
            }

            this.previewerHost.Anchor &= ~AnchorStyles.Left;
            this.toolStrip1.Width = newWidth;
            this.displayComboBox.Width = newWidth - this.meatballButton.Width - 8;
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

        public Bitmap GetMeatballIcon()
        {
            Bitmap bitmap = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                for (int i = 0; i < 3; i++)
                {
                    graphics.FillEllipse(new SolidBrush(this.ForeColor), i * 6, 6, 4, 4);
                }
            }
            return bitmap;
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
                    _("Question"), MessageBoxIcon.Warning);

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

        private string[] GetDisplayNames()
        {
            // https://github.com/winleafs/Winleafs/blob/98ba3ba/Winleafs.Wpf/Helpers/ScreenBoundsHelper.cs#L36=
            var activeDisplays = WindowsDisplayAPI.Display.GetDisplays();
            var activeDisplayDevicePaths = activeDisplays.OrderBy(d => d.DisplayName)
                .Select(d => d.DevicePath).ToArray();
            return PathDisplayTarget.GetDisplayTargets()
                .Where(dt => activeDisplayDevicePaths.Contains(dt.DevicePath))
                .OrderBy(dt => Array.IndexOf(activeDisplayDevicePaths, dt.DevicePath)).Select(dt =>
                {
                    return string.IsNullOrEmpty(dt.FriendlyName) ? _("Internal Display") : dt.FriendlyName;
                }).ToArray();
        }

        private void LoadThemes(List<ThemeConfig> themes, string activeTheme = null, string focusTheme = null)
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

                            if (activeTheme != null && activeTheme == theme.themeId)
                            {
                                newItem.Font = new Font(newItem.Font, FontStyle.Bold);
                            }
                            if (focusTheme == null || focusTheme == theme.themeId)
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
            string activeTheme = (selectedIndex > 0) ? ThemeManager.themeSettings[selectedIndex - 1].themeId : null;
            List<string> activeThemes = JsonConfig.settings.activeThemes?.ToList() ?? new List<string> { null };

            if (displayComboBox.SelectedIndex == 0)
            {
                activeThemes[0] = activeTheme;
            }
            else if (IsLockScreenSelected)
            {
                if (JsonConfig.settings.lockScreenDisplayIndex != -1)
                {
                    DialogResult result = MessageDialog.ShowQuestion(string.Format(
                        _("Applying this theme will stop the lock screen image from syncing with {0}. " +
                        "Are you sure you want to continue?"), LockScreenChanger.GetDisplayName()));
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }

                JsonConfig.settings.lockScreenDisplayIndex = -1;
                JsonConfig.settings.lockScreenTheme = activeTheme;
            }
            else
            {
                int numNewDisplays = Screen.AllScreens.Length - activeThemes.Count + 1;

                if (numNewDisplays > 0)
                {
                    for (int i = 0; i < numNewDisplays; i++)
                    {
                        activeThemes.Add(null);
                    }
                }

                if (activeThemes[0] != null)
                {
                    for (int i = 0; i < activeThemes.Count; i++)
                    {
                        if (activeThemes[i] == null)
                        {
                            activeThemes[i] = activeThemes[0];
                        }
                    }

                    activeThemes[0] = null;
                }

                activeThemes[displayComboBox.SelectedIndex] = activeTheme;
            }

            JsonConfig.settings.activeThemes = activeThemes.ToArray();

            foreach (ListViewItem item in listView1.Items)
            {
                item.Font = new Font(item.Font, item.Selected ? FontStyle.Bold : FontStyle.Regular);
            }

            if (selectedIndex == 0)
            {
                AppContext.scheduler.Run(true, new DisplayEvent()
                {
                    displayIndex = IsLockScreenSelected ? DisplayEvent.LockScreenIndex :
                        (displayComboBox.SelectedIndex - 1),
                    lastImagePath = windowsWallpaper
                });
            }
            else
            {
                ThemeShuffler.AddThemeToHistory(activeTheme);
                AppContext.scheduler.Run(true);
                AppContext.ShowPopup(string.Format(_("New theme applied: {0}"),
                    ThemeManager.GetThemeName(ThemeManager.themeSettings[selectedIndex - 1])));
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

            previewer.ViewModel.PreviewTheme(theme, ThemeThumbLoader.GetWindowsWallpaper(IsLockScreenSelected));
        }

        private bool IsLockScreenSelected
        {
            get
            {
                return UwpDesktop.IsUwpSupported() &&
                    displayComboBox.SelectedIndex == displayComboBox.Items.Count - 1;
            }
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
            SetWindowTheme(listView1.Handle, DarkUI.IsDark ? "DarkMode_Explorer" : "Explorer", null);

            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            Size thumbnailSize = ThemeThumbLoader.GetThumbnailSize(this);
            imageList.ImageSize = thumbnailSize;
            listView1.LargeImageList = imageList;

            imageList.Images.Add(ThemeThumbLoader.ScaleImage(windowsWallpaper, thumbnailSize));
            listView1.Items.Add(_("None"), 0);

            meatballButton.DropDownItems.AddRange(ThemeShuffler.GetMenuItems());
            meatballButton.DropDownItems.Add(new ToolStripSeparator());
            meatballButton.DropDownItems.AddRange(LockScreenChanger.GetMenuItems());

            if (UwpDesktop.IsMultiDisplaySupported())
            {
                string[] displayNames = GetDisplayNames();
                for (int i = 0; i < displayNames.Length; i++)
                {
                    displayComboBox.Items.Add(string.Format(_("Display {0}: {1}"), i + 1, displayNames[i]));
                }
            }
            if (UwpDesktop.IsUwpSupported())
            {
                imageList.Images.Add(ThemeThumbLoader.ScaleImage(windowsLockScreen, thumbnailSize));
                displayComboBox.Items.Add(_("Lock Screen"));
            }
            displayComboBox.Enabled = displayComboBox.Items.Count > 1;
            int activeThemeIndex = JsonConfig.settings.activeThemes?.ToList().FindIndex(
                themeId => themeId != null) ?? -1;
            displayComboBox.SelectedIndex = activeThemeIndex != -1 ? activeThemeIndex : 0;

            string activeTheme = JsonConfig.settings.activeThemes?[displayComboBox.SelectedIndex];
            string focusTheme = activeTheme ?? "";
            if (activeTheme == null && JsonConfig.firstRun)
            {
                focusTheme = "Mojave_Desert";
            }

            Task.Run(new Action(() => LoadThemes(ThemeManager.themeSettings, activeTheme, focusTheme)));
        }

        private void displayComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string activeTheme = IsLockScreenSelected ? JsonConfig.settings.lockScreenTheme : null;
            if (!IsLockScreenSelected && JsonConfig.settings.activeThemes != null &&
                JsonConfig.settings.activeThemes.Length > displayComboBox.SelectedIndex)
            {
                activeTheme = JsonConfig.settings.activeThemes[displayComboBox.SelectedIndex];
            }
            LockScreenChanger.UpdateMenuItems(IsLockScreenSelected ? null : displayComboBox.SelectedIndex);

            int oldImageIndex = listView1.Items[0].ImageIndex;
            listView1.Items[0].ImageIndex = IsLockScreenSelected ? 1 : 0;
            if (oldImageIndex != listView1.Items[0].ImageIndex)
            {
                UpdateSelectedItem();
            }

            foreach (ListViewItem item in listView1.Items)
            {
                if ((string)item.Tag == activeTheme)
                {
                    listView1.Items[item.Index].Selected = true;
                    listView1.EnsureVisible(item.Index);
                }

                listView1.Items[item.Index].Font = new Font(listView1.Items[item.Index].Font,
                    (string)item.Tag == activeTheme ? FontStyle.Bold : FontStyle.Regular);
            }
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
            openFileDialog1.Filter = string.Format(_("Theme files|{0}|All files|{1}"), "*.ddw;*.zip;*.json", "*.*");
            openFileDialog1.Title = _("Import Theme");
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
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(themeLink) { UseShellExecute = true });
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            applyButton.Enabled = false;
            bool themeDownloaded = true;

            if (selectedIndex > 0)
            {
                themeDownloaded = ThemeManager.IsThemeDownloaded(ThemeManager.themeSettings[selectedIndex - 1]);
                if (ThemeManager.IsThemePreinstalled(ThemeManager.themeSettings[selectedIndex - 1]))
                {
                    DefaultThemes.InstallWindowsTheme(ThemeManager.themeSettings[selectedIndex - 1]);
                }
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
            bool isWindowsTheme = themeId == DefaultThemes.GetWindowsTheme()?.themeId;
            contextMenuStrip1.Items[1].Enabled = ThemeManager.IsThemeDownloaded(theme) && !isWindowsTheme;

            if (JsonConfig.settings.favoriteThemes == null ||
                !JsonConfig.settings.favoriteThemes.Contains(themeId))
            {
                contextMenuStrip1.Items[0].Text = _("Add to favorites");
            }
            else
            {
                contextMenuStrip1.Items[0].Text = _("Remove from favorites");
            }

            if (ThemeManager.defaultThemes.Contains(themeId) || isWindowsTheme)
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
                "the '{0}' theme?"), ThemeManager.GetThemeName(theme)), _("Question"), MessageBoxIcon.Warning);

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
            if (e.CloseReason == CloseReason.UserClosing && !LaunchSequence.IsThemeReady())
            {
                DialogResult result = MessageDialog.ShowQuestion(_("WinDynamicDesktop cannot dynamically update your " +
                    "wallpaper until you have selected a theme. Are you sure you want to continue without a theme " +
                    "selected?"), _("Question"), MessageBoxIcon.Warning);

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

    public class CustomToolStripRenderer : ToolStripSystemRenderer
    {
        public CustomToolStripRenderer() { }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }
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
