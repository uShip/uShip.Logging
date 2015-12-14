using log4net;

namespace uShip.Logging
{
    internal class LogFactory
    {
        internal ILog Create(ConfiguredLogger configuredLogger)
        {
            return LogManager.GetLogger(configuredLogger.ToString());
        }
    }
}