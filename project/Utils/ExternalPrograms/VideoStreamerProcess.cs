﻿using REAC_LockerDevice.Utils.Output;
using System;
using System.Collections.Generic;
using System.Text;

namespace REAC_LockerDevice.Utils.ExternalPrograms
{
    public class VideoStreamerProcess : GenericProcess
    {
        private const string PATH_TO_VIDEO_PROCESS = "raspivid -n -ih -w 320 -h 240 -fps 12 -t 0 -o udp://";//"ffmpeg -hide_banner -loglevel panic -re -loop 1 -i /home/reac/Project/cmake-build-debug/frame.jpg -r 10 -vcodec mpeg4 -f mpegts udp://";//"raspivid -n -ih -w 320 -h 240 -fps 3 -t 0 -o udp://"; //-vf -hf

        public VideoStreamerProcess(string ipAddress, int port)
            : base(PATH_TO_VIDEO_PROCESS + ipAddress + ":" + port.ToString(), false, false)
        {

        }

        public override void OnReceivedLine(string line)
        {
        }
    }
}
