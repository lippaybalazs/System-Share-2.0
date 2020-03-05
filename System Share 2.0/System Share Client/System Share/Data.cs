using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace System_Share
{
    class Data
    {
        private static string path = GetPath();
        public static string mac = GetMac();
        public static string ip = "127.0.0.1";
        public static int port = 8080;
        public static string name = Environment.MachineName;

        public static List<Display> LocalDisplays = new List<Display>();

        #region Innitials
        /// <summary>
        /// Fetches path to %appdata%\SystemShareHost\
        /// </summary>
        private static string GetPath()
        {
            string p = "";
            //p = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //p = Path.Combine(p, "SystemShareClient");
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
        #endregion


        /// <summary>
        /// Saves data to txt
        /// </summary>
        public static void SaveData()
        {
            SaveUserSettings();
            SaveLocalDisplays();
        }
        #region Save Data methods
        private static void SaveUserSettings()
        {
            using (StreamWriter writetext = new StreamWriter(path + "UserSettings.txt"))
            {
                writetext.WriteLine("Device Name: " + name + ";");
                writetext.WriteLine("Port: " + port.ToString() + ";");
                writetext.WriteLine("Ip: " + ip + ";");
            }
        }
        private static void SaveLocalDisplays()
        {
            using (StreamWriter writetext = new StreamWriter(path + "LocalDisplays.txt"))
            {
                foreach (Display disp in LocalDisplays)
                {
                    string s = "";
                    s += "Width: " + disp.width.ToString();
                    s += " Height: " + disp.height.ToString();
                    s += " X: " + disp.x.ToString();
                    s += " Y: " + disp.y.ToString();
                    writetext.WriteLine(s);
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
            ReadLocalDisplays();
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

                    pattern = new Regex(@"Port: (.+);", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    match = pattern.Match(data);
                    port = Convert.ToInt32(match.Groups[1].Value);

                    pattern = new Regex(@"Ip: (.+);", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    match = pattern.Match(data);
                    ip = match.Groups[1].Value;
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
                    foreach (Match match in matches)
                    {
                        int width = Convert.ToInt32(match.Groups[1].Value);
                        int height = Convert.ToInt32(match.Groups[2].Value);
                        int x = Convert.ToInt32(match.Groups[3].Value);
                        int y = Convert.ToInt32(match.Groups[4].Value);
                        LocalDisplays.Add(new Display(null, null, width, height, x, y));
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
            }
        }
        #endregion

    }

    class Display
    {
        public int width;
        public int height;
        public int x;
        public int y;
        public string name;
        public string mac;

        public Display(string name, string mac, int width, int height, int x, int y)
        {
            this.name = name;
            this.mac = mac;
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
        }
    }
}
