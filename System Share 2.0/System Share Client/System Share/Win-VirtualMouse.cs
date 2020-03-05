using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System_Share
{
    class Win_VirtualMouse
    {
        #region Dependencies
        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            Wheel = 0x00000800
        }

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out Point lpMousePoint);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        #endregion

        ///<summary>
        ///Sets the mouse pointer position to the given coordinates
        ///</summary>
        public static void SetCursorPosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

        ///<summary>
        ///Gets the coordinates of the mouse pointer
        ///</summary>
        public static Point GetCursorPosition()
        {
            Point position;
            if (GetCursorPos(out position))
            {
                position = new Point(0, 0);
            }
            return position;
        }



        ///<summary>
        ///Sends a mouse key event to the operating system
        ///</summary>
        public static void MouseEvent(MouseEventFlags value)
        {
            Point position = GetCursorPosition();
            mouse_event((int)value, position.X, position.Y, 0, 0);
        }

        ///<summary>
        ///Sends x lines of scrolling to the operating system
        ///</summary>
        public static void MouseWheel(int x)
        {
            Point position = GetCursorPosition();
            mouse_event((int)MouseEventFlags.Wheel, position.X, position.Y, x, 0);
        }
    }
}
