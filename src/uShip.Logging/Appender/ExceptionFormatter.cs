using log4net.Core;
using System;
using System.Globalization;
using uShip.Logging.Extensions;
using uShip.Logging.LogBuilders;

namespace uShip.Logging.Appender
{
    internal static class ExceptionFormatter
    {
        public static string Format(LoggingEvent loggingEvent)
        {
            var message = new FormattedMessage
            {
                Time = loggingEvent.TimeStamp.ToString(CultureInfo.InvariantCulture),
                Message = loggingEvent.RenderedMessage,
                Parameters = loggingEvent.Properties["Parameters"] as string,
                QueryString = loggingEvent.Properties["QueryString"] as string,
                Referrer = loggingEvent.Properties["Referrer"] as string,
                Server = loggingEvent.Properties["Server"] as string,
                Url = loggingEvent.Properties["Url"] as string,
                UserAgent = loggingEvent.Properties["UserAgent"] as string,
                IpAddress = loggingEvent.Properties["IPAddress"] as string,
                RequestHeaders = loggingEvent.Properties["RequestHeaders"] as string,
                RequestMethod = loggingEvent.Properties["RequestMethod"] as string,
                ResponseHeaders = loggingEvent.Properties["ResponseHeaders"] as string,
                StatusCode = Convert.ToInt32(loggingEvent.Properties["StatusCode"]),
                RequestBody = loggingEvent.Properties["RequestBody"] as string,
                ResponseBody = loggingEvent.Properties["ResponseBody"] as string,
                UserName = loggingEvent.Identity
            };

            var exception = loggingEvent.Properties["Exception"] as LoggableException;
            if (exception != null)
            {
                message.SqlErrorMessages = string.IsNullOrEmpty(message.Parameters) ? string.Empty : exception.Message;

                message.Source = exception.Source;
                message.StackTrace = exception.StackTrace;
                message.TargetSite = exception.TargetSite.IfNotNull(x => x.ToString());
                message.InnerException = exception.InnerException;
            }

            if (loggingEvent.Properties.Contains("ExceptionMessage"))
            {
                message.ExceptionMessage = loggingEvent.Properties["ExceptionMessage"].ToString();
            }

            if (loggingEvent.Properties.Contains("ErrorCode"))
            {
                message.ErrorCode = (int)loggingEvent.Properties["ErrorCode"];
            }

            return ExceptionLogFormatter.FormatMessage(message);
        }
    }
}