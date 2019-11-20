using System;
using System.Net;
using System.IO;
using System.Threading;

using REAC_LockerDevice.Utils.Output;
using REAC_LockerDevice.Utils.Network;
using REAC_LockerDevice.Utils.Network.Udp;
using REAC_LockerDevice.Utils.Network.Tcp;

namespace REAC_LockerDevice
{
    //sudo apt-get update
    //sudo apt-get install curl libunwind8 gettext apt-transport-https
    //sudo chmod 755 ./REAC_LockerDevice

    //dotnet publish --self-contained --runtime linux-arm


    public class Program
    {
        public static DateTime InitStartUTC { get; set; }

        public static IPAddress IPAddressServer = null;
        public static AsynchronousClient Client = null;

        private static bool HasExited = false;

        static void Main(string[] args)
        {
            InitStartUTC = DateTime.UtcNow;

            DotNetEnv.Env.Load(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".env");
            Logger.Initialize();

            Console.CancelKeyPress += delegate
            {
                ExitProgram();
            };

            AppDomain.CurrentDomain.ProcessExit += delegate
            {
                ExitProgram();
            };

            BroadcastReceiver broadcastReceiver = new BroadcastReceiver(); //FOR TESTING ONLY

            while(IPAddressServer == null)
            {
                Logger.WriteLine("Waiting to get the IP Addres of the server...", Logger.LOG_LEVEL.DEBUG);
                Thread.Sleep(500);
            }

            Logger.WriteLine("IP Address found: " + IPAddressServer.ToString(), Logger.LOG_LEVEL.DEBUG);

            while(true)
            {
                Client = new AsynchronousClient(IPAddressServer);
                Thread.Sleep(500);
                while (!Client.hasClosed)
                {
                    Thread.Sleep(500);
                }
            }
            
        }

        private static void ExitProgram()
        {
            if (HasExited)
                return;

            HasExited = true;

            CloseSocket();

            Logger.WriteLine("Stopped. Good bye!", Logger.LOG_LEVEL.DEBUG);
            Environment.Exit(0);
        }

        private static void CloseSocket()
        {
            try
            {
                //something
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error closing socket: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }
        }
    }
}
