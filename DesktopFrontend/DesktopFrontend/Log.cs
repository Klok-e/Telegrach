using System;
using Avalonia.Logging;

namespace DesktopFrontend
{
    public static class Log
    {
        public static class Areas
        {
            public const string Network = "Network";
        }

        public static void Error(string area, object source, string message)
        {
            Logger.Sink.Log(LogEventLevel.Error, area, source, message);
        }

        public static void Warn(string area, object source, string message)
        {
            Logger.Sink.Log(LogEventLevel.Warning, area, source, message);
        }

        public static void Info(string area, object source, string message)
        {
            Logger.Sink.Log(LogEventLevel.Information, area, source, message);
        }
    }
}