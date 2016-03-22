using System;
using System.Net.Http;
using System.Text;
using log4net.Appender;
using log4net.Core;
using Newtonsoft.Json;
using uShip.Logging.Extensions;

namespace uShip.Logging.Appender
{
    /// <summary>
    /// A base class for which one can log to an API
    /// </summary>
    public class BaseApiLogAppender<T> : AppenderSkeleton
        where T : ApiErrorLogModel, new()
    {
        protected override bool RequiresLayout
        {
            get { return false; }
        }

        protected virtual string LoggingUrl { get; set; }
        public virtual SecurityContext SecurityContext { get; set; }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            if (SecurityContext == null)
                SecurityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
        }
        
        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                using (SecurityContext.Impersonate(this))
                {
                    var content = JsonConvert.SerializeObject(BuildApiErrorLogModel(loggingEvent));
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(LoggingUrl),
                        Content = new StringContent(content, Encoding.UTF8, "application/json")
                    };

                    var handler = new HttpClientHandler
                    {
                        AllowAutoRedirect = false
                    };
                    var httpClient = new HttpClient(handler)
                    {
                        Timeout = TimeSpan.FromMinutes(1)
                    };
                    var task = httpClient.SendAsync(request);
                    task.ContinueWith(t =>
                    {
                        HttpResponseMessage response = null;
                        try
                        {
                            response = t.Result;
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.Error("Error occurred while sending exception log.", ex);
                        }
                        if (null == response) return;
                        if ((int) response.StatusCode < 400) return; // error handle only 400 and 500 codes
                        
                        var result = response.Content.ReadAsStringAsync().Result;
                        ErrorHandler.Error(string.Format("An error occurred during POST of an error, status code: {0} content: {1}",
                            response.StatusCode,
                            response.Content == null ? null : result));
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Error("Error occurred while sending exception log.", ex);
            }
        }

        protected virtual T BuildApiErrorLogModel(LoggingEvent loggingEvent)
        {
            return new T
            {
                RenderedMessage = loggingEvent.RenderedMessage,
                MessageObject = loggingEvent.MessageObject,
                //This is generally empty:
                //ExceptionString = loggingEvent.GetExceptionString(),
                ExceptionString = (loggingEvent.Properties["Exception"] as Exception).IfNotNull(x =>x .ToString()),
                MachineName = loggingEvent.Properties["MachineName"].IfNotNull(x => x.ToString()),
                //These have properties "message" and "Message" which causes a deserialization error
                //  "Message" has been null the times I've seen it, so excluding default values might work
                //ExceptionObject = (loggingEvent.Properties["Exception"] as Exception),
                //EventData = loggingEvent.GetLoggingEventData()
            };
        }
    }
}