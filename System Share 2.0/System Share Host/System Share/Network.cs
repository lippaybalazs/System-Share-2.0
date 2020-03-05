using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace System_Share
{
    class Network
    {
        private static TcpListener listen;
        private static bool ShutDown;
        private static Thread Listener;
        public static Dictionary<string,Client> Clients = new Dictionary<string,Client>();

        /// <summary>
        /// Starts the network
        /// </summary>
        public static void Start()
        {
            Win_GUI.LogWrite("Network started");
            ShutDown = false;
            Listener = new Thread(Listen);
            Listener.Start();
        }

        /// <summary>
        /// Stops the network
        /// </summary>
        public static void Stop()
        {
            Win_GUI.LogWrite("Network stopped");
            ShutDown = true;
            Listener.Abort();
            listen.Stop();
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
        /// Listen for connections and autenticate them
        /// </summary>
        private static void Listen()
        {
            ClearOnline();
            UpdateOnline();
            listen = new TcpListener(IPAddress.Any, Data.port);
            listen.Start();
            while (!ShutDown)
            {
                if (!listen.Pending())
                {
                    Thread.Sleep(500);
                }
                else
                {
                    TcpClient clientSocket = listen.AcceptTcpClient();
                    Win_GUI.LogWrite("Client connected");
                    Autenticate(new Client(clientSocket));
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// Autenticates the client
        /// </summary>
        /// <param name="client"></param>
        private static void Autenticate(Client client)
        {
            Win_GUI.LogWrite("Autentication...");
            Crypto.RSAGenerate();
            client.SendCommand("a" + Crypto.rsaPublic + ";");

            Win_GUI.LogWrite("    Key Exchange finished");
            client.SendCommand("j;");
            if (!UpdateName(client))
            {
                Win_GUI.LogWrite("    New computer detected");
                UpdateNetwork(client);
                Data.SaveData();
                Win_GUI.LogWrite("    Data updated");
                Win_GUI.LoadNetwork();
            }
            try
            {
                Clients.Add(client.mac, client);
            }
            catch (Exception)
            {
                if (client.mac != null)
                {
                    Clients[client.mac] = client;
                }
            }
            Win_GUI.LogWrite("    " + client.name + " autenticated");
            UpdateOnline();

            Win_GUI.LogWrite("Autentication finished");
        }

        /// <summary>
        /// Resets the online list
        /// </summary>
        private static void ClearOnline()
        {
            Clients.Clear();
            Clients.Add(Data.mac, new Client(null));
        }

        /// <summary>
        /// Resets every virtual display that has a coresponding client
        /// </summary>
        public static void UpdateOnline()
        {
            List<Display> temp = new List<Display>();
            foreach (string key in Clients.Keys)
            {
                for (int i = 0; i < Data.NetworkDisplays.Count; i++)
                {
                    string asd = Data.NetworkDisplays[i].mac;
                    if (Data.NetworkDisplays[i].mac == key)
                    {
                        Clients[key].Indexes.Add(i);
                        temp.Add(new Display(Data.NetworkDisplays[i]));
                    }
                }
            }
            Data.OnlineDisplays = temp;
            Geometry.ArrangeAll(ref Data.OnlineDisplays);
            Win_GUI.LoadOnline();
        }

        /// <summary>
        /// Updates the display names coresponding to the client, if not found, return false
        /// </summary>
        private static bool UpdateName(Client client)
        {
            bool found = false;
            for (int i = 0; i < Data.NetworkDisplays.Count; i++)
            {
                if (Data.NetworkDisplays[i].mac == client.mac)
                {
                    found = true;
                    Data.NetworkDisplays[i].name = client.name;
                }
            }
            return found;
        }

        /// <summary>
        /// Asks for display data, and updates the network displays
        /// </summary>
        private static void UpdateNetwork(Client client)
        {
            client.SendCommand("d;");
            foreach (Display disp in client.Temporary)
            {
                Geometry.Insert(disp, ref Data.NetworkDisplays);
            }
        }
    }

    class Client
    {
        private TcpClient client;
        public string mac;
        public string name = "unknown client";
        public List<Display> Temporary = new List<Display>();
        public List<int> Indexes = new List<int>();

        private byte[] AESIV = null;
        private byte[] AESKey = null;

        public Client(TcpClient clientSocket)
        {
            client = clientSocket;
        }

        /// <summary>
        /// Sends a string to the client, returns an answer string
        /// </summary>
        public bool SendCommand(string cmd)
        {
            try
            {
                NetworkStream networkStream = client.GetStream();

                networkStream.ReadTimeout = 2000;
                networkStream.WriteTimeout = 2000;

                cmd += "\0";
                byte[] cmdBytes = Crypto.Encrypt(cmd,AESKey,AESIV);
                byte[] cmdLen = BitConverter.GetBytes(cmdBytes.Length);
                networkStream.Write(cmdLen, 0, 4);
                networkStream.Write(cmdBytes, 0, cmdBytes.Length);

                cmdLen = new byte[4];
                networkStream.Read(cmdLen, 0, 4);
                int len = BitConverter.ToInt32(cmdLen, 0);
                cmdBytes = new byte[len];
                networkStream.Read(cmdBytes, 0, len);
                cmd = Crypto.Decrypt(cmdBytes,AESKey,AESIV);
                cmd = cmd.Substring(0, cmd.IndexOf("\0"));
                Console.WriteLine(cmd);
                networkStream.Flush();
                Decode(cmd);
                return true;
            }
            catch (Exception)
            {
                Win_GUI.LogWrite("Connection failed with " + name);
                return false;
            }
        }

        /// <summary>
        /// Decodes the given command and executes it
        /// </summary>
        private void Decode(string cmd)
        {
            while (!String.IsNullOrEmpty(cmd))
            {
                switch (cmd[0])
                {
                    case 'v':
                        Empty(ref cmd);
                        break;
                    case 'p':
                        UpdateClipboard(ref cmd);
                        break;
                    case 'd':
                        SaveDisplay(ref cmd);
                        break;
                    case 'l':
                        LogIn(ref cmd);
                        break;
                    case 'n':
                        UpdateName(ref cmd);
                        break;
                    case 'a':
                        UpdateKey(ref cmd);
                        break;
                    default:
                        throw new Exception("bad string");
                }
            }
        }

        #region Decoder
        /// <summary>
        /// 'a2325,32523,326.....;12214,125125,125125....;'
        /// </summary>
        private void UpdateKey(ref string cmd)
        {
            string iv = cmd.Substring(1, cmd.IndexOf(';') - 1);
            var encoder = new UnicodeEncoding();
            AESIV = encoder.GetBytes(Crypto.Decrypt(iv,Crypto.rsaPrivate));
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
            string key = cmd.Substring(0, cmd.IndexOf(';'));
            AESKey = encoder.GetBytes(Crypto.Decrypt(key, Crypto.rsaPrivate));
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// Updates clipboard
        /// </summary>
        private void UpdateClipboard(ref string cmd)
        {
                int len = Convert.ToInt32(cmd.Substring(cmd.IndexOf('<') + 1, cmd.IndexOf('>') - cmd.IndexOf('<') - 1));
                string msg = cmd.Substring(cmd.IndexOf('>') + 1, len);
                Win_Clipboard.Set(msg);
                cmd = cmd.Substring(len.ToString().Length + 4 + len);
        }

        /// <summary>
        /// 'd1920x1080;
        /// </summary>
        private void SaveDisplay(ref string cmd)
        {
            int width = Convert.ToInt32(cmd.Substring(1, cmd.IndexOf("x") - 1));
            int height = Convert.ToInt32(cmd.Substring(cmd.IndexOf("x") + 1, cmd.IndexOf(";") - cmd.IndexOf("x") - 1));
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
            Temporary.Add(new Display(Temporary.Count, name, mac, width, height, 0, 0));
        }

        /// <summary>
        /// 'l00D86137774B;'
        /// </summary>
        private void LogIn(ref string cmd)
        {
            mac = cmd.Substring(1, cmd.IndexOf(";") - 1);
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 'nASUS;'
        /// </summary>
        private void UpdateName(ref string cmd)
        {
            name = cmd.Substring(1, cmd.IndexOf(";") - 1);
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }

        /// <summary>
        /// 'v;'
        /// </summary>
        private void Empty(ref string cmd)
        {
            cmd = cmd.Substring(cmd.IndexOf(";") + 1);
        }
        #endregion
    }
}
