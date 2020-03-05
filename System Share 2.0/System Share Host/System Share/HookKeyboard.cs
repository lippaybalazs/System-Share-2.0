using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System_Share
{
    class HookKeyboard
    {
        #region Dependencies
        public struct KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookStruct lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);
        #endregion

        public static event KeyEventHandler KeyDown = delegate { };
        public static event KeyEventHandler KeyUp = delegate { };
        private static keyboardHookProc proc;
        private static IntPtr hook = IntPtr.Zero;
        public delegate IntPtr keyboardHookProc(int code, int wParam, ref KeyboardHookStruct lParam);
        public static bool hooked = false;

        /// <summary>
        /// Hooks to keyboard
        /// </summary>
        public static void Hook()
        {
            if (!hooked)
            {
                IntPtr hInstance = LoadLibrary("User32");
                proc = HookProc;
                hook = SetWindowsHookEx(13, proc, hInstance, 0);
                hooked = true;
            }
        }

        /// <summary>
        /// Unhooks from keyboard
        /// </summary>
        public static void Unhook()
        {
            if (hooked)
            {
                UnhookWindowsHookEx(hook);
                proc = null;
                hooked = false;
            }
        }

        /// <summary>
        /// Sends keypress events
        /// </summary>
        private static IntPtr HookProc(int code, int wParam, ref KeyboardHookStruct lParam)
        {
            
            if (code >= 0)
            {
                Keys key = (Keys)lParam.vkCode;
                KeyEventArgs kea = new KeyEventArgs(key);
                if ((wParam == 0x100 || wParam == 0x104) && (KeyDown != null))
                {
                    KeyDown(typeof(HookKeyboard), kea);
                }
                else if ((wParam == 0x101 || wParam == 0x105) && (KeyUp != null))
                {
                    KeyUp(typeof(HookKeyboard), kea);
                }
                return (IntPtr)1;
            }
            else if (proc == null)
            {
                return (IntPtr)1;
            }
            else
            {
                return CallNextHookEx(hook, code, wParam, ref lParam);
            }
        }
    }
}
