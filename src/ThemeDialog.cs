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
using System.Threading.Tasks;
using System.Windows.Forms;
using WinDynamicDesktop.Properties;

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
            int oldButtonWidth = this.importButton.Width;
            Localization.TranslateForm(this);
            this.themeLinkLabel.Left += (this.importButton.Width - oldButtonWidth);

#pragma warning disable SYSLIB5002
            this.searchBoxButton.BackgroundImage = SystemColors.UseAlternativeColorSet ?
                Resources.IconSearch_Dark : Resources.IconSearch_Light;
#pragma warning restore SYSLIB5002
            this.themeLinkLabel.LinkColor = SystemColors.HotTrack;

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
            this.displayComboBox.Width = newWidth;
            this.listView1.Width = newWidth;
            this.searchBox.Width = newWidth;
            this.searchBoxButton.Left += (newWidth - oldWidth);
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

        private bool IsLockScreenSelected
        {
            get
            {
                return UwpDesktop.IsUwpSupported() &&
                    displayComboBox.SelectedIndex == displayComboBox.Items.Count - 1;
            }
        }

        private void ApplySelectedTheme()
        {
            string activeTheme = (selectedIndex > 0) ? ThemeManager.themeSettings[selectedIndex - 1].themeId : null;
            List<string> activeThemes = JsonConfig.IsNullOrEmpty(JsonConfig.settings.activeThemes) ?
                new List<string> { null } : JsonConfig.settings.activeThemes.ToList();

            if (displayComboBox.SelectedIndex == 0)
            {
                activeThemes[0] = activeTheme;
                if (selectedIndex == 0)
                {
                    activeThemes.RemoveRange(1, activeThemes.Count - 1);
                }
            }
            else if (IsLockScreenSelected)
            {
                bool shouldContinue = ThemeDialogUtils.UpdateConfigForLockScreen(activeTheme);
                if (!shouldContinue)
                {
                    return;
                }
                JsonConfig.settings.lockScreenTheme = activeTheme;
            }
            else
            {
                ThemeDialogUtils.UpdateConfigForDisplay(activeThemes);
                activeThemes[displayComboBox.SelectedIndex] = activeTheme;
            }

            JsonConfig.settings.activeThemes = activeThemes.ToArray();

            foreach (ListViewItem item in listView1.Items)
            {
                item.Font = new Font(item.Font, item.Selected ? FontStyle.Bold : FontStyle.Regular);
            }

            if (selectedIndex > 0)
            {
                ThemeShuffler.AddThemeToHistory(activeTheme);
                AppContext.scheduler.Run(true);
                AppContext.ShowPopup(string.Format(_("New theme applied: {0}"),
                    ThemeManager.GetThemeName(ThemeManager.themeSettings[selectedIndex - 1])));
            }
            else if (!IsLockScreenSelected)
            {
                AppContext.scheduler.Run(true, new DisplayEvent()
                {
                    displayIndex = displayComboBox.SelectedIndex - 1,
                    lastImagePath = windowsWallpaper
                });
            }
            else
            {
                AppContext.scheduler.Run(true, new DisplayEvent()
                {
                    displayIndex = DisplayEvent.LockScreenIndex,
                    lastImagePath = windowsLockScreen
                });
            }
        }

        private void DownloadTheme(ThemeConfig theme, bool applyPending = false)
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
                if (listView1.Items.Count == 0)
                {
                    previewer.ViewModel.PreviewTheme(null, null,
                        ThemeThumbLoader.GetWindowsWallpaper(IsLockScreenSelected));
                }
                applyButton.Enabled = false;
                return;
            }

            selectedIndex = listView1.SelectedIndices[0];
            bool themeDownloaded = true;
            ThemeConfig theme = null;

            if (listView1.Items[selectedIndex].Tag != null)
            {
                string themeId = (string)listView1.Items[selectedIndex].Tag;
                selectedIndex = ThemeManager.themeSettings.FindIndex(t => t.themeId == themeId) + 1;
                theme = ThemeManager.themeSettings[selectedIndex - 1];
                themeDownloaded = ThemeManager.IsThemeDownloaded(theme);
            }

            applyButton.Enabled = true;
            previewer.ViewModel.PreviewTheme(theme, new Action<ThemeConfig>((theme) => DownloadTheme(theme)),
                ThemeThumbLoader.GetWindowsWallpaper(IsLockScreenSelected));
        }

        private void ThemeDialog_Load(object sender, EventArgs e)
        {
            previewer = new WPF.ThemePreviewer();
            previewerHost.Child = previewer;

            listView1.ContextMenuStrip = contextMenuStrip1;
            listView1.ListViewItemSorter = new CompareByItemText();
            ThemeDialogUtils.SetWindowTheme(listView1.Handle, "Explorer", null);

            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            Size thumbnailSize = ThemeThumbLoader.GetThumbnailSize(this);
            imageList.ImageSize = thumbnailSize;
            listView1.LargeImageList = imageList;

            imageList.Images.Add(ThemeThumbLoader.ScaleImage(windowsWallpaper, thumbnailSize));
            listView1.Items.Add(_("None"), 0);

            contextMenuStrip2.Items.AddRange(ThemeShuffler.GetMenuItems());
            contextMenuStrip2.Items.Add(new ToolStripSeparator());
            contextMenuStrip2.Items.AddRange(LockScreenChanger.GetMenuItems());

            if (UwpDesktop.IsWinRtSupported())
            {
                string[] displayNames = ThemeDialogUtils.GetDisplayNames();
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

            string activeTheme = JsonConfig.IsNullOrEmpty(JsonConfig.settings.activeThemes) ? null :
                JsonConfig.settings.activeThemes[displayComboBox.SelectedIndex];
            string focusTheme = activeTheme ?? "";
            if (activeTheme == null && JsonConfig.firstRun)
            {
                focusTheme = "Mojave_Desert";
            }

            ThemeLoadOpts loadOpts = new ThemeLoadOpts(activeTheme, focusTheme);
            Task.Run(new Action(() => ThemeDialogUtils.LoadThemes(ThemeManager.themeSettings, listView1, loadOpts)));
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
#pragma warning disable SYSLIB5002
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                this.searchBoxButton.BackgroundImage = SystemColors.UseAlternativeColorSet ?
                    Resources.IconSearch_Dark : Resources.IconSearch_Light;
            }
            else
            {
                this.searchBoxButton.BackgroundImage = SystemColors.UseAlternativeColorSet ?
                    Resources.IconDismiss_Dark : Resources.IconDismiss_Light;
            }
#pragma warning restore SYSLIB5002

            ThemeDialogUtils.ApplySearchFilter(listView1, searchBox.Text);
            UpdateSelectedItem();
        }

        private void searchBoxButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(searchBox.Text))
            {
                searchBox.Text = "";
            }
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

        private void advancedButton_Click(object sender, EventArgs e)
        {
            if (!contextMenuStrip2.Visible)
            {
                contextMenuStrip2.Show(advancedButton, new Point(0, advancedButton.Height));
            }
            else
            {
                contextMenuStrip2.Hide();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var hitTestInfo = listView1.HitTest(listView1.PointToClient(Cursor.Position));
            int itemIndex = hitTestInfo.Item?.Index ?? -1;
            string themeId = null;
            ThemeConfig theme = null;

            if (itemIndex < 0)
            {
                e.Cancel = true;
                return;
            }
            else if (listView1.Items[itemIndex].Tag != null)
            {
                themeId = (string)listView1.Items[itemIndex].Tag;
                theme = ThemeManager.themeSettings.Find(t => t.themeId == themeId);
            }

            bool isWindowsTheme = themeId == DefaultThemes.GetWindowsTheme()?.themeId;
            contextMenuStrip1.Items[0].Enabled = theme != null;
            contextMenuStrip1.Items[1].Enabled = theme != null && ThemeManager.IsThemeDownloaded(theme) &&
                !isWindowsTheme;

            if (JsonConfig.settings.favoriteThemes == null ||
                !JsonConfig.settings.favoriteThemes.Contains(themeId))
            {
                contextMenuStrip1.Items[0].Text = _("Add to favorites");
            }
            else
            {
                contextMenuStrip1.Items[0].Text = _("Remove from favorites");
            }

            if (themeId == null || ThemeManager.defaultThemes.Contains(themeId) || isWindowsTheme)
            {
                contextMenuStrip1.Items[1].Text = _("Delete");
            }
            else
            {
                contextMenuStrip1.Items[1].Text = _("Delete permanently");
            }

            contextMenuStrip1.Items[3].Text = _("Show only installed themes");
            (contextMenuStrip1.Items[3] as ToolStripMenuItem).Checked = JsonConfig.settings.showInstalledOnly;
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
            bool shouldContinue = ThemeDialogUtils.DeleteTheme(theme, listView1, itemIndex);

            if (shouldContinue)
            {
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

        private void showInstalledMenuItem_Click(object sender, EventArgs e)
        {
            ThemeDialogUtils.ToggleShowInstalledOnly(listView1, !JsonConfig.settings.showInstalledOnly);
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
                ThemeDialogUtils.LoadImportedThemes(ThemeManager.importedThemes, listView1, importDialog);
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
            this.Invoke(previewer.ViewModel.Stop);
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
