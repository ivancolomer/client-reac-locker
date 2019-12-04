using REAC_LockerDevice.Utils.Output;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace REAC_LockerDevice.Utils.ExternalPrograms
{
    public abstract class GenericProcess
    {
        private ProcessStartInfo StartInfo;
        private Process Process;
        public bool hasExited = false;

        private BlockingCollection<string> InputQueue;
        private BlockingCollection<string> OutputQueue;

        public GenericProcess(string arguments, bool redirectStandardInput, bool redirectStandardOutput)
        {
            this.StartInfo = new ProcessStartInfo();
            
            this.StartInfo.FileName = "/bin/bash";
            this.StartInfo.Arguments = "-c \"" + arguments + "\"";
            this.StartInfo.RedirectStandardOutput = redirectStandardOutput;
            this.StartInfo.RedirectStandardInput = redirectStandardInput;
            this.StartInfo.RedirectStandardError = false;
            this.StartInfo.CreateNoWindow = false;
            this.StartInfo.UseShellExecute = false;
            this.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            try { Process = Process.Start(StartInfo); }
            catch(Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.ERROR);
            }

            Task.Run(() => ProcessChecker());

            if (this.StartInfo.RedirectStandardInput)
            {
                InputQueue = new BlockingCollection<string>(100);
                Task.Run(() => InputQueueChecker());
            }

            if (this.StartInfo.RedirectStandardOutput)
            {
                OutputQueue = new BlockingCollection<string>(100);
                Task.Run(() => OutputQueueChecker());
            }
        }

        private async void InputQueueChecker()
        {
            if (InputQueue == null)
                return;

            while (!hasExited && Process != null && !Process.HasExited)
            {
                string packet = null;
                try
                {
                    packet = InputQueue.Take();
                }
                catch (Exception)
                {
                }

                if (packet == null)
                {
                    await Task.Delay(100);
                }
                else
                {
                    try
                    {
                        if(Process != null && !Process.HasExited)
                            Process.StandardInput.WriteLine(packet);
                    }
                    catch(Exception)
                    {

                    }
                }
            }

            Logger.WriteLine("InputQueueChecker exited", Logger.LOG_LEVEL.DEBUG);
        }

        private async void OutputQueueChecker()
        {
            if (OutputQueue == null)
                return;

            while (!hasExited && Process != null && !Process.HasExited)
            {
                try
                {
                    OnReceivedLine(Process.StandardOutput.ReadLine());
                }
                catch (Exception)
                {
                    await Task.Delay(10);
                }
            }
            Logger.WriteLine("OutputQueueChecker exited", Logger.LOG_LEVEL.DEBUG);
        }

        private void ProcessChecker()
        {
            try
            {
                if (Process != null)
                    Process.WaitForExit();
            }
            catch(Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.ERROR);
            }

            hasExited = true;
            Logger.WriteLine("ProcessChecker exited", Logger.LOG_LEVEL.DEBUG);
        }

        public void SendLine(string line)
        {
            if(InputQueue != null)
                InputQueue.TryAdd(line);
        }

        public abstract void OnReceivedLine(string line);

        public void Stop()
        {

            if (Process != null)
            {
                try
                {
                    Process.Kill();
                }
                catch (Exception)
                {

                }

                try
                {
                    Process.Close();
                }
                catch (Exception)
                {

                }

                SendLine(null);
            }
        }
    }
}
