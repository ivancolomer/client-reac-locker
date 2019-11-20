using REAC_LockerDevice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace REAC_LockerDevice.Utils.Output
{
    class Logger
    {
        private const string TEXT_RESET = "\u001b[0m";

        private const string DECORATION_BOLD = "\u001b[1m";
        private const string DECORATION_UNDERLINE = "\u001b[4m";

        private const string COLOR_BLACK = "\u001b[30m";
        private const string COLOR_RED = "\u001b[31m";
        private const string COLOR_GREEN = "\u001b[32m";
        private const string COLOR_YELLOW = "\u001b[33m";
        private const string COLOR_BLUE = "\u001b[34m";
        private const string COLOR_MAGENTA = "\u001b[35m";
        private const string COLOR_CYAN = "\u001b[36m";
        private const string COLOR_WHITE = "\u001b[37m";

        public enum LOG_LEVEL { 
            DEBUG,
            INFO,
            WARN,
            ERROR
        };

        private static readonly object n = new object();

        public static void Initialize()
        {
            if (!Directory.Exists("log/"))
                Directory.CreateDirectory("log/");

            WriteLine("AndroidAPI has started!", LOG_LEVEL.INFO);
        }

        public static void WriteLine(string text, LOG_LEVEL logLevel)
        {
            WriteLineWithHeader(text, "", logLevel);
        }

        public static void WriteLineWithHeader(string text, string header, LOG_LEVEL logLevel)
        {
            string unformattedText = "[" + DateTime.Now + "] ";
            string formattedText = COLOR_GREEN + unformattedText + TEXT_RESET;

            if (header != "")
            {
                unformattedText += "[" + header + "] ";
                formattedText += "[" + COLOR_GREEN + header + TEXT_RESET + "] ";
            }

            unformattedText += text + Environment.NewLine;
            formattedText += getFormatStart(logLevel) + text + getFormatEnd(logLevel) + Environment.NewLine;

            Write(formattedText, unformattedText);
        }

        private static void Write(string formattedText, string unformattedText)
        {
            lock (n)
            {
                try
                {
                    using (StreamWriter outputFile = File.AppendText("log/" + Program.InitStartUTC.ToString("yyyy-MM-dd_HH-mm-ss") + ".log"))
                    {
                        outputFile.Write(unformattedText);
                    }
                    Console.Write(formattedText);
                } catch(Exception) {
                    //Console.WriteLine(e.ToString());
                }
            }
        }

        private static string getFormatStart(LOG_LEVEL logLevel)
        {
            switch (logLevel)
            {
                case LOG_LEVEL.INFO:
                    return COLOR_BLUE;
                case LOG_LEVEL.WARN:
                    return COLOR_YELLOW;
                case LOG_LEVEL.ERROR:
                    return COLOR_RED + DECORATION_BOLD;
                default:
                    return TEXT_RESET;
            }
        }

        private static string getFormatEnd(LOG_LEVEL logLevel)
        {
            switch (logLevel)
            {
                case LOG_LEVEL.DEBUG:
                    return "";
                default:
                    return TEXT_RESET;
            }
        }
    }
}

