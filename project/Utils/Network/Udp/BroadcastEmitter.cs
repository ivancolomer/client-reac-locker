using REAC_LockerDevice.Utils.Loop;
using REAC_LockerDevice.Utils.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace REAC_LockerDevice.Utils.Network.Udp
{
    public class BroadcastEmitter
    {
        private const int LOOP_MILLS = 3 * 1000; // 3 seconds time

        private InfiniteLoop broadcastLoop;

        private UdpClient udpClient;
        private IPEndPoint udpAddress;

        public BroadcastEmitter()
        {
            /*udpAddress = new IPEndPoint(GetLocalBroadcastAddress(), DotNetEnv.Env.GetInt("UDP_LISTENER_PORT"));//new IPEndPoint(GetLocalBroadcastAddress(), DotNetEnv.Env.GetInt("UDP_LISTENER_PORT"));
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            udpClient.ExclusiveAddressUse = false; // only if you want to send/receive on same machine.
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, DotNetEnv.Env.GetInt("UDP_EMITER_PORT")));*/

            udpAddress = new IPEndPoint(IPAddress.Broadcast, DotNetEnv.Env.GetInt("UDP_LISTENER_PORT"));
            udpClient = new UdpClient();
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            udpClient.EnableBroadcast = true;

            

            broadcastLoop = new InfiniteLoop(LOOP_MILLS, new OnTickCallback(SendBroadcastMessage));
        }

        public void Stop()
        {
            try
            {
                broadcastLoop.Stop();
                broadcastLoop = null;

                udpClient.Close();
                udpClient.Dispose();
                udpClient = null;

                udpAddress = null;
            }
            catch(Exception e)
            {
                Logger.WriteLine("Exception while Stop from BroadcastEmiter: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
        }

        private void SendBroadcastMessage()
        {
            try
            {
                if(udpClient != null && udpAddress != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes("Something");
                    //Logger.WriteLine("Sending message to " + udpAddress.ToString(), Logger.LOG_LEVEL.DEBUG);
                    udpClient.Send(bytes, bytes.Length, udpAddress);
                    //Logger.WriteLine("Broadcast message sent to " + udpAddress.ToString(), Logger.LOG_LEVEL.DEBUG);
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
                Logger.WriteLine("Exception while SendBroadcastMessage from BroadcastEmiter: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
        }

        /*private static IPAddress GetLocalBroadcastAddress()
        {
            IPAddress result;
            try
            {
                if ((result = GetBroadcastAddressFromNetworkInterfaceType(NetworkInterfaceType.Ethernet)) != null)
                {
                    return result;
                }
                if ((result = GetBroadcastAddressFromNetworkInterfaceType(NetworkInterfaceType.Wireless80211)) != null)
                {
                    return result;
                }

                if ((result = GetBroadcastAddressFromNetworkInterfaceType(NetworkInterfaceType.GigabitEthernet)) != null)
                {
                    return result;
                }
                if ((result = GetBroadcastAddressFromNetworkInterfaceType(NetworkInterfaceType.FastEthernetT)) != null)
                {
                    return result;
                }
                if ((result = GetBroadcastAddressFromNetworkInterfaceType(NetworkInterfaceType.FastEthernetFx)) != null)
                {
                    return result;
                }
                if ((result = GetBroadcastAddressFromNetworkInterfaceType(NetworkInterfaceType.Ethernet3Megabit)) != null)
                {
                    return result;
                }
            }
            catch(Exception e)
            {
                Logger.WriteLine("Exception whileGetLocalBroadcastAddress from BroadcastEmiter: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }

            return IPAddress.Broadcast;
        }

        private static IPAddress GetBroadcastAddressFromNetworkInterfaceType(NetworkInterfaceType _type)
        {
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties adapterProperties = item.GetIPProperties();

                    if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                    {
                        foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                return ParseBroadcastAddress(ip.Address, ip.IPv4Mask);
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static IPAddress ParseBroadcastAddress(IPAddress address, IPAddress mask)
        {
            uint ipAddress = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
            uint ipMaskV4 = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
            uint broadCastIpAddress = ipAddress | ~ipMaskV4;

            return new IPAddress(BitConverter.GetBytes(broadCastIpAddress));
        }*/
    }
}
