// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace WinDynamicDesktop
{
    public partial class ImportDialog : Form
    {
        private static readonly Func<string, string> _ = Localization.GetTranslation;
        private Queue<string> importQueue;
        private int numJobs;

        public ImportDialog()
        {
            InitializeComponent();
            Localization.TranslateForm(this);

            this.Font = SystemFonts.MessageBoxFont;
            this.FormClosing += OnFormClosing;
            ThemeLoader.taskbarHandle = this.Handle;
        }

        public void InitImport(List<string> themePaths)
        {
            ThemeManager.importMode = true;
            importQueue = new Queue<string>(themePaths);
            numJobs = importQueue.Count;

            Task.Run(() => ImportNext());
        }

        private void ImportNext()
        {
            if (ThemeManager.importPaths.Count > 0)
            {
                foreach (string themePath in ThemeManager.importPaths)
                {
                    importQueue.Enqueue(themePath);
                }

                numJobs += ThemeManager.importPaths.Count;
                ThemeManager.importPaths.Clear();
            }

            this.Invoke(new Action(() => UpdateTotalPercentage(0)));

            if (importQueue.Count > 0)
            {
                string themePath = importQueue.Peek();
                this.Invoke(new Action(() =>
                    label1.Text = string.Format(_("Importing theme from {0}..."),
                    Path.GetFileName(themePath))));
                ThemeResult maybeResult = ThemeManager.ImportTheme(themePath);

                if (maybeResult != null)
                {
                    maybeResult.Match(e => this.Invoke(new Action(
                        () => ThemeLoader.HandleError(e))),
                        theme => ThemeManager.importedThemes.Add(theme));
                }

                importQueue.Dequeue();
                ImportNext();
            }
            else
            {
                ThemeManager.importMode = false;
                this.Invoke(new Action(() => this.Close()));
            }
        }

        private void UpdateTotalPercentage(int themePercentage)
        {
            int percentage = ((numJobs - importQueue.Count) * 100 + themePercentage) / numJobs;

            progressBar1.Invoke(new Action(() =>
            {
                progressBar1.Value = percentage;
                progressBar1.Refresh();
                TaskbarProgress.SetValue(this.Handle, percentage, 100);
            }));
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            ThemeLoader.taskbarHandle = IntPtr.Zero;
        }
    }
}
