using System;
using ReactiveUI;

namespace DesktopFrontend
{
    public static class Utils
    {
        public static void LogErrors<T1, T2>(this ReactiveCommand<T1, T2> command, string area, object source)
        {
            command.ThrownExceptions.Subscribe(e => Log.Error(area, source, e.ToString()));
        }
    }
}