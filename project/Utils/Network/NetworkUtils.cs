using REAC_LockerDevice.Utils.Output;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace REAC_LockerDevice.Utils.Network
{
    class NetworkUtils
    {
        private readonly static bool USE_EXTERNAL_IP = false;

        private static string ExternalIPAddress = null;

        public static string GetExternalIPAddress()
        {
            if (ExternalIPAddress == null)
            {
                try
                {
                    if (USE_EXTERNAL_IP)
                        ExternalIPAddress = new WebClient().DownloadString("https://api.ipify.org");
                    else
                    {
                        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                        {
                            socket.Connect("8.8.8.8", 65530);
                            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                            ExternalIPAddress = endPoint.Address.ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteLine("Errror while getting IPAddress: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
                }
            }

            return ExternalIPAddress;

            /*string localIP = null;
            try
            {

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }

            }
            catch (Exception e)
            {
                Logger.WriteLine("Errror while getting IPAddress: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
            return localIP;*/
        }
    }
}
