using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace REAC_LockerDevice.Utils.ExternalPrograms
{
    public class ProcessManager
    {
        public enum PROCESS {
            VIDEO_STREAMING = 0,
            LOCKING_DEVICE = 1
        }

        private static GenericProcess[] Processes = null;
        private static object[] Lockers = null;

        private static string IPAddress = null;
        private static int Port = 0;

        public static void Initialize()
        {
            Processes = new GenericProcess[((PROCESS[])Enum.GetValues(typeof(PROCESS))).Length];
            Lockers = new object[((PROCESS[])Enum.GetValues(typeof(PROCESS))).Length];
            for (int i = 0; i < Lockers.Length; i++)
                Lockers[i] = new object();
        }

        public static void SetIpAddress(string ipAddress, int port)
        {
            IPAddress = ipAddress;
            Port = port;
        }

        public static void StartProcess(PROCESS process)
        {
            lock (Lockers[(int)process])
            {
                GenericProcess currentProcess = Processes[(int)process];
                if (currentProcess != null && !currentProcess.hasExited)
                    return;

                if (process == PROCESS.LOCKING_DEVICE)
                {
                    Processes[(int)process] = new LockerProcess();
                }
                else if (process == PROCESS.VIDEO_STREAMING && IPAddress != null && Port != 0)
                {
                    Processes[(int)process] = new VideoStreamerProcess(IPAddress, Port);
                }
            }
        }

        public static bool WriteLineToStandardInput(PROCESS process, string line)
        {
            GenericProcess currentProcess = Processes[(int)process];
            if (currentProcess != null && !currentProcess.hasExited)
            {
                return currentProcess.SendLine(line);
            }
            return false;
        }

        public static void StopProcess(PROCESS process)
        {
            lock (Lockers[(int)process])
            {
                GenericProcess currentProcess = Processes[(int)process];
                if (currentProcess != null)
                    currentProcess.Stop();

                Processes[(int)process] = null;
            }
        }

        public static void StopAllProcesses()
        {
            foreach(PROCESS process in ((PROCESS[])Enum.GetValues(typeof(PROCESS))))
            {
                StopProcess(process);
            }
        }
    }
}
