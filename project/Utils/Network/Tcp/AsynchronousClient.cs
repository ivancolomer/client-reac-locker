using System;  
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
        private ManualResetEvent sendDone;

        public byte[] imageToSend = null;

        public AsynchronousClient(IPAddress ipAddress)
        {
            this.Buffer = new byte[BUFFER_LENGTH];
            sendDone = new ManualResetEvent(false);

            //ProcessManager.SetIpAddress(ipAddress.ToString(), DotNetEnv.Env.GetInt("UDP_VIDEO_STREAM_PORT"));

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
                    int separatorIndex = receiveString.IndexOf('|');
                    if (separatorIndex == -1)
                        return;

                    long packetId = long.Parse(receiveString.Substring(0, separatorIndex));
                    string message = receiveString.Substring(separatorIndex + 1);
                    //Logger.WriteLine(packetId + "|" + message + "...", Logger.LOG_LEVEL.DEBUG);
                    if (message.StartsWith("get_live_image"))
                    {
                        byte[] image = imageToSend;
                        if (image == null)
                        {
                            Send(packetId + "|send_image|0|");
                            //Logger.WriteLineWithHeader(packetId + "|send_image|0|", "image", Logger.LOG_LEVEL.DEBUG);
                        }
                        else
                        {
                            Send(packetId + "|send_image|" + image.Length + "|");
                            Logger.WriteLineWithHeader(packetId + "|send_image|" + image.Length + "|", "image", Logger.LOG_LEVEL.DEBUG);
                            Send(image);
                        }
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
                    else if(message.StartsWith("open_door"))
                    {
                        //send to locker process a line string "open"
                        if (ProcessManager.WriteLineToStandardInput(ProcessManager.PROCESS.LOCKING_DEVICE, "open")) 
                        {
                            Logger.WriteLine("'open' String has been sent to locker device", Logger.LOG_LEVEL.DEBUG);
                            Send(packetId + "|door_opened");
                        }
                        else
                        {
                            Logger.WriteLine("'open' String couldn't been send been sent to locker device", Logger.LOG_LEVEL.DEBUG);
                            Send(packetId + "|door_not_opened");
                        }

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
                ProcessManager.StartProcess(ProcessManager.PROCESS.VIDEO_STREAMING, Program.IPAddressServer.ToString(), DotNetEnv.Env.GetInt("UDP_VIDEO_STREAM_PORT"));
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
    }
}