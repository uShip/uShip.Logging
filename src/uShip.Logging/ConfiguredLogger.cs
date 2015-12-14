namespace uShip.Logging
{
    public enum ConfiguredLogger
    {
        /// <summary>
        /// This is the flag for the Graphite logger.
        /// </summary>
        Graphite,
        /// <summary>
        /// This is the flag for the main Logstash logger.
        /// </summary>
        Logstash,
        /// <summary>
        /// This is the flag for the minimal Logstash logger.
        /// </summary>
        Minimal
    }
}