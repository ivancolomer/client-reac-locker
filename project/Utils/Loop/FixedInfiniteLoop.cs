using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REAC_LockerDevice.Utils.Loop
{
    public class FixedInfiniteLoop : InfiniteLoop
    {
        private long TimeBefore = 0;
        private int TimeLaggedStepBefore = 0;

        public FixedInfiniteLoop(int SleepTime, OnTickCallback Callback)
            : base(SleepTime, Callback)
        {
        }

        protected override void BeforeAction()
        {
            TimeBefore = Time.GetTime();
        }

        protected override int TimeToWait()
        {
            int TimeToSleep = base.TimeToWait() - TimeLaggedStepBefore - (int)(Time.GetTime() - TimeBefore);

            if (TimeToSleep < 0)
            {
                TimeLaggedStepBefore = -TimeToSleep;
                return 0;
            }

            TimeLaggedStepBefore = 0;
            return TimeToSleep;
        }
    }
}
