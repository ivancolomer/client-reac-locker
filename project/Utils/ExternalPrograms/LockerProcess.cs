using REAC_LockerDevice.Utils.Output;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_LockerDevice.Utils.ExternalPrograms
{
    public class LockerProcess : GenericProcess
    {
        public const string PATH_TO_LOCKER_DIR = "/home/reac/Project/cmake-build-debug-raspberrypi/";
        public const string PATH_TO_LOCKER_PROGRAM = PATH_TO_LOCKER_DIR + "Project";

        public LockerProcess(string ipAddress, int port)
            :base(PATH_TO_LOCKER_PROGRAM + " " + ipAddress + ":" + port.ToString(), true, true, PATH_TO_LOCKER_DIR)
        {

        }

        public override void OnReceivedLine(string line)
        {
            Logger.WriteLineWithHeader(line, "LOCKER_DEVICE_PROGRAM", Logger.LOG_LEVEL.DEBUG);
            if(line == "image")
            {
                int separatorIndex = line.IndexOf('|');
                if (separatorIndex == -1)
                    return;

                char[] imageChars = new char[int.Parse(line.Substring(separatorIndex + 1))];
                Process.StandardOutput.ReadBlock(imageChars, 0, imageChars.Length);
                try
                {
                    Program.Client.imageToSend = Encoding.ASCII.GetBytes(imageChars);
                }
                catch(Exception)
                {

                }
            }
        }
    }
}
