using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    class FormWrapper : ApplicationContext
    {
        private Container components;
        private MainForm mainForm;
        private NotifyIcon notifyIcon;

        public FormWrapper()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            notifyIcon.Visible = true;
        }

        private void InitializeComponent()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.Icon1;
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("&Settings", settingsItem_Click),
                new MenuItem("E&xit", exitItem_Click)
            });
            notifyIcon.DoubleClick += new System.EventHandler(notifyIcon_DoubleClick);
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMainForm();
        }

        private void settingsItem_Click(object sender, EventArgs e)
        {
            ShowMainForm();
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        public void ShowMainForm()
        {
            if (mainForm == null)
            {
                mainForm = new MainForm();
                mainForm.FormClosed += mainForm_Closed;
                mainForm.Show();
            }
            else
            {
                mainForm.ShowDialog();
            }
        }

        private void mainForm_Closed(object sender, EventArgs e)
        {
            mainForm = null;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (mainForm != null)
            {
                mainForm.Close();
            }
            notifyIcon.Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}