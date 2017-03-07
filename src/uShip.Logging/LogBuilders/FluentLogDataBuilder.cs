﻿using log4net;
using log4net.Core;
using log4net.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using uShip.Logging.LogBuilders;

namespace uShip.Logging
{
    public partial class Logger : ILogger
    {
        internal class FluentLogDataBuilder : IFluentLoggerWriter
        {
            private readonly ILog _log;
            private readonly LoggingEventDataBuilder _loggingEventDataBuilder;
            private string _message;
            private Severity? _severity;
            private Exception _exception;
            private string _sql;
            private IEnumerable<KeyValuePair<string, object>> _sqlParameters;
            internal readonly IDictionary<string, object> _data = new Dictionary<string, object>();
            private readonly IList<string> _tags = new List<string>();
            private HttpRequestBase _request;
            private HttpResponseBase _response;
            private bool _shouldOmitRequestBody;
            private FluentLoggerOptions _advancedOptions = new FluentLoggerOptions();

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

            /// <summary>
            /// Reflectively add non-complex properties (exceptions like DateTime and DateTimeOffset) from an object to the Data dictionary to be included in the logger's message.
            /// </summary>
            /// <param name="prefix">Prefix that will be prepended to the property key names in the data dictionary.</param>
            /// <param name="value">Object/class that we are logging properties from.</param>
            /// <returns></returns>
            public IFluentLoggerWriter Data(string prefix, object value)
            {
                MapDataFromObject(prefix, value, true);
                return this;
            }

            /// <summary>
            /// Short-hand version of <see cref="Data(string,object)"/ of logging data from an object.
            /// </summary>
            /// <param name="value">Object's whose type will be used as the prefix to logging key name for all its properties.</param>
            /// <returns></returns>
            public IFluentLoggerWriter Data(object value)
            {
                var name = value.GetType().Name + '_';
                return Data(name, value);
            }

            /// <summary>
            /// Recursive method that adds properties of an object to the Data dictionary to be included in the logger's message.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="value"></param>
            /// <param name="topLevel"></param>
            private void MapDataFromObject(string name, object value, bool topLevel = false)
            {
                var type = value.GetType();        
                if (topLevel)
                {                    
                    var props = type.GetProperties();
                    //hard limit to 30 for now
                    //TODO: Extend this to be dynamic.
                    foreach (var prop in props.Take(30))
                    {
                        var propertyPrefixedName = name + prop.Name;
                        //this seems really odd, but it is how you peel the properties's values off of the topLevel object. 
                        var val = prop.GetValue(value, null);
                        MapDataFromObject(propertyPrefixedName, val);
                    }
                    //we infer that this is definitely not a primitive and exit early. 
                    return;
                }
                                
                if (type.IsPrimitive
                    || type == typeof(Decimal)
                    || type == typeof(String)
                    || type == typeof(DateTime)
                    || type == typeof(DateTimeOffset)
                    || type == typeof(TimeSpan)
                    )
                {
                    
                    //primitives
                    if (type == typeof(String))
                    {
                        _data.Add(name, (String) value);
                    }
                    else if (type == typeof(int))
                    {
                        _data.Add(name, (int) value);
                    }
                    else if (type == typeof(long))
                    {                       
                        _data.Add(name, (long) value);
                    }
                    else if (type == typeof(float))
                    {
                        _data.Add(name, (float)value);
                    }
                    else if (type == typeof(double))
                    {
                        _data.Add(name, (double)value);
                    }
                    else if (type == typeof(Boolean))
                    {
                        _data.Add(name, (Boolean)value);
                    }
                    else if (type == typeof(Decimal))
                    {
                        _data.Add(name, (decimal)value);
                    }
                    //dates
                    else if (type == typeof(DateTime))
                    {
                        _data.Add(name, (DateTime) value);
                    }
                    else if (type == typeof(DateTimeOffset))
                    {
                        _data.Add(name, (DateTimeOffset)value);
                    }         
                }                              
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

            public IFluentLoggerWriter Request(HttpRequestBase request)
            {
                _request = request;
                return this;
            }

            public IFluentLoggerWriter Response(HttpResponseBase response)
            {
                _response = response;
                return this;
            }

            public IFluentLoggerWriter AdvancedOptions(FluentLoggerOptions options)
            {
                _advancedOptions.Merge(options);
                return this;
            }

            public IFluentLoggerWriter OmitRequestBody()
            {
                _shouldOmitRequestBody = true;
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
                        var loggingEventContextBuilder = new LoggingEventPropertiesBuilder()
                            .WithMachineName()
                            .WithSqlData(_sql, _sqlParameters)
                            .WithException(_exception)
                            .WithCurrentVersion()
                            .WithUniqueOrigin(_message, _exception)
                            .WithCurrentContext()
                            .WithRequest(_request)
                            .WithResponse(_response)
                            .IncludeBasicRequestInfo();
                        if (!_shouldOmitRequestBody)
                        {
                            loggingEventContextBuilder = loggingEventContextBuilder.IncludeRequestBody(_advancedOptions.TruncateRequestBodyCharactersTo);
                        }
                        var properties = loggingEventContextBuilder
                            .IncludeResponse(_advancedOptions.TruncateResponseBodyCharactersTo)
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
                        loggingEvent = HandlePropertiesBuildingException(ex, _severity.Value);
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

            private LoggingEvent HandlePropertiesBuildingException(Exception ex, Severity severity)
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
                    Level = severity.ToLog4NetLevel()
                };
                return new LoggingEvent(eventData);
            }
        }
    }
}