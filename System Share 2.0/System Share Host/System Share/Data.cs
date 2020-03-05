using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace System_Share
{
    class Data
    {
        private static string path = GetPath();
        public static string mac = GetMac();
        public static string ip = GetIP();
        public static int port = GenPort();
        public static string name = Environment.MachineName;
        public static bool FullScreen = false;
        public static List<string> Except = new List<string>();
        public static List<Display> LocalDisplays = new List<Display>();
        public static List<Display> NetworkDisplays = new List<Display>();
        public static List<Display> OnlineDisplays = new List<Display>();

        #region Innitials
        /// <summary>
        /// Fetches path to %appdata%\SystemShareHost\
        /// </summary>
        private static string GetPath()
        {
            string p = "";
            //p = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //p = Path.Combine(p, "SystemShareHost");
            //Directory.CreateDirectory(p);
            //p += @"\";
            return p;
        }

        /// <summary>
        /// Fetches the MAC address
        /// </summary>
        private static string GetMac()
        {
            foreach (var interf in NetworkInterface.GetAllNetworkInterfaces())
            {

                if (interf.OperationalStatus == OperationalStatus.Up && (!interf.Description.Contains("Virtual") && !interf.Description.Contains("Pseudo")))
                {
                    if (interf.GetPhysicalAddress().ToString() != "")
                    {
                        return interf.GetPhysicalAddress().ToString();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Fetches the IP address
        /// </summary>
        private static string GetIP()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }

        /// <summary>
        /// Generates a new unused port
        /// </summary>
        public static int GenPort()
        {
            int p;
            Random rng = new Random();
            bool isAvailable;
            do
            {
                p = rng.Next(8000, 9999);
                isAvailable = true;
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

                foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                {
                    if (tcpi.LocalEndPoint.Port == p)
                    {
                        isAvailable = false;
                        break;
                    }
                }
            } while (!isAvailable);
            return p;
        }
        #endregion

        /// <summary>
        /// Saves data to txt
        /// </summary>
        public static void SaveData()
        {
            SaveUserSettings();
            SaveLocalDisplays();
            SaveNetworkDisplays();
            SaveFullscreenExceptions();
        }
        #region Save Data methods
        private static void SaveUserSettings()
        {
            using (StreamWriter writetext = new StreamWriter(path + "UserSettings.txt"))
            {
                writetext.WriteLine("Device Name: " + name + ";");
                writetext.WriteLine("Port: " + port.ToString() + ";");
                if (FullScreen)
                {
                    writetext.WriteLine("Disable Fullscreen: true");
                }
                else
                {
                    writetext.WriteLine("Disable Fullscreen: false");
                }
            }
        }
        private static void SaveLocalDisplays()
        {
            using (StreamWriter writetext = new StreamWriter(path + "LocalDisplays.txt"))
            {
                foreach (Display disp in LocalDisplays)
                {
                    string s = "";
                    s += "Width: " + disp.Area.Width.ToString();
                    s += " Height: " + disp.Area.Height.ToString();
                    s += " X: " + disp.Area.X.ToString();
                    s += " Y: " + disp.Area.Y.ToString();
                    writetext.WriteLine(s);
                }
            }
        }
        private static void SaveNetworkDisplays()
        {
            using (StreamWriter writetext = new StreamWriter(path + "NetworkDisplays.txt"))
            {
                foreach (Display disp in NetworkDisplays)
                {
                    string s = "";
                    s += "Index: " + disp.index.ToString();
                    s += " Name: " + disp.name;
                    s += " Mac: " + disp.mac;
                    s += " Width: " + disp.Area.Width.ToString();
                    s += " Height: " + disp.Area.Height.ToString();
                    s += " X: " + disp.Area.X.ToString();
                    s += " Y: " + disp.Area.Y.ToString();
                    writetext.WriteLine(s);
                }
            }
        }
        private static void SaveFullscreenExceptions()
        {
            using(StreamWriter writetext = new StreamWriter(path + "FullScreenExceptions.txt"))
            {
                for (int i = 1; i < Except.Count; i++)
                {
                    writetext.WriteLine(Except[i]);
                }
            }
        }
        #endregion

        /// <summary>
        /// Reads data from txt
        /// </summary>
        public static void ReadData()
        {
            ReadUserSettings();
            try
            {
                ReadLocalDisplays();
                ReadNetworkDisplays();
            }
            catch (Exception)
            {
                NetworkDisplays = Win_Screen.Screens(name, mac);
            }
            ReadFullscreenExceptions();
        }
        #region Read Data methods
        private static void ReadUserSettings()
        {
            try
            {
                using (StreamReader readtext = new StreamReader(path + "UserSettings.txt"))
                {
                    Match match;
                    Regex pattern;
                    string data = readtext.ReadToEnd();

                    pattern = new Regex(@"Device Name: (.*);", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    match = pattern.Match(data);
                    name = match.Groups[1].Value;
                    if (String.IsNullOrWhiteSpace(name))
                    {
                        name = Environment.MachineName;
                    }

                    pattern = new Regex(@"Port: (.*);", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    match = pattern.Match(data);
                    string p = match.Groups[1].Value;
                    if (!String.IsNullOrWhiteSpace(p))
                    {
                        port = Convert.ToInt32(match.Groups[1].Value);
                    }

                    pattern = new Regex(@"Disable Fullscreen: (.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    match = pattern.Match(data);
                    FullScreen = match.Groups[1].Value.Trim() == "true";
                }
            }
            catch (Exception) { };
        }
        private static void ReadLocalDisplays()
        {
            try
            {
                using (StreamReader readtext = new StreamReader(path + "LocalDisplays.txt"))
                {
                    string data = readtext.ReadToEnd();
                    Regex pattern = new Regex(@"Width: (.+) Height: (.+) X: (.+) Y: (.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    MatchCollection matches = pattern.Matches(data);
                    int i = 0;
                    foreach (Match match in matches)
                    {
                        int width = Convert.ToInt32(match.Groups[1].Value);
                        int height = Convert.ToInt32(match.Groups[2].Value);
                        int x = Convert.ToInt32(match.Groups[3].Value);
                        int y = Convert.ToInt32(match.Groups[4].Value);
                        LocalDisplays.Add(new Display(i,null, null, width, height, x, y));
                        i++;
                    }
                }
                if (LocalDisplays.Count == 0)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                LocalDisplays = Win_Screen.Screens(null, null);
                throw new Exception();
            }
        }
        private static void ReadNetworkDisplays()
        {
            try
            {
                using (StreamReader readtext = new StreamReader(path + "NetworkDisplays.txt"))
                {
                    string data = readtext.ReadToEnd();
                    Regex pattern = new Regex(@"Index: (.+) Name: (.*) Mac: (.*) Width: (.+) Height: (.+) X: (.+) Y: (.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    MatchCollection matches = pattern.Matches(data);
                    foreach (Match match in matches)
                    {
                        int index = Convert.ToInt32(match.Groups[1].Value);
                        string n = match.Groups[2].Value;
                        string m = match.Groups[3].Value;
                        int width = Convert.ToInt32(match.Groups[4].Value);
                        int height = Convert.ToInt32(match.Groups[5].Value);
                        int x = Convert.ToInt32(match.Groups[6].Value);
                        int y = Convert.ToInt32(match.Groups[7].Value);
                        NetworkDisplays.Add(new Display(index, n, m, width, height, x, y));
                    }
                }
                if (NetworkDisplays.Count == 0)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                NetworkDisplays = Win_Screen.Screens(name, mac);
            }
        }
        private static void ReadFullscreenExceptions()
        {
            Except.Add("System Share");
            try
            {
                using (StreamReader readtext = new StreamReader(path + "FullScreenExceptions.txt"))
                {
                    while (!readtext.EndOfStream)
                    {
                        Except.Add(readtext.ReadLine());
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

    }

    class Display
    {
        public Rectangle Area; 
        public string name;
        public string mac;
        public int index;
        public int poligon = -1;

        public Display(int index,string name, string mac, int width, int height, int x, int y)
        {
            this.index = index;
            this.name = name;
            this.mac = mac;
            Area = new Rectangle(x, y, width, height);
        }
        public Display( Display disp)
        {
            index = disp.index;
            name = disp.name;
            mac = disp.mac;
            Area = new Rectangle(disp.Area.X, disp.Area.Y, disp.Area.Width, disp.Area.Height);
        }

        /// <summary>
        /// Sets every touching rectangle to the same poligon
        /// </summary>
        public void GeneratePoligon(int p, ref List<Display> list)
        {
            poligon = p;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].poligon == -1)
                {
                    Rectangle rec = Rectangle.Intersect(Area, list[i].Area);
                    if (rec.Width > 0 || rec.Height > 0)
                    {
                        list[i].GeneratePoligon(p, ref list);
                    }
                }
            }
        }
    }
}
