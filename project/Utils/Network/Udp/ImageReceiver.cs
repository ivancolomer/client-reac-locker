using REAC_LockerDevice.Utils.Network;
using REAC_LockerDevice.Utils.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace REAC_LockerDevice.Utils.Network.Udp
{
    class ImageReceiver
    {
        public bool stopListeningToBroadcast;
        private UdpClient udpClient;

        public ImageReceiver()
        {
            stopListeningToBroadcast = false;

            udpClient = new UdpClient(DotNetEnv.Env.GetInt("UDP_IMAGE_LISTENER_PORT"));
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            Logger.WriteLine("ImageReceiver listening on port: " + DotNetEnv.Env.GetInt("UDP_IMAGE_LISTENER_PORT"), Logger.LOG_LEVEL.DEBUG);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint from = new IPEndPoint(0, 0);
                byte[] receiveBytes = udpClient.EndReceive(ar, ref from);
                string ipReceiver = from.ToString().Split(':')[0];

                if (ipReceiver != "127.0.0.1")
                    return;

                string receiveString = Encoding.ASCII.GetString(receiveBytes);
                //Logger.WriteLineWithHeader(receiveString, "ImageReceiver", Logger.LOG_LEVEL.DEBUG);

                int separatorIndex = receiveString.IndexOf('|');
                if (separatorIndex == -1)
                    return;

                int maxLength = int.Parse(receiveString.Substring(separatorIndex + 1));
                MemoryStream imageBytes = new MemoryStream(maxLength);
                
                int totalRead = 0;
                while (totalRead < maxLength)
                {
                    receiveBytes = udpClient.Receive(ref from);
                    if (from.ToString().Split(':')[0] != "127.0.0.1")
                        continue;

                    udpClient.Send(new byte[] { (byte)1 }, 1, from);

                    int toRead = Math.Min(receiveBytes.Length, maxLength - totalRead);
                    imageBytes.Write(receiveBytes, 0, toRead);
                    totalRead += toRead;
                }

                if (Program.Client != null)
                {
                    Program.Client.imageToSend = imageBytes.ToArray();
                    //Logger.WriteLineWithHeader("finished reading " + Program.Client.imageToSend.Length, "LOCKER_DEVICE_PROGRAM", Logger.LOG_LEVEL.DEBUG);
                }

                if (!stopListeningToBroadcast)
                {
                    udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
                }
                else
                {
                    try
                    {
                        udpClient.Close();
                        udpClient.Dispose();
                    }
                    catch(Exception)
                    {

                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                Logger.WriteLine("Exception while ReceivedCallback from BroadcastReceiver: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
        }

        public void Stop()
        {
            if(udpClient != null)
            {
                try
                {
                    udpClient.Close();
                    udpClient.Dispose();
                    udpClient = null;
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
