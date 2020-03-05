using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System_Share
{
    class Win_FullScreenChecker
    {

        #region Dependencies
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        #endregion

        private static Screen screen;
        

        /// <summary>
        /// Checks if something is fullscreened, excludes 'Except' elements
        /// </summary>
        /// <returns></returns>
        public static bool IsForegroundFullScreen()
        {
            if (!Data.FullScreen)
            {
                return false;
            }
            else
            {
                if (screen == null)
                {
                    screen = Screen.PrimaryScreen;
                }
                RECT rect = new RECT();
                IntPtr hWnd = GetForegroundWindow();
                GetWindowRect(new HandleRef(null, hWnd), ref rect);
                GetWindowThreadProcessId(hWnd, out uint procId);
                var proc = System.Diagnostics.Process.GetProcessById((int)procId);
                for (int i = 0; i < Data.Except.Count(); i++)
                {
                    if (proc.ProcessName == Data.Except[i])
                    {
                        return false;
                    }
                }
                return screen.Bounds.Width == (rect.right - rect.left) && screen.Bounds.Height == (rect.bottom - rect.top);
            }

        }
    }
}
