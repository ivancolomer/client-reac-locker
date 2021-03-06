﻿using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Threading;  
using System.Text;
using System.Runtime.InteropServices;
using REAC_LockerDevice.Utils.Output;
using System.Diagnostics;
using System.IO;
using REAC_LockerDevice.Utils.ExternalPrograms;
using System.Linq;

namespace REAC_LockerDevice.Utils.Network.Tcp
{
    public class AsynchronousClient
    {
        private const int BUFFER_LENGTH = 4096;

        private Socket Client;
        private byte[] Buffer;
        public bool hasClosed = false;
        private ManualResetEvent sendDone;

        public byte[] imageToSend = null;

        public AsynchronousClient(IPAddress ipAddress)
        {
            this.Buffer = new byte[BUFFER_LENGTH];
            sendDone = new ManualResetEvent(false);

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, DotNetEnv.Env.GetInt("TCP_LOCKER_LISTENER_PORT"));
            this.Client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            this.Client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), null);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                this.Client.EndConnect(ar);
                //socket connected
                this.Receive();
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.ERROR);
                CloseSocket();
            }
        }

        private void Receive()
        {
            try
            {
                // Begin receiving the data from the remote device.  
                this.Client.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch (Exception)
            {
            }
        }

        public void CloseSocket()
        {
            if (!hasClosed)
            {
                hasClosed = true;
                try
                {
                    this.Client.Close();
                }
                catch (Exception)
                {

                }
                try
                {
                    this.Client.Dispose();
                }
                catch (Exception)
                {

                }
                StopProcess();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            { 
                int bytesRead = this.Client.EndReceive(ar);

                if(bytesRead == 0)
                {
                    CloseSocket();
                    return;
                }

                HandlePacket();
                this.Receive();
            }
            catch (SocketException)
            {
                CloseSocket();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
            }
        }

        private void HandlePacket()
        {
            try
            {
                //Handle Packet
                string receiveString = Encoding.UTF8.GetString(Buffer);

                string message;
                string value = getStringFirstValue(receiveString, out message);
                if (value == null)
                    return;

                long packetId = long.Parse(value);

                if (message.StartsWith("get_live_image"))
                {
                    byte[] image = imageToSend;
                    if (image == null)
                    {
                        //Logger.WriteLineWithHeader("Image = null", "LIVE_IMAGE", Logger.LOG_LEVEL.ERROR);
                        Send(packetId + "|error|");
                        return;
                    }

                    //Logger.WriteLineWithHeader("Image sent", "LIVE_IMAGE", Logger.LOG_LEVEL.ERROR);
                    Send(packetId + "|send_image|" + image.Length + "|");
                    Send(image);
                }
                else if (message.StartsWith("get_photo_list"))
                {
                    value = getStringFirstValue(message, out message);
                    if (value == null)
                    {
                        Send(packetId + "|error|");
                        return;
                    }


                    value = getStringFirstValue(message, out message);
                    if (value == null)
                    {
                        Send(packetId + "|error|");
                        return;
                    }

                    //uint userId = uint.Parse(value);

                    if (!Directory.Exists(DotNetEnv.Env.GetString("LOCKER_PHOTO_DIR_PATH") + value + Path.DirectorySeparatorChar))
                    {
                        Send(packetId + "|error|");
                        return;
                    }

                    string[] files = Directory.GetFiles(DotNetEnv.Env.GetString("LOCKER_PHOTO_DIR_PATH") + value + Path.DirectorySeparatorChar);

                    Send(packetId + "|" + String.Join("|", files.Select(Path.GetFileName).Select(word => word.Substring(0, word.Length - 4))) + "|");

                }
                else if (message.StartsWith("get_photo_user"))
                {
                    value = getStringFirstValue(message, out message);
                    if (value == null)
                    {
                        Send(packetId + "|error|");
                        return;
                    }

                    string dirname = getStringFirstValue(message, out message);
                    if (dirname == null)
                    {
                        Send(packetId + "|error|");
                        return;
                    }

                    string photoId = getStringFirstValue(message, out message);
                    if (photoId == null)
                    {
                        Send(packetId + "|error|");
                        return;
                    }

                    try
                    {
                        byte[] image = File.ReadAllBytes(DotNetEnv.Env.GetString("LOCKER_PHOTO_DIR_PATH") + dirname + Path.DirectorySeparatorChar + photoId + ".png");
                        Send(packetId + "|send_image|" + image.Length + "|");
                        Send(image);
                        //Logger.WriteLineWithHeader("image sent", "IMAGE_FACE", Logger.LOG_LEVEL.DEBUG);
                        return;
                    }
                    catch (Exception)
                    {
                        //Logger.WriteLineWithHeader(e.ToString(), "IMAGE_FACE", Logger.LOG_LEVEL.DEBUG);
                    }
                    Send(packetId + "|error|");
                }
                else if (message.StartsWith("create_user"))
                {
                    value = getStringFirstValue(message, out message);
                    if (value == null)
                    {
                        Send(packetId + "|error|");
                        return;
                    }

                    uint userId = 0;
                    if (uint.TryParse(getStringFirstValue(message, out message), out userId) && ProcessManager.WriteLineToStandardInput(ProcessManager.PROCESS.LOCKING_DEVICE, "add_" + userId))
                    {
                        Send(packetId + "|creating_user|");
                    }

                    Send(packetId + "|error|");
                    return;
                }
                else if (message.StartsWith("start_video_stream"))
                {
                    //Logger.WriteLine("START VIDEO STREAM PROCESS", Logger.LOG_LEVEL.DEBUG);
                    StartProcess();
                    Send(packetId + "|video_stream_started");
                }
                else if (message.StartsWith("stop_video_stream"))
                {
                    //Logger.WriteLine("STOP VIDEO STREAM PROCESS", Logger.LOG_LEVEL.DEBUG);
                    StopProcess();
                    Send(packetId + "|video_stream_stopped");
                }
                else if (message.StartsWith("open_door"))
                {
                    //send to locker process a line string "open"
                    if (ProcessManager.WriteLineToStandardInput(ProcessManager.PROCESS.LOCKING_DEVICE, "open"))
                    {
                        Send(packetId + "|door_opened");
                    }
                    else
                    {
                        //Logger.WriteLine("'open' String couldn't been send been sent to locker device", Logger.LOG_LEVEL.DEBUG);
                        Send(packetId + "|door_not_opened");
                    }

                }
                else if (message.StartsWith("reset"))
                {
                    //send to locker process a line string "open"
                    if (ProcessManager.WriteLineToStandardInput(ProcessManager.PROCESS.LOCKING_DEVICE, "deleteall"))
                    {
                        Send(packetId + "|wiped_all_data");
                    }
                    else
                    {
                        //Logger.WriteLine("'open' String couldn't been send been sent to locker device", Logger.LOG_LEVEL.DEBUG);
                        Send(packetId + "|data_could_not_be_wiped");
                    }

                }
            }
            catch (Exception)
            {

            }
        }

        private void StartProcess()
        {
            try
            {
                ProcessManager.StartProcess(ProcessManager.PROCESS.VIDEO_STREAMING);
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
        }

        private void StopProcess()
        {
            try
            {
                ProcessManager.StopProcess(ProcessManager.PROCESS.VIDEO_STREAMING);
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
        }

        public void Send(string message)
        {
            Send(Encoding.UTF8.GetBytes(message));
        }

        byte[] byteDataToBeSent;
        int bytesSent = 0;

        private void Send(byte[] byteData)
        {
            byteDataToBeSent = byteData;
            bytesSent = 0;

            Client.BeginSend(byteDataToBeSent, 0, byteDataToBeSent.Length, 0, new AsyncCallback(SendCallback), null);
            sendDone.WaitOne();
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            { 
                bytesSent += Client.EndSend(ar);

                if (bytesSent >= byteDataToBeSent.Length)
                    sendDone.Set();
                else
                    Client.BeginSend(byteDataToBeSent, bytesSent, byteDataToBeSent.Length - bytesSent, 0, new AsyncCallback(SendCallback), null);
            }
            catch (SocketException)
            {
                CloseSocket();
            }
            catch (ObjectDisposedException)
            {

            }
            catch(Exception)
            {

            }
        }

        public static string getStringFirstValue(string message, out string substring)
        {
            int separatorIndex = message.IndexOf('|');
            if (separatorIndex == -1)
            {
                substring = null;
                return null;
            }

            substring = message.Substring(separatorIndex + 1);
            return message.Substring(0, separatorIndex);
        }
    }
}