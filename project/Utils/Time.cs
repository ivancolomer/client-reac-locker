using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REAC_LockerDevice.Utils
{
    public class Time
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); //Utc

        public static long GetTime()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds; //UtcNow
        }

        public static long GetTimeDate(DateTime n)
        {
            return (long)(n - Jan1st1970).TotalMilliseconds;
        }

        public static DateTime GetDateTime(long time)
        {
            return Jan1st1970.AddMilliseconds(time);
        }
    }
}
