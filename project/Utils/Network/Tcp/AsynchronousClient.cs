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

namespace REAC_LockerDevice.Utils.Network.Tcp
{
    public class AsynchronousClient
    {
        private const int BUFFER_LENGTH = 4096;

        private Socket Client;
        private byte[] Buffer;
        public bool hasClosed = false;

        public AsynchronousClient(IPAddress ipAddress)
        {
            this.Buffer = new byte[BUFFER_LENGTH];

            ProcessManager.SetIpAddress(ipAddress.ToString(), DotNetEnv.Env.GetInt("UDP_VIDEO_STREAM_PORT"));

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

                try
                {
                    //Handle Packet
                    string receiveString = Encoding.UTF8.GetString(Buffer);

                    if (receiveString.StartsWith("start_video_stream"))
                    {
                        Logger.WriteLine("START VIDEO STREAM PROCESS", Logger.LOG_LEVEL.DEBUG);
                        StartProcess();
                    }
                    else if(receiveString.StartsWith("stop_video_stream"))
                    {
                        Logger.WriteLine("STOP VIDEO STREAM PROCESS", Logger.LOG_LEVEL.DEBUG);
                        StopProcess();
                    }
                    else if(receiveString.StartsWith("open_door"))
                    {
                        //send to locker process a line string "open"
                        ProcessManager.WriteLineToStandardInput(ProcessManager.PROCESS.LOCKING_DEVICE, "open");
                    }
                }
                catch (Exception)
                {

                }

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

        private void Send(string message)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(message);
            Client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), null);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            { 
                int bytesSent = Client.EndSend(ar);
            }
            catch (SocketException)
            {
                CloseSocket();
            }
            catch (ObjectDisposedException)
            {

            }
        }
    }
}