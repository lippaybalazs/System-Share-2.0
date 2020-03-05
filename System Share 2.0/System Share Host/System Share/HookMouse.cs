using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System_Share
{
    class HookMouse
    {
        #region Dependencies

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public Point pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        public static Point Mouse;
        public static bool leftDown = false;

        public static event EventHandler MouseAction = delegate { };
        private static LowLevelMouseProc proc = HookCallback;
        private static IntPtr hook = IntPtr.Zero;
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Sets cursor position
        /// </summary>
        public static void SetCursorPosition(int x, int y)
        {
            SetCursorPos(x, y);
            Mouse = new Point(x, y);
        }

        /// <summary>
        /// Gets cursor position
        /// </summary>
        public static Point GetCursorPosition()
        {
            return Mouse;
        }



        /// <summary>
        /// Hooks to mouse
        /// </summary>
        public static void Start()
        {
            hook = SetHook(proc);
        }

        /// <summary>
        /// Starts hook
        /// </summary>
        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                IntPtr hook = SetWindowsHookEx(14, proc, GetModuleHandle("user32"), 0);
                return hook;
            }
        }

        /// <summary>
        /// Sends mouse events
        /// </summary>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            Mouse = hookStruct.pt;
            if (nCode >= 0 && 0x0201 == (int)wParam)
            {
                leftDown = true;
                MouseAction(null, new EventArgs());
            }
            if (nCode >= 0 && 0x0202 == (int)wParam)
            {
                leftDown = false;
            }
            return CallNextHookEx(hook, nCode, wParam, lParam);
        }
    }
}
