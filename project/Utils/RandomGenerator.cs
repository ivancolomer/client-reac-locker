using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REAC_LockerDevice.Utils
{
    public class RandomGenerator
    {
        private static Random random = new Random();
        private static ushort times = 0;
        private static object lockrand = new object();

        public static int Next(int minValue, int maxValue)
        {
            lock (lockrand)
            {
                if (times >= UInt16.MaxValue)
                {
                    times = 0;
                    random = new Random();
                }
                times++;
                return random.Next(minValue, maxValue);
            }
        }

        public static double NextDoubleAlone()
        {
            lock (lockrand)
            {
                if (times >= UInt16.MaxValue)
                {
                    times = 0;
                    random = new Random();
                }
                times++;
                return random.NextDouble();
            }
        }

        public static int NextAlone(int Value)
        {
            lock (lockrand)
            {
                if (times >= UInt16.MaxValue)
                {
                    times = 0;
                    random = new Random();
                }
                times++;
                return random.Next(Value);
            }
        }

        public static double NextDouble(double minValue, double maxValue)
        {
            lock (lockrand)
            {
                if (times == UInt16.MaxValue)
                {
                    times = 0;
                    random = new Random();
                }
                times++;
                return random.NextDouble() * (maxValue - minValue) + minValue;
            }
        }

        public static byte[] GenerateRandomBytes(int bytesLength)
        {
            byte[] b = new byte[bytesLength];
            random.NextBytes(b);

            return b;
        }
    }
}
