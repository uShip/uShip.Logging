using System.Collections.Generic;
using log4net;
using log4net.Config;
using System;
using System.Text;
using uShip.Logging.Extensions;
using uShip.Logging.LogBuilders;

namespace uShip.Logging
{
    public partial class Logger : ILogger
    {
        private readonly string _graphiteMetricPath = uShipLogging.Config.GraphiteMetricPath;
         
        private readonly LoggingEventDataBuilder _loggingEventDataBuilder = new LoggingEventDataBuilder();

        private readonly ILog _logstashLog;
        private readonly ILog _graphiteLog;
        private readonly ILog _minimalLog;

        private readonly string _graphiteCountFormat;
        private readonly string _graphiteTimedFormat;

        private static readonly Lazy<ILogger> LazyInstance = new Lazy<ILogger>(() => new Logger(new LogFactory(), new LoggingEventDataBuilder()));

        public static ILogger GetInstance()
        {
            return LazyInstance.Value;
        }

        /// <summary>
        /// This is invoked the first time this class is used. If it throws it will throw until app domain cycles
        /// It is needed to read from the webconfig for log4net sections. 
        /// </summary>
        static Logger()
        {
            var errors = XmlConfigurator.Configure();
            if (errors.Count > 0)
            {
                var builder = new StringBuilder();
                foreach (var error in errors)
                {
                    builder.AppendLine(error.IfNotNull(x => x.ToString()));
                }
                throw new Exception("Failed to configure log4net during startup.\n\n" + builder);
            }
        }

        public Logger(LogFactory logFactory, LoggingEventDataBuilder loggingEventDataBuilder)
        {
            _loggingEventDataBuilder = loggingEventDataBuilder;

            _logstashLog = logFactory.Create(ConfiguredLogger.Logstash);
            _graphiteLog = logFactory.Create(ConfiguredLogger.Graphite);
            _minimalLog = logFactory.Create(ConfiguredLogger.Minimal);
            _graphiteCountFormat = _graphiteMetricPath + "{0}{1}:{2}|c";
            _graphiteTimedFormat = "{0}{1}:{2}|ms";
        }

        private IFluentLoggerWriter CreateMessageBuilder()
        {
            return new FluentLogDataBuilder(_logstashLog, _loggingEventDataBuilder);
        }

        public IFluentLoggerWriter Message(string message)
        {
            return CreateMessageBuilder().Message(message);
        }

        public IFluentLoggerWriter Exception(Exception exception)
        {
            return CreateMessageBuilder().Exception(exception);
        }

        public void Write(IGraphiteKey key, string subKey = null, Dictionary<string, string> tags = null)
        {
            var message = FormatGraphiteMessage(key.Key, subKey, null, tags: tags);
            _graphiteLog.Info(message);
        }

        public void Write(int count, IGraphiteKey key, string subKey = null, Dictionary<string, string> tags = null)
        {
            var message = FormatGraphiteMessage(key.Key, subKey, null, count, tags: tags);
            _graphiteLog.Info(message);
        }

        public void Write(IGraphiteKey key, long milliseconds, Dictionary<string, string> tags = null)
        {
            var message = FormatGraphiteMessage(key.Key, null, milliseconds, tags: tags);
            _graphiteLog.Info(message);
        }

        public void Write(IGraphiteKey key, string subKey, long milliseconds, Dictionary<string, string> tags = null)
        {
            var message = FormatGraphiteMessage(key.Key, subKey, milliseconds, tags: tags);
            _graphiteLog.Info(message);
        }

        public void Count(string key, int count, Dictionary<string, string> tags = null)
        {
            Count(key, null, count, tags);
        }

        public void Count(string key, string subkey, int count, Dictionary<string, string> tags = null)
        {
            var message = FormatGraphiteMessage(key, null, null, count, tags);
            _graphiteLog.Info(message);
        }

        public void Timer(string key, TimeSpan span, Dictionary<string, string> tags = null)
        {
            Timer(key, null, span, tags);
        }

        public void Timer(string key, string subkey, TimeSpan span, Dictionary<string, string> tags = null)
        {
            var message = FormatGraphiteMessage(key, subkey, Convert.ToInt64(span.TotalMilliseconds), tags: tags);
            _graphiteLog.Info(message);
        }

        private IMinimalLogDataBuilder CreateMinimalMessageBuilder()
        {
            return new MinimalLogDataBuilder(_minimalLog, _loggingEventDataBuilder);
        }

        public void WriteMinimalDataLog(string jsonData, Severity? severity = null)
        {
            CreateMinimalMessageBuilder()
                .Message(uShipLogging.Config.MinimalDataLogMessage)
                .WithSeverity(severity)
                .WithRawJsonObject(jsonData)
                .Write();
        }

        private string FormatGraphiteMessage(string key, 
                                            string subKey, 
                                            long? milliseconds, 
                                            int count = 1, 
                                            Dictionary<string, string> tags = null)
        {
            if (!string.IsNullOrEmpty(subKey))
            {
                key = key + "." + subKey;
            }

            var message = milliseconds == null 
                ? string.Format(_graphiteCountFormat, key, GetDataTags(tags),count) 
                : string.Format(_graphiteTimedFormat, key, GetDataTags(tags), milliseconds);

            return message;
        }

        private string GetDataTags(Dictionary<string, string> tags)
        {
            const string tagFormat = "~{0}={1}";
            
            var tagsToWrite = new StringBuilder();

            if (tags == null) return tagsToWrite.ToString();

            foreach (var tag in tags)
            {
                tagsToWrite.Append(string.Format(tagFormat, tag.Key, tag.Value));
            }

            return tagsToWrite.ToString(); 
        }
    }
}