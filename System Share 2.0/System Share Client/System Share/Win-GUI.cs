using System;
using System.Windows.Forms;

namespace System_Share
{
    public partial class Win_GUI : Form
    {
        public Win_GUI()
        {
            InitializeComponent();
            textBox1.Text = Data.name;
            textBox2.Text = Data.ip;
            textBox3.Text = Data.port.ToString();
        }
        /// <summary>
        /// Updates Data,saves it, and restarts the network
        /// </summary>
        private void UpdateData()
        {
            if (CheckData())
            {
                Data.name = textBox1.Text;
                Data.ip = textBox2.Text;
                Data.port = Convert.ToInt32(textBox3.Text);
                Data.SaveData();
                Data.ReadData();
                Network.Restart();
            }
        }

        /// <summary>
        /// Checks that no textbox is empty
        /// </summary>
        private bool CheckData()
        {
            bool valid = true;
            if (String.IsNullOrWhiteSpace(textBox1.Text))
            {
                valid = false;
                label5.Visible = true;
            }
            else
            {
                label5.Visible = false;
            }
            if (String.IsNullOrWhiteSpace(textBox2.Text))
            {
                valid = false;
                label6.Visible = true;
            }
            else
            {
                label6.Visible = false;
            }
            if (String.IsNullOrWhiteSpace(textBox3.Text))
            {
                valid = false;
                label7.Visible = true;
            }
            else
            {
                label7.Visible = false;
            }
            return valid;
        }



        private void Button1_Click(object sender, EventArgs e)
        {
            UpdateData();
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            UpdateData();
            if (CheckData())
            {
                Close();
            }
        }
        private void Button4_Click(object sender, EventArgs e)
        {
            Data.LocalDisplays = Win_Screen.Screens(null, null);
            Data.SaveData();
            Network.Restart();
        }
    }
}
