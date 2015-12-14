using System;
using System.Diagnostics;
using log4net;
using log4net.Core;
using log4net.Util;
using Newtonsoft.Json;

namespace uShip.Logging.LogBuilders
{
    public interface IMinimalLogDataBuilder
    {
        IMinimalLogDataBuilder Message(string message);
        IMinimalLogDataBuilder WithRawJsonObject(string jsonData);
        IMinimalLogDataBuilder WithSeverity(Severity? severity);
        void Write();
    }

    public class MinimalLogDataBuilder : IMinimalLogDataBuilder
    {
        private readonly ILog _log;
        private readonly LoggingEventDataBuilder _loggingEventDataBuilder;

        private string _message;
        private string _jsonData = null;
        private Severity? _severity;

        public MinimalLogDataBuilder(ILog log, LoggingEventDataBuilder loggingEventDataBuilder)
        {
            _log = log;
            _loggingEventDataBuilder = loggingEventDataBuilder;
        }

        public IMinimalLogDataBuilder Message(string message)
        {
            _message = message;
            return this;
        }

        public IMinimalLogDataBuilder WithRawJsonObject(string jsonData)
        {
            if (!string.IsNullOrEmpty(jsonData))
                _jsonData = jsonData;
            return this;
        }

        public IMinimalLogDataBuilder WithSeverity(Severity? severity)
        {
            _severity = severity;
            return this;
        }

        public void Write()
        {
            var properties = new PropertiesDictionary();
            if (_jsonData != null)
            {
                try
                {
                    properties["data"] = JsonConvert.DeserializeObject(_jsonData);
                }
                catch (Exception)
                {
                    properties["data"] = _jsonData;
                }
            }

            var frame = new StackTrace().GetFrame(0);
            var logEvent = _loggingEventDataBuilder.Build(frame, _severity ?? Severity.Info, _message, null, properties);

            _log.Logger.Log(new LoggingEvent(logEvent));
        }
    }
}