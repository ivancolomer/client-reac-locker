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

        public static void Initialize()
        {
            Processes = new GenericProcess[((PROCESS[])Enum.GetValues(typeof(PROCESS))).Length];
            Lockers = new object[((PROCESS[])Enum.GetValues(typeof(PROCESS))).Length];
            for (int i = 0; i < Lockers.Length; i++)
                Lockers[i] = new object();
        }

        public static void StartProcess(PROCESS process)
        {
            lock (Lockers[(int)process])
            {
                GenericProcess currentProcess = Processes[(int)process];
                if (currentProcess != null && !currentProcess.hasExited)
                    return;

                if (process == PROCESS.LOCKING_DEVICE && Program.IPAddressServer != null)
                {
                    Processes[(int)process] = new LockerProcess(Program.IPAddressServer.ToString());
                }
                else if (process == PROCESS.VIDEO_STREAMING && Program.IPAddressServer != null)
                {
                    //Processes[(int)process] = new VideoStreamerProcess(Program.IPAddressServer.ToString());
                }
            }
        }

        public static bool WriteLineToStandardInput(PROCESS process, string line)
        {
            try
            {
                GenericProcess currentProcess = Processes[(int)process];
                if (currentProcess != null && !currentProcess.hasExited)
                {
                    return currentProcess.SendLine(line);
                }
            }
            catch(Exception e)
            {

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
