using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace System_Share
{
    public partial class Win_GUI : Form
    {
        private struct Disp
        {
        }

        private static List<string> log = new List<string>();
        private delegate void SafeCallDelegate();
        private static Win_GUI form = null;

        private List<Display> networkDisplays = new List<Display>();
        private List<Display> onlineDisplays = new List<Display>();
        private string selected = "";
        private int dragIndex = -1;
        private Point offset;
        private Point previous;
        private bool down = false;
        int scale = 10;

        public Win_GUI()
        {
            InitializeComponent();
            form = this;
            LoadData();
            
        }
        /// <summary>
        /// Updates the visual log
        /// </summary>
        public static void LogWrite(string s)
        {
            log.Add(s);
            if (form != null)
            {
                var d = new SafeCallDelegate(form.LoadLog);
                form.Invoke(d, new object[] { });
            }
        }

        /// <summary>
        /// Refreshes the GUI data regarding network displays
        /// </summary>
        public static void LoadNetwork()
        {
            if (form != null)
            {
                var d = new SafeCallDelegate(form.LoadNetworkData);
                form.Invoke(d, new object[] { });
            }
        }

        /// <summary>
        /// Refreshes the GUI data regarding online displays
        /// </summary>
        public static void LoadOnline()
        {
            if (form != null)
            {
                var d = new SafeCallDelegate(form.LoadOnlineData);
                form.Invoke(d, new object[] { });
            }
        }




        /// <summary>
        /// Fills up the fields for data
        /// </summary>
        private void LoadData()
        {
            LoadLog();
            LoadFS();
            UpdateFSControls();
            textBox1.Text = Data.name;
            textBox2.Text = Data.ip;
            textBox6.Text = Data.port.ToString();
            checkBox1.Checked = Data.FullScreen;
            LoadNetworkData();
            LoadOnlineData();
        }

        /// <summary>
        /// Updates the fullscreen exception list
        /// </summary>
        private void LoadFS()
        {
            listBox1.Items.Clear();
            for(int i = 1; i < Data.Except.Count; i++)
            {
                listBox1.Items.Add(Data.Except[i]);
            }
        }

        /// <summary>
        /// Enables/Disables fullscreen controlls
        /// </summary>
        private void UpdateFSControls()
        {
            listBox1.Enabled = checkBox1.Checked;
            button11.Enabled = checkBox1.Checked;
            button12.Enabled = checkBox1.Checked;
            textBox3.Enabled = checkBox1.Checked;
        }

        /// <summary>
        /// Updates the log listbox
        /// </summary>
        private void LoadLog()
        {
            try
            {
                listBox2.Items.Clear();
                foreach (string s in log)
                {
                    listBox2.Items.Add(s);
                }
                listBox2.SelectedIndex = listBox2.Items.Count - 1;
                listBox2.SelectedIndex = -1;
            }
            catch (Exception) { };
        }

        /// <summary>
        /// Loads network display data
        /// </summary>
        private void LoadNetworkData()
        {
            networkDisplays.Clear();
            foreach(Display disp in Data.NetworkDisplays)
            {
                networkDisplays.Add(new Display(disp));
            }
            pictureBox1.Refresh();
        }

        /// <summary>
        /// Loads network display data
        /// </summary>
        private void LoadOnlineData()
        {
            onlineDisplays.Clear();
            foreach (Display disp in Data.OnlineDisplays)
            {
                onlineDisplays.Add(new Display(disp));
            }
            pictureBox2.Refresh();
        }

        /// <summary>
        /// Saves all data from gui
        /// </summary>
        private void SaveData()
        {
            Processing.Stop();
            Network.Stop();
            Data.name = textBox1.Text;
            Data.port = Convert.ToInt32(textBox6.Text);
            Data.Except.Clear();
            Data.Except.Add("System Share");
            foreach (string s in listBox1.Items)
            {
                Data.Except.Add(s);
            }
            Data.NetworkDisplays.Clear();
            for( int i = 0; i < networkDisplays.Count; i++)
            {
                networkDisplays[i].Area.X -= networkDisplays[0].Area.X;
                networkDisplays[i].Area.Y -= networkDisplays[0].Area.Y;
                Data.NetworkDisplays.Add(new Display(networkDisplays[i]));
            }
            Data.SaveData();
            Network.Start();
            Processing.Start();
        }





        #region User Settings
        private void TextBox1_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = Environment.MachineName;
            }
        }
        private void TextBox3_Enter(object sender, EventArgs e)
        {
            if(textBox3.Text == "Process name (Task Manager -> Details)")
            {
                textBox3.Text = "";
            }
        }
        private void TextBox3_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox3.Text))
            {
                textBox3.Text = "Process name (Task Manager -> Details)";
            }
        }
        private void Button9_Click(object sender, EventArgs e)
        {
            textBox6.Text = Data.GenPort().ToString();
        }
        private void Button13_Click(object sender, EventArgs e)
        {
            Network.Restart();
        }
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFSControls();
        }
        private void Button11_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox3.Text);
            textBox3.Text = "Process name (Task Manager -> Details)";
        }
        private void Button12_Click(object sender, EventArgs e)
        {
            listBox1.Items.Remove(listBox1.SelectedItem);
        }
        #endregion


        #region Network Displays
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                for (int i = 0; i < networkDisplays.Count; i++)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(networkDisplays[i].Area.X / scale + pictureBox1.Width / 2 - 2, networkDisplays[i].Area.Y / scale + pictureBox1.Height / 2 - 2, networkDisplays[i].Area.Width / scale + 4, networkDisplays[i].Area.Height / scale + 4));
                }
                for (int i = 0; i < networkDisplays.Count; i++)
                {
                    if (networkDisplays[i].mac == selected)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(networkDisplays[i].Area.X / scale + pictureBox1.Width / 2, networkDisplays[i].Area.Y / scale + pictureBox1.Height / 2, networkDisplays[i].Area.Width / scale, networkDisplays[i].Area.Height / scale));
                        e.Graphics.FillRectangle(new SolidBrush(Color.LightBlue), new Rectangle(networkDisplays[i].Area.X / scale + pictureBox1.Width / 2 + 1, networkDisplays[i].Area.Y / scale + pictureBox1.Height / 2 + 1, networkDisplays[i].Area.Width / scale - 2, networkDisplays[i].Area.Height / scale - 2));
                        e.Graphics.FillRectangle(new SolidBrush(Color.RoyalBlue), new Rectangle(networkDisplays[i].Area.X / scale + pictureBox1.Width / 2 + 4, networkDisplays[i].Area.Y / scale + pictureBox1.Height / 2 + 4, networkDisplays[i].Area.Width / scale - 8, networkDisplays[i].Area.Height / scale - 8));
                    }
                    else
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(networkDisplays[i].Area.X / scale + pictureBox1.Width / 2, networkDisplays[i].Area.Y / scale + pictureBox1.Height / 2, networkDisplays[i].Area.Width / scale, networkDisplays[i].Area.Height / scale));
                        e.Graphics.FillRectangle(new SolidBrush(Color.RoyalBlue), new Rectangle(networkDisplays[i].Area.X / scale + pictureBox1.Width / 2 + 1, networkDisplays[i].Area.Y / scale + pictureBox1.Height / 2 + 1, networkDisplays[i].Area.Width / scale - 2, networkDisplays[i].Area.Height / scale - 2));
                    }
                    e.Graphics.DrawString(networkDisplays[i].index.ToString() + ", " + networkDisplays[i].name, new Font("Arial", 8), Brushes.White, networkDisplays[i].Area.X / scale + pictureBox1.Width / 2 + 5, networkDisplays[i].Area.Y / scale + pictureBox1.Height / 2 + 5);
                }
            }
            catch (Exception) { };
        }
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            dragIndex = -1;
            selected = "";
            for (int i = 0; i < networkDisplays.Count; i++)
            {
                if (networkDisplays[i].Area.Contains((e.Location.X - pictureBox1.Width / 2) * scale, (e.Location.Y - pictureBox1.Height / 2) * scale))
                {
                    selected = networkDisplays[i].mac;
                    dragIndex = i;
                    offset = new Point((e.Location.X - pictureBox1.Width / 2) * scale - networkDisplays[i].Area.X, (e.Location.Y - pictureBox1.Height / 2) * scale - networkDisplays[i].Area.Y);
                }
            }
            pictureBox1.Refresh();
            down = true;
        }
        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            down = false;
            Geometry.ArrangeAll(ref networkDisplays);
            pictureBox1.Refresh();
        }
        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (down)
            {
                if (dragIndex != -1)
                {
                    try
                    {
                        int destX = (e.Location.X - pictureBox1.Width / 2) * scale - offset.X;
                        int destY = (e.Location.Y - pictureBox1.Height / 2) * scale - offset.Y;
                        int prevX = networkDisplays[dragIndex].Area.X;
                        int prevY = networkDisplays[dragIndex].Area.Y;
                        int incX = 1;
                        int incY = 1;
                        if (destX < prevX)
                        {
                            incX *= -1;
                        }
                        if (destY < prevY)
                        {
                            incY *= -1;
                        }
                        while (networkDisplays[dragIndex].Area.X != destX)
                        {
                            networkDisplays[dragIndex].Area.X += incX;
                            if (!Geometry.Valid(dragIndex, networkDisplays))
                            {
                                networkDisplays[dragIndex].Area.X -= incX;
                                break;
                            }
                        }
                        while (networkDisplays[dragIndex].Area.Y != destY)
                        {
                            networkDisplays[dragIndex].Area.Y += incY;
                            if (!Geometry.Valid(dragIndex, networkDisplays))
                            {
                                networkDisplays[dragIndex].Area.Y -= incY;
                                break;
                            }
                        }
                    }
                    catch (Exception) { };
                }
                else
                {
                    for (int i = 0; i < networkDisplays.Count; i++)
                    {
                        networkDisplays[i].Area.X += (e.Location.X - previous.X) * scale;
                        networkDisplays[i].Area.Y += (e.Location.Y - previous.Y) * scale;
                    }
                }
                pictureBox1.Refresh();
            }
            previous = e.Location;
        }
        private void TrackBar1_ValueChanged(object sender, EventArgs e)
        {
            scale = trackBar1.Value + 10;
            trackBar2.Value = trackBar1.Value;
            pictureBox1.Refresh();
        }
        private void Button4_Click(object sender, EventArgs e)
        {
            if (selected == Data.mac)
            {
                MessageBox.Show("You can not delete main Display!");
            }
            else
            {
                for (int i = 0; i < networkDisplays.Count; i++)
                {
                    if (networkDisplays[i].mac == selected)
                    {
                        networkDisplays.RemoveAt(i);
                        i--;
                    }
                }
            }
            pictureBox1.Refresh();
        }
        private void Button5_Click(object sender, EventArgs e)
        {
            networkDisplays.Clear();
            for (int i = 0; i < Data.LocalDisplays.Count; i++)
            {
                networkDisplays.Add(new Display(i, Data.name, Data.mac, Data.LocalDisplays[i].Area.Width, Data.LocalDisplays[i].Area.Height, Data.LocalDisplays[i].Area.X, Data.LocalDisplays[i].Area.Y));
            }
            pictureBox1.Refresh();
        }
        private void Button6_Click(object sender, EventArgs e)
        {
            selected = "";
            LoadNetworkData();
        }
        #endregion



        #region Online Displays
        private void PictureBox2_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                for (int i = 0; i < onlineDisplays.Count; i++)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(onlineDisplays[i].Area.X / scale + pictureBox1.Width / 2 - 2, onlineDisplays[i].Area.Y / scale + pictureBox1.Height / 2 - 2, onlineDisplays[i].Area.Width / scale + 4, onlineDisplays[i].Area.Height / scale + 4));
                }
                for (int i = 0; i < onlineDisplays.Count; i++)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(onlineDisplays[i].Area.X / scale + pictureBox1.Width / 2, onlineDisplays[i].Area.Y / scale + pictureBox1.Height / 2, onlineDisplays[i].Area.Width / scale, onlineDisplays[i].Area.Height / scale));
                    e.Graphics.FillRectangle(new SolidBrush(Color.RoyalBlue), new Rectangle(onlineDisplays[i].Area.X / scale + pictureBox1.Width / 2 + 1, onlineDisplays[i].Area.Y / scale + pictureBox1.Height / 2 + 1, onlineDisplays[i].Area.Width / scale - 2, onlineDisplays[i].Area.Height / scale - 2));
                    e.Graphics.DrawString(onlineDisplays[i].index.ToString() + ", " + onlineDisplays[i].name, new Font("Arial", 8), Brushes.White, onlineDisplays[i].Area.X / scale + pictureBox1.Width / 2 + 5, onlineDisplays[i].Area.Y / scale + pictureBox1.Height / 2 + 5);
                }
            }
            catch (Exception) { };
        }
        private void PictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            down = true;
        }
        private void PictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            down = false;
        }
        private void PictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (down)
            {
                for (int i = 0; i < onlineDisplays.Count; i++)
                {
                    onlineDisplays[i].Area.X += (e.Location.X - previous.X) * scale;
                    onlineDisplays[i].Area.Y += (e.Location.Y - previous.Y) * scale;
                    pictureBox2.Refresh();
                }
            }
            previous = e.Location;
        }
        private void Button7_Click(object sender, EventArgs e)
        {
            LoadOnlineData();
        }
        #endregion

        #region Form Controll
        private void Button1_Click(object sender, EventArgs e)
        {
            SaveData();
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            SaveData();
            Close();
        }
        private void Win_GUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Win_MainController.openGUI = false;
        }
        #endregion
    }
}
