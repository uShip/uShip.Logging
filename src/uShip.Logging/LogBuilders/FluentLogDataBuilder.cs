using log4net;
using log4net.Core;
using log4net.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using uShip.Logging.LogBuilders;

namespace uShip.Logging
{
    public partial class Logger : ILogger
    {
        private class FluentLogDataBuilder : IFluentLoggerWriter
        {
            private readonly ILog _log;
            private readonly LoggingEventDataBuilder _loggingEventDataBuilder;
            private string _message;
            private Severity? _severity;
            private Exception _exception;
            private string _sql;
            private IEnumerable<KeyValuePair<string, object>> _sqlParameters;
            private readonly IDictionary<string, object> _data = new Dictionary<string, object>();
            private readonly IList<string> _tags = new List<string>();

            public FluentLogDataBuilder(ILog log, LoggingEventDataBuilder loggingEventDataBuilder)
            {
                _log = log;
                _loggingEventDataBuilder = loggingEventDataBuilder;
            }

            public IFluentLoggerWriter Message(string message)
            {
                _message = message;
                return this;
            }

            public IFluentLoggerWriter Severity(Severity severity)
            {
                _severity = severity;
                return this;
            }

            public IFluentLoggerWriter Fatal()
            {
                _severity = Logging.Severity.Fatal;
                return this;
            }

            public IFluentLoggerWriter Error()
            {
                _severity = Logging.Severity.Error;
                return this;
            }

            public IFluentLoggerWriter Debug()
            {
                _severity = Logging.Severity.Debug;
                return this;
            }

            public IFluentLoggerWriter Warn()
            {
                _severity = Logging.Severity.Warn;
                return this;
            }

            public IFluentLoggerWriter Info()
            {
                _severity = Logging.Severity.Info;
                return this;
            }

            public IFluentLoggerWriter Exception(Exception exception)
            {
                _exception = exception;
                return this;
            }

            public IFluentLoggerWriter Data(string name, string value)
            {
                _data.Add(name, value);
                return this;
            }

            public IFluentLoggerWriter Data(string name, bool value)
            {
                _data.Add(name, value.ToString());
                return this;
            }

            public IFluentLoggerWriter Data(string name, decimal value)
            {
                _data.Add(name + "+Decimal", value);
                return this;
            }

            public IFluentLoggerWriter Data(string name, int value)
            {
                _data.Add(name + "+Int32", value);
                return this;
            }

            public IFluentLoggerWriter Data(string name, long value)
            {
                _data.Add(name + "+Int64", value);
                return this;
            }

            public IFluentLoggerWriter Tag(params string[] tags)
            {
                foreach (var tag in tags)
                {
                    _tags.Add(tag);
                }
                return this;
            }

            public IFluentLoggerWriter Sql(string sql, IEnumerable<KeyValuePair<string, object>> parameters)
            {
                _sql = sql;
                _sqlParameters = parameters;
                return this;
            }

            private static bool UseAsync = false;
            public void Write()
            {
                if (!_severity.HasValue)
                    _severity = _exception != null ? Logging.Severity.Error : Logging.Severity.Info;

                Action x = () =>
                {
                    LoggingEvent loggingEvent;
                    try
                    {
                        var properties = new LoggingEventPropertiesBuilder()
                            .WithMachineName()
                            .WithSqlData(_sql, _sqlParameters)
                            .WithException(_exception)
                            .WithCurrentVersion()
                            .WithUniqueOrigin(_message, _exception)
                            .WithCurrentContext()
                            .IncludeBasicRequestInfo()
                            .IncludeRequestBody()
                            .IncludeResponse()
                            .FinishContext()
                            .WithTags(_tags)
                            .WithAdditionalData(_data)
                            .Build();

                        var frame = new StackTrace().GetFrame(0);
                        var eventData = _loggingEventDataBuilder.Build(frame, _severity.Value, _message, _exception, properties);
                        loggingEvent = new LoggingEvent(eventData);
                    }
                    catch (Exception ex)
                    {
                        loggingEvent = HandlePropertiesBuildingException(ex);
                    }

                    try
                    {
                        _log.Logger.Log(loggingEvent);
                    }
                    catch(Exception e)
                    {
                        // We need something here eventually, if logstash is down we lose logs
                        Console.WriteLine("Failed to send messages to Logstash: " + e);
                    }
                };

                if (UseAsync)
                {
                    Task.Factory.StartNew(x);
                }
                else
                {
                    x();
                }
            }

            private LoggingEvent HandlePropertiesBuildingException(Exception ex)
            {
                var properties = new PropertiesDictionary();
                if (_exception != null)
                {
                    properties["Original.Exception.Message"] = _exception.Message;
                    properties["Original.Exception.StackTrace"] = _exception.StackTrace;
                }

                if (!string.IsNullOrEmpty(_message))
                {
                    properties["Original.Message"] = _message;
                }

                try
                {
                    properties["Exception"] = new LoggableException(ex);
                }
                catch
                {
                    properties["Exception.Message"] = ex.Message;
                    properties["Exception.StackTrace"] = ex.StackTrace;
                }

                properties["MachineName"] = Environment.MachineName;
                var eventData = new LoggingEventData
                {
                    Message = "Error building log properties",
                    Properties = properties,
                    Level = Level.Error
                };
                return new LoggingEvent(eventData);
            }
        }
    }
}