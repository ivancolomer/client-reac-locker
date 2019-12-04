using REAC_LockerDevice.Utils.Output;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_LockerDevice.Utils.ExternalPrograms
{
    public class LockerProcess : GenericProcess
    {
        public const string PATH_TO_LOCKER_PROGRAM = "/home/reac/Project/cmake-build-debug-raspberrypi/Project";
        public LockerProcess()
            :base(PATH_TO_LOCKER_PROGRAM, true, true)
        {

        }

        public override void OnReceivedLine(string line)
        {
            Logger.WriteLineWithHeader(line, "LOCKER_DEVICE_PROGRAM", Logger.LOG_LEVEL.DEBUG);
        }
    }
}
