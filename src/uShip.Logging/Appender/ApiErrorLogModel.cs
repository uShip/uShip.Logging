using log4net.Core;

namespace uShip.Logging.Appender
{
    /// <summary>
    /// Represents a summary of an API Log mocking logstash
    /// </summary>
    public class ApiErrorLogModel
    {
        /// <summary>
        /// Gets or sets the rendered message as found in the <see cref="LoggingEvent"/> class.
        /// </summary>
        public string RenderedMessage { get; set; }

        /// <summary>
        /// Gets or sets the message object as found in the <see cref="LoggingEvent"/> class.
        /// </summary>
        public object MessageObject { get; set; }

        /// <summary>
        /// Gets or sets the exception string if present.
        /// </summary>
        public string ExceptionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the machine on which the log was created.
        /// </summary>
        public string MachineName { get; set; }

    }
}