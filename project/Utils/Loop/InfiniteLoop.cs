using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REAC_LockerDevice.Utils.Loop
{
    public delegate void OnTickCallback();

    public class InfiniteLoop : IDisposable
    {
        private readonly int SleepTime;
        private OnTickCallback Callback;
        private bool Running;

        public InfiniteLoop(int SleepTime, OnTickCallback Callback)
        {
            this.SleepTime = SleepTime;
            this.Callback = Callback;
            this.Running = true;

            StartLoop();
        }

        protected async void StartLoop()
        {
            while (Running)
            {
                BeforeAction();

                OnTickCallback CallbackCpy = Callback;
                if (CallbackCpy != null)
                    CallbackCpy.Invoke();

                await Task.Delay(TimeToWait());
            }
            Dispose();
        }

        protected virtual void BeforeAction()
        {
        }

        protected virtual int TimeToWait()
        {
            return SleepTime;
        }

        public void Stop()
        {
            this.Running = false;
        }

        public void Dispose()
        {
            Callback = null;
        }
    }
}
