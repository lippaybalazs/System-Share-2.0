using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace System_Share
{
    class Network
    {
        private delegate void SafeCallDelegate();
        private static bool ShutDown;
        private static TcpClient client;
        private static Thread Connection;
        private static string response = "";
        private static bool keySent = false;
        private static bool useKey = false;

        /// <summary>
        /// Starts the network
        /// </summary>
        public static void Start()
        {
            ShutDown = false;
            Connection = new Thread(Chat);
            Connection.Start();
        }

        /// <summary>
        /// Stops the network
        /// </summary>
        public static void Stop()
        {
            ShutDown = true;
            Connection.Abort();
        }

        /// <summary>
        /// Restarts the Network
        /// </summary>
        public static void Restart()
        {
            Stop();
            Start();
        }



        /// <summary>
        /// Connects to host, waits for command, restarts if connection is lost
        /// </summary>
        private static void Chat()
        {
            while (!ShutDown)
            {
                keySent = false;
                useKey = false;
                client = new TcpClient();
                LoopConnect();
                WaitCommand();
                client.Close();
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Keeps searching until it finds the host
        /// </summary>
        private static void LoopConnect()
        {
            while (!ShutDown)
            {
                try
                {
                    client.Connect(IPAddress.Parse(Data.ip), Data.port);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(500);
                }
            }

        }

        /// <summary>
        /// Waits for a command, sends response
        /// </summary>
        /// <returns></returns>
        private static void WaitCommand()
        {
            while (!ShutDown)
            {
                try
                {
                    NetworkStream networkStream = client.GetStream();

                    networkStream.ReadTimeout = 5000;
                    networkStream.WriteTimeout = 5000;

                    byte[] cmdLen = new byte[4];
                    networkStream.Read(cmdLen, 0, 4);
                    int len = BitConverter.ToInt32(cmdLen, 0);
                    byte[] cmdBytes = new byte[len];
                    networkStream.Read(cmdBytes, 0, cmdBytes.Length);
                    string cmd = Crypto.Decrypt(cmdBytes,useKey);
                    cmd = cmd.Substring(0, cmd.IndexOf("\0"));
                    Console.WriteLine(cmd);
                    Decode(cmd);
                    cmd = response + "v;";
                    response = "";
                    cmd += "\0";
                    cmdBytes = Crypto.Encrypt(cmd,useKey);
                    cmdLen = BitConverter.GetBytes(cmdBytes.Length);
                    networkStream.Write(cmdLen, 0, 4);
                    networkStream.Write(cmdBytes, 0, cmdBytes.Length);

                    networkStream.Flush();
                    useKey = keySent;
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private static void Decode(string cmd)
        {
            while (!String.IsNullOrEmpty(cmd))
            {
                switch (cmd[0])
                {
                    case 'm':
                        MoveMouse(ref cmd);
                        break;
                    case 'l':
                        LeftClick(ref cmd);
                        break;
                    case 'r':
                        RightClick(ref cmd);
                        break;
                    case 'w':
                        MiddleClick(ref cmd);
                        break;
                    case 's':
                        Scroll(ref cmd);
                        break;
                    case 'k':
                        KeyPress(ref cmd);
                        break;
                    case 'v':
                        Empty(ref cmd);
                        break;
                    case 'p':
                        Paste(ref cmd);
                        break;
                    case 'c':
                        Copy(ref cmd);
                        break;
                    case 't':
                        LoseFocus(ref cmd);
                        break;
                    case 'h':
                        HideCursor(ref cmd);
                        break;
                    case 'd':
                        SendDisplay(ref cmd);
                        break;
                    case 'a':
                        UpdateKey(ref cmd);
                        break;
                    case 'j':
                        LogIn(ref cmd);
                        break;
                    default:
                        cmd = "";
                        break;

                }
            }   
        }
        #region Decoder

        /// <summary>
        /// 'a2325,32523,326.....;'
        /// </summary>
        private static void UpdateKey(ref string cmd)
        {
            string key = cmd.Substring(1, cmd.IndexOf(';') - 1);
            Crypto.rsaPublic = key;
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
            var encoder = new UnicodeEncoding();
            string IV = encoder.GetString(Crypto.GetVector());
            string Key = encoder.GetString(Crypto.GetKey());
            response += "a" + Crypto.Encrypt(IV,Crypto.rsaPublic) + ";";
            response += Crypto.Encrypt(Key, Crypto.rsaPublic) + ";";
            keySent = true;
        }

        /// <summary>
        /// 'p<3>asd;'
        /// </summary>
        private static void Paste(ref string cmd)
        {
            int len = Convert.ToInt32(cmd.Substring(cmd.IndexOf('<') + 1, cmd.IndexOf('>') - cmd.IndexOf('<') - 1));
            string msg = cmd.Substring(cmd.IndexOf('>') + 1, len);
            Win_Clipboard.Set(msg);
            cmd = cmd.Substring(len.ToString().Length + 4 + len);
        }

        /// <summary>
        /// Sends the clipboard
        /// </summary>
        private static void Copy(ref string cmd)
        {
            Win_Clipboard.Get();
            while (Win_Clipboard.clipWait)
            {
                Thread.Sleep(1); /// wait for thread to update clipboard data
            }
            string clip = Win_Clipboard.clip;
            response += "p<" + Win_Clipboard.clip.Length.ToString() + ">" + Win_Clipboard.clip + ";";
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        ///'lu;' or 'ld;'
        /// </summary>
        private static void LeftClick(ref string cmd)
        {
            if (cmd[1] == 'd')
            {
                Win_VirtualMouse.MouseEvent(Win_VirtualMouse.MouseEventFlags.LeftDown);
            }
            else if (cmd[1] == 'u')
            {
                Win_VirtualMouse.MouseEvent(Win_VirtualMouse.MouseEventFlags.LeftUp);
            }
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 'ru;' or 'rd;'
        /// </summary>
        private static void RightClick(ref string cmd)
        {
            if (cmd[1] == 'd')
            {
                Win_VirtualMouse.MouseEvent(Win_VirtualMouse.MouseEventFlags.RightDown);
            }
            else if (cmd[1] == 'u')
            {
                Win_VirtualMouse.MouseEvent(Win_VirtualMouse.MouseEventFlags.RightUp);
            }
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 'wu;' or 'wd;'
        /// </summary>
        private static void MiddleClick(ref string cmd)
        {
            if (cmd[1] == 'd')
            {
                Win_VirtualMouse.MouseEvent(Win_VirtualMouse.MouseEventFlags.MiddleDown);
            }
            else if (cmd[1] == 'u')
            {
                Win_VirtualMouse.MouseEvent(Win_VirtualMouse.MouseEventFlags.MiddleUp);
            }
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 's500;' or 's-500;'
        /// </summary>
        private static void Scroll(ref string cmd)
        {
            Win_VirtualMouse.MouseWheel(Convert.ToInt32(cmd.Substring(1, cmd.IndexOf(';') - 1)));
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 'ku12;' or 'kd12;'
        /// </summary>
        private static void KeyPress(ref string cmd)
        {
            if (cmd[1] == 'd')
            {
                Win_VirtualKeyboard.KeyDown(Convert.ToByte(cmd.Substring(2, cmd.IndexOf(';') - 2)));
            }
            else if (cmd[1] == 'u')
            {
                Win_VirtualKeyboard.KeyUp(Convert.ToByte(cmd.Substring(2, cmd.IndexOf(';') - 2)));
            }
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 'm0,500x500;'
        /// </summary>
        private static void MoveMouse(ref string cmd)
        {
            int x = Convert.ToInt32(cmd.Substring(cmd.IndexOf(",") + 1, cmd.IndexOf("x") - cmd.IndexOf(",") - 1)) + Data.LocalDisplays[Convert.ToInt32(cmd.Substring(1, cmd.IndexOf(",") - 1))].x;
            int y = Convert.ToInt32(cmd.Substring(cmd.IndexOf("x") + 1, cmd.IndexOf(';') - cmd.IndexOf("x") - 1)) + Data.LocalDisplays[Convert.ToInt32(cmd.Substring(1, cmd.IndexOf(",") - 1))].y;
            Win_VirtualMouse.SetCursorPosition(x, y);
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 't;'
        /// </summary>
        private static void LoseFocus(ref string cmd)
        {
            var d = new SafeCallDelegate(Win_MainController.GiveFocus);
            Win_MainController.contr.Invoke(d, new object[] { });
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// h;
        /// </summary>
        private static void HideCursor(ref string cmd)
        {
            Win_VirtualMouse.SetCursorPosition(0, Data.LocalDisplays[0].height);
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 'j;'
        /// </summary>
        private static void LogIn(ref string cmd)
        {
            response += "l" + Data.mac + ";";
            response += "n" + Data.name + ";";
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 'd;'
        /// </summary>
        private static void SendDisplay(ref string cmd)
        {
            foreach (Display disp in Data.LocalDisplays)
            {
                response += "d" + disp.width + "x" + disp.height + ";";
            }
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 'v;'
        /// </summary>
        private static void Empty(ref string cmd)
        {
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }
        #endregion

    }
}
