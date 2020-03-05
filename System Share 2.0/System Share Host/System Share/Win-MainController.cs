using System;
using System.Windows.Forms;

namespace System_Share
{
    public partial class Win_MainController : Form
    {
        public static Win_InputManager input;
        public static bool openGUI = false;
        private static Win_GUI gui;


        public Win_MainController()
        {
            InitializeComponent();

            Data.ReadData();
            Data.SaveData();

            HookMouse.Start();
            Win_InputManager.Start();
            Network.Start();
            Processing.Start();

        }

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

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Environment.Exit(0);
            Application.Exit();
        }
        private void RestartNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Network.Restart();
        }
        private void OpenSystemShareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!openGUI)
            {
                gui = new Win_GUI();
                gui.Show();
                openGUI = true;
            }
            else
            {
                gui.TopMost = true;
                gui.TopMost = false;
            }
        }
    }
}
