using System.Collections.Generic;

namespace System_Share
{
    class Win_Screen
    {
        /// <summary>
        /// Fetches all screens and converts them into Display class
        /// </summary>
        public static List<Display> Screens(string name, string mac)
        {
            List<Display> Monitors = new List<Display>();
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                Monitors.Add(new Display(name, mac, screen.Bounds.Width, screen.Bounds.Height, screen.Bounds.X, screen.Bounds.Y));
            }
            return Monitors;
        }
    }
}
