using System;
using System.Collections.Generic;
using System.Text;

namespace EP94.LgSmartThinq.Utils
{
    public enum LogLevel
    {
        Fatal,
        Error,
        Warning,
        Information,
        Debug,
        Verbose
    }
    public class SmartThinqLogger
    {
        public static LogHandler OnNewLogMessage;
        public delegate void LogHandler(string message, LogLevel logLeve, params object[] parameters);
        internal static void Log(string message, LogLevel logLevel, params object[] parameters)
        {
            OnNewLogMessage?.Invoke(message, logLevel, parameters);
        }
    }
}
