using log4net;

namespace uShip.Logging
{
    public class LogFactory
    {
        public virtual ILog Create(ConfiguredLogger configuredLogger)
        {
            return LogManager.GetLogger(configuredLogger.ToString());
        }
    }
}