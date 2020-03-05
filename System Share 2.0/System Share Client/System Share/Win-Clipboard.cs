using System.Threading;
using System.Windows.Forms;

namespace System_Share
{
    class Win_Clipboard
    {
        public static string clip = "";
        public static bool clipWait = false;

        /// <summary>
        /// Updates the public stinrg 'Clip' to the current clipboard.
        /// </summary>
        public static void Get()
        {
            Thread.Sleep(100);
            clipWait = true;
            Thread newThread1 = new Thread(new ThreadStart(GetClip));
            newThread1.SetApartmentState(ApartmentState.STA);
            newThread1.Start();
        }

        /// <summary>
        /// Updates the clipboard to the current string 'Clip'.
        /// </summary>
        public static void Set(string Cl)
        {
            clip = Cl;
            Thread setter = new Thread(new ThreadStart(SetClip));
            setter.SetApartmentState(ApartmentState.STA);
            setter.Start();
        }



        /// <summary>
        /// The thread that grabs the data from the clipboard.
        /// Sets Connection.ClipWait to fals when done
        /// </summary>
        private static void GetClip()
        {
            IDataObject iData = Clipboard.GetDataObject();

            if (iData.GetDataPresent(DataFormats.Text))
            {
                clip = (string)iData.GetData(DataFormats.Text);
            }
            else
            {
                clip = "";
            }
            clipWait = false;
        }

        /// <summary>
        /// The thread that sets the clipboard and pastes it.
        /// </summary>
        private static void SetClip()
        {
            Clipboard.SetText(clip);
            Thread.Sleep(1);
            Win_VirtualKeyboard.KeyDown(86);
        }
    }
}
