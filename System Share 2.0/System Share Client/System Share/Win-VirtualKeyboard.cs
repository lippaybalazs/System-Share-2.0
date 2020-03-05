using System.Runtime.InteropServices;

namespace System_Share
{
    class Win_VirtualKeyboard
    {
        #region Dependencies
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        #endregion

        /// <summary>
        /// Sends a keyboarddown event to the operating system.
        /// </summary>
        public static void KeyDown(byte Key)
        {
            keybd_event(Key, 0, 0x0001 | 0, 0);
        }

        /// <summary>
        /// Sends a keyboardup event to the operating system.
        /// </summary>
        public static void KeyUp(byte Key)
        {
            keybd_event(Key, 0, 0x0001 | 0x0002, 0);
        }
    }
}
