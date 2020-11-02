// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class MessageDialog
    {
        public static DialogResult ShowError(string message, string title = null)
        {
            return MessageBox.Show(message, title ?? "WinDynamicDesktop", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult ShowInfo(string message, string title = null, bool cancelButton = false)
        {
            return MessageBox.Show(message, title ?? "WinDynamicDesktop",
                cancelButton ? MessageBoxButtons.OKCancel : MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static DialogResult ShowQuestion(string message, string title = null, bool useWarningIcon = false)
        {
            return MessageBox.Show(message, title ?? "WinDynamicDesktop", MessageBoxButtons.YesNo,
                useWarningIcon ? MessageBoxIcon.Warning : MessageBoxIcon.Question);
        }

        public static DialogResult ShowWarning(string message, string title = null)
        {
            return MessageBox.Show(message, title ?? "WinDynamicDesktop", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
