using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace System_Share
{
    class Processing
    {


        private delegate void SafeCallDelegate();
        private static Point lastAnalogPosition;
        private static Point LastValidPosition = new Point(0, 0);
        public static Point VirtualMouse = new Point(0, 0);
        private static bool ShutDown = false;
        private static Thread CycleThread;
        private static bool real = true;
        private static string current = "";
        private static string previous = "";
        public static string keyToSend = "";
        public static string mouseToSend = "";
        private static bool hidden = false;
        private static int currentMain = 0;

        /// <summary>
        /// Starts the processing
        /// </summary>
        public static void Start()
        {
            ShutDown = false;
            CycleThread = new Thread(MainCycle);
            CycleThread.Start();
        }

        /// <summary>
        /// Stops the processing
        /// </summary>
        public static void Stop()
        {
            ShutDown = true;
            CycleThread.Abort();
        }



        /// <summary>
        /// 'The main loop'
        /// </summary>
        private static void MainCycle()
        {
            int skip = 0;
            while (!ShutDown)
            {
                try
                {
                    var p = VirtualMouse;
                    bool found = false;
                    UpdateVirtualMouse();
                    List<string> keys = new List<string>(Network.Clients.Keys);
                    foreach (string key in keys)
                    {
                        if (key != Data.mac)
                        {
                            string toSend = "";
                            if (skip >= 500)
                            {
                                // Send ping
                                toSend += "v;";
                            }
                            if (!Win_FullScreenChecker.IsForegroundFullScreen() && !(real && HookMouse.leftDown))// Disable when foreground fullscreen or left click is down on the real displays
                            {
                                if (!(!real && HookMouse.leftDown) || key == Win_InputManager.selected) // Only selected when left click is down
                                {
                                    if (Win_InputManager.selected == key)
                                    {
                                        // Send keys
                                        string temp = keyToSend;
                                        keyToSend = keyToSend.Substring(temp.Length);
                                        toSend += temp;
                                    }
                                    else if (Win_InputManager.prevSelected == key)
                                    {
                                        toSend += "t;";
                                        Win_InputManager.prevSelected = null;
                                    }
                                    for (int j = 0; j < Network.Clients[key].Indexes.Count; j++)
                                    {
                                        if (Data.OnlineDisplays[Network.Clients[key].Indexes[j]].Area.Contains(VirtualMouse))
                                        {
                                            found = true;
                                            if (real)
                                            {
                                                int x = Data.LocalDisplays[0].Area.Width / 2;
                                                int y = Data.LocalDisplays[0].Area.Height / 2;
                                                lastAnalogPosition = new Point(x, y);
                                                HookMouse.SetCursorPosition(x, y);
                                                // Windows is a piace of shit, this will not fix anything, just reduce the chance of failure
                                            }
                                            real = false;
                                            previous = current;
                                            current = key;
                                            if (VirtualMouse.X != LastValidPosition.X || VirtualMouse.Y != LastValidPosition.Y)
                                            {
                                                // Send mouse position
                                                int x = VirtualMouse.X - Data.OnlineDisplays[Network.Clients[key].Indexes[j]].Area.X;
                                                int y = VirtualMouse.Y - Data.OnlineDisplays[Network.Clients[key].Indexes[j]].Area.Y;
                                                toSend += "m" + j.ToString() + "," + (x).ToString() + "x" + (y).ToString() + ";";
                                            }
                                            // Send mouse keys
                                            string temp = mouseToSend;
                                            mouseToSend = mouseToSend.Substring(temp.Length);
                                            toSend += temp;
                                            break;
                                        }
                                    }
                                    if (previous != current && previous == key)
                                    {
                                        toSend += "h;";
                                    }
                                }
                            }
                            if (!String.IsNullOrWhiteSpace(toSend))
                            {
                                if (!Network.Clients[key].SendCommand(toSend))
                                {
                                    previous = current;
                                    current = Data.mac;
                                    Network.Clients.Remove(key);
                                    Network.UpdateOnline();
                                    real = true;
                                }
                            }
                        }

                    }
                    if (!found)
                    {
                        previous = current;
                        current = Data.mac;
                        if (!real)
                        {
                            for (int i = 0; i < Data.LocalDisplays.Count; i++)
                            {
                                if (Data.OnlineDisplays[i].Area.Contains(VirtualMouse))
                                {
                                    currentMain = i;
                                    break;
                                }
                            }
                            int x = VirtualMouse.X - Data.OnlineDisplays[currentMain].Area.X + Data.LocalDisplays[currentMain].Area.X;
                            int y = VirtualMouse.Y - Data.OnlineDisplays[currentMain].Area.Y + Data.LocalDisplays[currentMain].Area.Y;
                            HookMouse.SetCursorPosition(x, y);
                            // Windows is a piace of shit, this will not fix anything, just reduce the chance of failure
                        }
                        real = true;
                    }
                    if (skip >= 500)
                    {
                        skip = 0;
                    }
                    else
                    {
                        skip++;
                    }
                }
                catch (Exception) { };
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Update the position of the virtual mouse
        /// </summary>
        private static void UpdateVirtualMouse()
        {
            if (real)
            {
                ShowCursor();
                for (int i = 0; i < Data.LocalDisplays.Count; i++)
                {
                    if (Data.OnlineDisplays[i].Area.Contains(VirtualMouse))
                    {
                        currentMain = i;
                        break;
                    }
                }
                Point p = HookMouse.GetCursorPosition();
                int x = p.X + Data.OnlineDisplays[currentMain].Area.X - Data.LocalDisplays[currentMain].Area.X;
                int y = p.Y + Data.OnlineDisplays[currentMain].Area.Y - Data.LocalDisplays[currentMain].Area.Y;
                VirtualMouse = new Point(x, y);
            }
            else
            {
                HideCursor();
                Point p = HookMouse.GetCursorPosition();
                int x = p.X - lastAnalogPosition.X + VirtualMouse.X;
                int y = p.Y - lastAnalogPosition.Y + VirtualMouse.Y;
                lastAnalogPosition = p;
                CenterCursor();
                LastValidPosition = VirtualMouse;
                if (CheckPoint(x, y))
                {
                    VirtualMouse = new Point(x, y);
                }
                else
                {
                    if (CheckPoint(x, LastValidPosition.Y))
                    {
                        VirtualMouse = new Point(x, LastValidPosition.Y);
                    }
                    else if (CheckPoint(LastValidPosition.X, y))
                    {
                        VirtualMouse = new Point(LastValidPosition.X, y);
                    }
                }
            }
        }

        /// <summary>
        /// Centers the cursor on the main monitor
        /// </summary>
        public static void CenterCursor()
        {
            if (Math.Abs(lastAnalogPosition.X - Data.LocalDisplays[0].Area.Width / 2) > Data.LocalDisplays[0].Area.Width / 4 || Math.Abs(lastAnalogPosition.Y - Data.LocalDisplays[0].Area.Height / 2) > Data.LocalDisplays[0].Area.Height / 4)
            {
                int x = Data.LocalDisplays[0].Area.Width / 2;
                int y = Data.LocalDisplays[0].Area.Height / 2;
                lastAnalogPosition = new Point(x, y);
                HookMouse.SetCursorPosition(x, y);
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Show the cursor
        /// </summary>
        private static void ShowCursor()
        {
            if (hidden)
            {
                int x = VirtualMouse.X - Data.OnlineDisplays[currentMain].Area.X + Data.LocalDisplays[currentMain].Area.X;
                int y = VirtualMouse.Y - Data.OnlineDisplays[currentMain].Area.Y + Data.LocalDisplays[currentMain].Area.Y;
                HookMouse.SetCursorPosition(x, y);
                var d = new SafeCallDelegate(Win_InputManager.ShowCursor);
                Win_InputManager.input.Invoke(d, new object[] { });
                hidden = false;
            }
        }

        /// <summary>
        /// Hide the cursor
        /// </summary>
        private static void HideCursor()
        {
            if (!hidden)
            {
                var d = new SafeCallDelegate(Win_InputManager.HideCursor);
                Win_InputManager.input.Invoke(d, new object[] { });
                hidden = true;
                int x = Data.LocalDisplays[0].Area.Width / 2;
                int y = Data.LocalDisplays[0].Area.Height / 2;
                lastAnalogPosition = new Point(x, y);
                HookMouse.SetCursorPosition(x, y);
            }
        }

        /// <summary>
        /// Checks if inside any display
        /// </summary>
        private static bool CheckPoint(int x, int y)
        {
            for (int i = 0; i < Data.OnlineDisplays.Count; i++)
            {
                if (!(!real && HookMouse.leftDown) || Data.OnlineDisplays[i].mac == Win_InputManager.selected) // if left click is down, only check the selected pc
                {
                    if (Data.OnlineDisplays[i].Area.Contains(new Point(x, y)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

}
