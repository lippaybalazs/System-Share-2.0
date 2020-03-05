using System;
using System.Windows.Forms;

namespace System_Share
{
    public partial class Win_MainController : Form
    {
        /// <summary>
        /// Hide from Task Manager
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }


        private delegate void SafeCallDelegate();
        public static Win_MainController contr;

        public Win_MainController()
        {
            InitializeComponent();
            contr = this;

            Data.ReadData();
            Data.SaveData();
            Network.Start();
        }


        private void OpenSystemShareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gui = new Win_GUI();
            gui.Show();
        }
        private void RestartNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Network.Restart();
        }
        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Environment.Exit(0);
            Application.Exit();
        }
        public static void GiveFocus()
        {
            contr.Focus();
        }
        
    }
}
