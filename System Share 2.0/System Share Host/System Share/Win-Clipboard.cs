using System.Threading;
using System.Windows.Forms;

namespace System_Share
{
    class Win_Clipboard
    {
        private static string clip;

        /// <summary>
        /// Fetches the clipboard
        /// </summary>
        public static void Get()
        {
            Thread ClipFetcher = new Thread(new ThreadStart(GetClip));
            ClipFetcher.SetApartmentState(ApartmentState.STA);
            ClipFetcher.Start();
        }

        /// <summary>
        /// Sets the cliboard
        /// </summary>
        public static void Set(string Cl)
        {
            clip = Cl;
            Thread ClipSetter = new Thread(new ThreadStart(SetClip));
            ClipSetter.SetApartmentState(ApartmentState.STA);
            ClipSetter.Start();
        }



        /// <summary>
        /// Gets the clipboard
        /// </summary>
        private static void GetClip()
        {
            IDataObject iData = Clipboard.GetDataObject();

            if (iData.GetDataPresent(DataFormats.Text))
            {
                clip = (string)iData.GetData(DataFormats.Text);
            }
            Processing.keyToSend = "p<" + clip.Length.ToString() + ">" + clip + ";";
        }

        /// <summary>
        /// Sets the clipboard
        /// </summary>
        private static void SetClip()
        {
            Clipboard.SetText(clip);
        }
    }
}
