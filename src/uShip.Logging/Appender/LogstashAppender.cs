using log4net.Appender;
using log4net.Core;
using Newtonsoft.Json;
using System;
using uShip.Logging.LogBuilders;

namespace uShip.Logging.Appender
{
    public sealed class LogstashAppender : UdpAppender
    {
        public LogstashAppender()
        {
            ErrorHandler = new Log4NetErrorHandler(RemoteAddress, RemotePort);
        }

        protected override bool RequiresLayout
        {
            get { return false; }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            string message;
            try
            {
                message = JsonConvert.SerializeObject(loggingEvent);
            }
            catch (Exception ex)
            {
                var serializationFailureEvent = new LoggingEvent(new LoggingEventData
                {
                    Message = string.Format(
                        "Failed to serialize the type LoggingEvent in type LogstashAppender. Original Message [{0}] Exception Message [{1}]",
                        loggingEvent.RenderedMessage, ex.Message)
                });

                message = JsonConvert.SerializeObject(serializationFailureEvent);
            }

            try
            {
                var bytes = Encoding.GetBytes(message);
                Client.Send(bytes, bytes.Length, RemoteEndPoint);
            }
            catch (Exception ex)
            {
                ErrorHandler.Error(string.Format("Unable to send logging event to remote host {0} on port {1}.", RemoteAddress, RemotePort), ex, ErrorCode.WriteFailure);
            }
        }
    }
}