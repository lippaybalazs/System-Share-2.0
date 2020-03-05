using System;
using System.Drawing;
using System.Windows.Forms;

namespace System_Share
{
    public partial class Win_InputManager : Form
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

        public static Win_InputManager input;
        public static string prevSelected = Data.mac;
        public static string selected = Data.mac;
        private static bool ctrl = false;

        public Win_InputManager()
        {
            InitializeComponent();
            Location = new Point(0, 0);

            MouseWheel += new MouseEventHandler(WinInputManager_MouseWheel);
            HookMouse.MouseAction += new EventHandler(LeftClickHook);
        }
        /// <summary>
        /// Starts the manager
        /// </summary>
        public static void Start()
        {
            input = new Win_InputManager();
            input.Show();
            HookKeyboard.KeyDown += new KeyEventHandler(KeyDownHook);
            HookKeyboard.KeyUp += new KeyEventHandler(KeyUpHook);
        }

        /// <summary>
        /// Hides the Cursor if not already hidden
        /// </summary>
        public static void HideCursor()
        {
            Cursor.Hide();
            input.TopMost = true;
            input.BackColor = Color.Black;
        }

        /// <summary>
        /// Shows the cursor if hidden
        /// </summary>
        public static void ShowCursor()
        {
            Cursor.Show();
            input.TopMost = false;
            input.BackColor = Color.White;
        }


        private void LeftClickHook(object sender, EventArgs e)
        {
            foreach (var disp in Data.OnlineDisplays)
            {
                if (disp.Area.Contains(Processing.VirtualMouse))
                {
                    prevSelected = selected;
                    selected = disp.mac;
                    if (selected == Data.mac)
                    {
                        HookKeyboard.Unhook();
                    }
                    else
                    {
                        HookKeyboard.Hook();
                    }
                }
            }
        }
        private static void KeyUpHook(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 17 || e.KeyValue == 162)
            {
                ctrl = false;
            }
            Processing.keyToSend += "ku" + e.KeyValue.ToString() + ";";
            e.Handled = true;
        }
        private static void KeyDownHook(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 17 || e.KeyValue == 162)
            {
                ctrl = true;
            }
            if (ctrl && e.KeyValue == 86)
            {
                Win_Clipboard.Get();
            }
            else
            {
                Processing.keyToSend += "kd" + e.KeyValue.ToString() + ";";
            }
            if (ctrl && (e.KeyValue == 67 || e.KeyValue == 88))
            {
                Processing.keyToSend += "c;";
            }
            e.Handled = true;
        }
        private void Win_InputManager_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Processing.mouseToSend += "ld;";
                    break;
                case MouseButtons.Right:
                    Processing.mouseToSend += "rd;";
                    break;
                case MouseButtons.Middle:
                    Processing.mouseToSend += "wd;";
                    break;
            }
        }
        private void Win_InputManager_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Processing.mouseToSend += "lu;";
                    break;
                case MouseButtons.Right:
                    Processing.mouseToSend += "ru;";
                    break;
                case MouseButtons.Middle:
                    Processing.mouseToSend += "wu;";
                    break;
            }
        }
        private void WinInputManager_MouseWheel(object sender, MouseEventArgs e)
        {
            int lines = SystemInformation.MouseWheelScrollLines * e.Delta / 3;
            Processing.mouseToSend += "s" + lines.ToString() + ";";
        }
    }

}
