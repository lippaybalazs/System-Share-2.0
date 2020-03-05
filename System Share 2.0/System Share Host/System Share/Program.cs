using System;
using System.Windows.Forms;

namespace System_Share
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Win_MainController());
        }
    }
}
