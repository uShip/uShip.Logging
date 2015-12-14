using System;
using log4net.Core;

namespace uShip.Logging
{
    public enum Severity
    {
        Fatal,
        Error,
        Warn,
        Info,
        Debug,
    }

    internal static class SeverityExtensions
    {
        internal static Level ToLog4NetLevel(this Severity severity)
        {
            switch (severity)
            {
                case Severity.Info:
                    return Level.Info;
                case Severity.Debug:
                    return Level.Debug;
                case Severity.Warn:
                    return Level.Warn;
                case Severity.Error:
                    return Level.Error;
                case Severity.Fatal:
                    return Level.Fatal;
                default:
                    throw new ArgumentOutOfRangeException("severity");
            }
        }
    }
}