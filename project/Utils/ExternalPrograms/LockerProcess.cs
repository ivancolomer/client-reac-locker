using REAC_LockerDevice.Utils.Output;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_LockerDevice.Utils.ExternalPrograms
{
    public class LockerProcess : GenericProcess
    {
        public LockerProcess(string ipAddress)
            :base(DotNetEnv.Env.GetString("LOCKER_DIR_PATH") + DotNetEnv.Env.GetString("LOCKER_APP_NAME") + " " + ipAddress + ":" + DotNetEnv.Env.GetString("UDP_IMAGE_LISTENER_PORT"), true, true, DotNetEnv.Env.GetString("LOCKER_DIR_PATH"))
        {

        }

        public override void OnReceivedLine(string line)
        {
            Logger.WriteLineWithHeader(line, "LOCKER_DEVICE_PROGRAM", Logger.LOG_LEVEL.DEBUG);

            if(line.StartsWith("added_user|"))
            {
                try
                {
                    int separatorIndex = line.IndexOf('|');

                    Program.Client.Send("door_opened_by|" + line.Substring(separatorIndex + 1) + "|");
                }
                catch(Exception)
                {

                }
            }

            //if (line.StartsWith("image|"))
            //{
                /*Logger.WriteLineWithHeader(line, "LOCKER_DEVICE_PROGRAM", Logger.LOG_LEVEL.DEBUG);
                int separatorIndex = line.IndexOf('|');
                if (separatorIndex == -1)
                    return;

                char[] imageChars = new char[int.Parse(line.Substring(separatorIndex + 1))];

                for(int i = 0; i < imageChars.Length; i++)
                {
                    imageChars[i] = (char)Process.StandardOutput.Read();
                    if(i > imageChars.Length - 10000 && i % 10 == 0)
                        Logger.WriteLineWithHeader(i.ToString(), "LOCKER_DEVICE_PROGRAM", Logger.LOG_LEVEL.DEBUG);
                }
                Logger.WriteLineWithHeader("finished", "LOCKER_DEVICE_PROGRAM", Logger.LOG_LEVEL.DEBUG);

                //int totalRead = Process.StandardOutput.Read(imageChars, 0, imageChars.Length);
                //Logger.WriteLineWithHeader(totalRead.ToString(), "LOCKER_DEVICE_PROGRAM", Logger.LOG_LEVEL.DEBUG);
                //while ( totalRead < imageChars.Length)
                //{
                // totalRead += Process.StandardOutput.Read(imageChars, totalRead, imageChars.Length - totalRead);
                //Logger.WriteLineWithHeader(totalRead.ToString(), "LOCKER_DEVICE_PROGRAM", Logger.LOG_LEVEL.DEBUG);
                //}


                try
                {
                    Program.Client.imageToSend = Encoding.ASCII.GetBytes(imageChars);
                }
                catch(Exception)
                {

                }*/
            //}
        }
    }
}
