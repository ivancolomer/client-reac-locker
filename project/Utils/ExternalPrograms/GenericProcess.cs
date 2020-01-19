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
        public Process Process;
        public bool hasExited = false;

        private BlockingCollection<string> InputQueue;

        public GenericProcess(string arguments, bool redirectStandardInput, bool redirectStandardOutput, string WorkingDirectory)
        {
            this.StartInfo = new ProcessStartInfo();
            
            this.StartInfo.FileName = "/bin/bash";
            this.StartInfo.Arguments = "-c \"" + arguments + "\"";
            this.StartInfo.RedirectStandardOutput = redirectStandardOutput;
            this.StartInfo.RedirectStandardInput = redirectStandardInput;
            this.StartInfo.RedirectStandardError = true;
            this.StartInfo.CreateNoWindow = false;
            this.StartInfo.UseShellExecute = false;
            this.StartInfo.WorkingDirectory = WorkingDirectory;

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
                Task.Run(() => OutputQueueChecker());
            }

            Task.Run(() => OutputErrorQueueChecker());
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

            //Logger.WriteLine("InputQueueChecker exited", Logger.LOG_LEVEL.DEBUG);
            try
            {
                Process.StandardInput.Close();
            }
            catch (Exception)
            {

            }
            try
            {
                Process.StandardInput.Dispose();
            }
            catch (Exception)
            {

            }
        }

        private async void OutputQueueChecker()
        {

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
            //Logger.WriteLine("OutputQueueChecker exited", Logger.LOG_LEVEL.DEBUG);
            try
            {
                Process.StandardOutput.Close();
            }
            catch (Exception)
            {

            }
            try
            {
                Process.StandardOutput.Dispose();
            }
            catch (Exception)
            {

            }
        }

        private async void OutputErrorQueueChecker()
        {

            while (!hasExited && Process != null && !Process.HasExited)
            {
                try
                {
                    OnReceivedErrorLine(Process.StandardError.ReadLine());
                }
                catch (Exception)
                {
                    await Task.Delay(10);
                }
            }
            //Logger.WriteLine("OutputQueueChecker exited", Logger.LOG_LEVEL.DEBUG);
            try
            {
                Process.StandardError.Close();
            }
            catch (Exception)
            {

            }
            try
            {
                Process.StandardError.Dispose();
            }
            catch (Exception)
            {

            }
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
            SendLine(null);
            try
            {
                Process.StandardOutput.Close();
            } 
            catch(Exception)
            {

            }
            Logger.WriteLine("ProcessChecker exited", Logger.LOG_LEVEL.DEBUG);
        }

        public virtual bool SendLine(string line)
        {
            if(InputQueue != null)
                return InputQueue.TryAdd(line);
            return false;
        }

        public abstract void OnReceivedLine(string line);

        public abstract void OnReceivedErrorLine(string line);

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
            }
        }
    }
}
