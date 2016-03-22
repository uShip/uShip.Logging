namespace uShip.Logging.Appender
{
    /// <summary>
    /// An Appender that sends error logs to a specified API endpoint
    /// </summary>
    public class ApiErrorLogAppender : BaseApiLogAppender<ApiErrorLogModel>
    {
        public virtual string ApiUrl
        {
            get { return LoggingUrl; }
            set { LoggingUrl = value; }
        }
    }
}