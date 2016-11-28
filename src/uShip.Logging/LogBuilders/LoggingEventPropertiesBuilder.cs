using log4net.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using uShip.Logging.Extensions;

namespace uShip.Logging.LogBuilders
{
    internal interface ILoggingEventPropertiesBuilder
    {
        ILoggingEventPropertiesBuilder WithMachineName();
        ILoggingEventPropertiesBuilder WithException(Exception exception);
        ILoggingEventPropertiesBuilder WithSqlData(string sql, IEnumerable<KeyValuePair<string, object>> parameters);
        ILoggingEventPropertiesBuilder WithUniqueOrigin(string message, Exception exception);
        ILoggingEventPropertiesBuilder WithCurrentVersion();

        ILoggingEventContextBuilder WithCurrentContext();
        
        ILoggingEventPropertiesBuilder WithAdditionalData(IDictionary<string, object> data);
        ILoggingEventPropertiesBuilder WithTags(IEnumerable<string> tags);

        PropertiesDictionary Build();
    }

    internal interface ILoggingEventContextBuilder
    {
        ILoggingEventContextBuilder WithRequest(HttpRequestBase request);
        ILoggingEventContextBuilder WithResponse(HttpResponseBase response);

        ILoggingEventContextBuilder IncludeBasicRequestInfo();
        ILoggingEventContextBuilder IncludeRequestBody(int? requestTruncateLength);
        ILoggingEventContextBuilder IncludeResponse(int? responseTruncateLength);

        ILoggingEventPropertiesBuilder FinishContext();
    }

    internal class LoggableException
    {
        public LoggableException(Exception ex)
        {
            Data = ex.Data.NewSanitizedDictionary();
            Message = ex.Message;
            Source = ex.Source;
            StackTrace = ex.StackTrace;
            TargetSite = ex.TargetSite;
            ExceptionTypeName = ex.GetType().FullName;
            if (ex.InnerException != null)
            {
                InnerException = new LoggableException(ex.InnerException);
            }
        }

        public IDictionary Data { get; set; }
        public LoggableException InnerException { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public string ExceptionTypeName { get; set; }
        public MethodBase TargetSite { get; set; }
    }

    internal class LoggingEventPropertiesBuilder : ILoggingEventPropertiesBuilder, ILoggingEventContextBuilder
    {
        private readonly PropertiesDictionary _props = new PropertiesDictionary();
        private LogType? LogEventType { get; set; }
        private string _sql;

        public ILoggingEventPropertiesBuilder WithMachineName()
        {
            _props.Set("MachineName", Environment.MachineName);
            return this;
        }

        public ILoggingEventPropertiesBuilder WithException(Exception exception)
        {
            LogEventType = GetLogType(exception);
            _props.Set("LogType", LogEventType.ToString());
            if (exception != null)
            {
                _props.Set("Exception", new LoggableException(exception));

                var httpException = exception as HttpException;
                if (httpException != null)
                {
                    _props.Set("ExceptionMessage", httpException.Message);
                    _props.Set("ErrorCode", httpException.ErrorCode);
                }
            }

            return this;
        }

        public ILoggingEventPropertiesBuilder WithCurrentVersion()
        {
            var pathSplit = AppDomain.CurrentDomain.BaseDirectory.Split(Path.DirectorySeparatorChar);
            var version = pathSplit.FirstOrDefault(x =>
            {
                int v;
                return Int32.TryParse(x, out v);
            });

            if (!String.IsNullOrEmpty(version))
            {
                _props.Set("DeployVersion", version);
            }
            return this;
        }

        public ILoggingEventPropertiesBuilder WithSqlData(string sql, IEnumerable<KeyValuePair<string, object>> sqlParameters)
        {
            if (!String.IsNullOrEmpty(sql))
            {
                _sql = sql;
                _props.Set("Sql", sql);
            }
            if (sqlParameters != null)
            {
                _props.Set("SqlParameters", sqlParameters.ToDictionary(x => x.Key, x => x.Value == null ? "null" : x.Value));
            }
            return this;
        }

        private LogType GetLogType(Exception exception)
        {
            if (exception != null)
            {
                return !String.IsNullOrEmpty(_sql) ? LogType.SqlException : LogType.Exception;
            }
            return LogType.Message;
        }

        public ILoggingEventPropertiesBuilder WithUniqueOrigin(string message, Exception exception)
        {
            _props.Set("Origin", GetUniqueOrigin(LogEventType ?? GetLogType(exception), message, exception));
            return this;
        }

        private string GetUniqueOrigin(LogType type, string message, Exception exception)
        {
            string uniqueOrigin;
            switch (type)
            {
                case LogType.Message:
                    uniqueOrigin = message;
                    break;
                case LogType.Exception:
                    var throwingMethod = GetThrowingMethod(exception);
                    var hasCustomMessage = !String.IsNullOrEmpty(message);
                    // a custom message always determines uniqueness
                    uniqueOrigin = hasCustomMessage ? message : String.Format("{0};{1}", exception.Message, throwingMethod);
                    break;
                case LogType.SqlException:
                    uniqueOrigin = String.Format("{0};{1}", exception.Message, _sql);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            return MD5Hash(uniqueOrigin);
        }

        private string GetThrowingMethod(Exception exception)
        {
            if (String.IsNullOrEmpty(exception.StackTrace))
            {
                return String.Empty;
            }
            var stacktrace = new StackTrace(exception);
            var stacktraceFrames = stacktrace.GetFrames().ToList();
            var targetStackframeNamespaces = uShipLogging.Config.TargetStackTraces
                .Cast<uShipLoggingConfigurationSection.TargetStackTracesElementCollection.AddElement>()
                .Select(x => x.RootNamespace);
            var excludedStackframeNamespaces = uShipLogging.Config.ExcludedStackTraces
                .Cast<uShipLoggingConfigurationSection.ExcludedStackTracesElementCollection.AddElement>()
                .Select(x => x.RootNamespace);
            var targetStacktraceFrame = stacktraceFrames.FirstOrDefault(x => 
                targetStackframeNamespaces.Any(rootNamespace => x.ToDescription().StartsWith(rootNamespace))
                && !excludedStackframeNamespaces.Any(rootNamespace => x.ToDescription().StartsWith(rootNamespace))
                );
            if (targetStacktraceFrame != null)
            {
                return targetStacktraceFrame.ToDescription();
            }
            return stacktraceFrames.FirstOrDefault().IfNotNull(x => x.ToDescription());
        }

        private string MD5Hash(string source)
        {
            using (var md5Hash = MD5.Create())
            {
                var parts = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source)).Select(x => x.ToString("x2"));
                return String.Join(String.Empty, parts);
            }
        }

        public ILoggingEventPropertiesBuilder WithAdditionalData(IDictionary<string, object> data)
        {
            if (data != null)
            {
                if (data.ContainsKey("AdditionalInfo"))
                    _props.Set("Parameters", data["AdditionalInfo"]);

                foreach (var datum in data)
                {
                    if (_props.Contains(datum.Key))
                    {
                        var duplicatePropsException = new ArgumentException("An element with the same key already exists.");
                        duplicatePropsException.Data.Add("Key", datum.Key);
                        duplicatePropsException.Data.Add("Value1", _props[datum.Key]);
                        duplicatePropsException.Data.Add("Value2", datum.Value);
                        throw duplicatePropsException;
                    }

                    _props.Set(datum.Key, datum.Value);
                }
            }
            return this;
        }

        public ILoggingEventPropertiesBuilder WithTags(IEnumerable<string> tags)
        {
            if (tags != null)
            {
                _props.Set("Tags", tags);
            }
            return this;
        }

        public PropertiesDictionary Build()
        {
            return _props;
        }

        #region Context properties
        private HttpContextBase _context;
        private HttpRequestBase _request;
        private HttpResponseBase _response;

        public ILoggingEventContextBuilder WithCurrentContext()
        {
            var currentContext = HttpContext.Current;
            if (currentContext == null)
            {
                return this;
            }

            _context = new HttpContextWrapper(currentContext);

            try
            {
                _request = _context.Request;
                _response = _context.Response;
            }
            catch (Exception)
            {
                _props.Set("HttpRequest", "Unable to read request instance");
            }

            return this;
        }

        public ILoggingEventContextBuilder WithRequest(HttpRequestBase request)
        {
            if (request != null)
            {
                _request = request;
            }
            return this;
        }

        public ILoggingEventContextBuilder WithResponse(HttpResponseBase response)
        {
            if (response != null)
            {
                _response = response;
            }
            return this;
        }

        public ILoggingEventContextBuilder IncludeBasicRequestInfo()
        {
            if (_request != null)
            {
                _props.Set("Url", _request.Url.OriginalString.CleanQueryString());
                _props.Set("UserAgent", _request.UserAgent);
                _props.SafeSetProp("IPAddress", () => GetCallingIpAddress(_context));
                _props.Set("RequestMethod", _request.HttpMethod);

                _props.SafeSetProp("RequestHeaders", () => _request.Headers.ToQuery());
                _props.SafeSetProp("Referrer", () => _request.ServerVariables["HTTP_REFERER"].CleanQueryString());
                _props.SafeSetProp("RawUrl", () => _request.RawUrl);
                _props.SafeSetProp("QueryString", () => _request.QueryString.ToString().CleanQueryString());
                _props.SafeSetProp("Server", () => _request.ServerVariables["LOCAL_ADDR"]);
            }
            return this;
        }

        public ILoggingEventContextBuilder IncludeRequestBody(int? requestTruncateLength)
        {
            if (_request == null) return this;

            Func<string, string> sanitize = s =>
                s.IfNotNull(x => x
                    .SanitizeSensitiveInfo()).IfNotNull(x => x
                        .RemoveViewState());
            
            _props.SafeSetProp("RequestBody", () =>
            {
                var requestForm = _request.Form.IfNotNull(x => x.ToString());
                var content = !String.IsNullOrEmpty(requestForm)
                    ? requestForm
                    : _request.GetContent();
                return Truncate(sanitize(content), requestTruncateLength);
            });

            return this;
        }

        private static string Truncate(string input, int? maxLength)
        {
            if (!maxLength.HasValue || input == null) return input;
            if (maxLength >= input.Length) return input;
            if (maxLength.Value < 0) throw new ArgumentOutOfRangeException("maxLength", "Truncate Length must be a non-negative number");

            return input.Substring(0, maxLength.Value) + "  #truncated#";
        }

        private string GetCallingIpAddress(HttpContextBase context)
        {
            if (context == null || context.Request == null)
            {
                return String.Empty;
            }
            if (!String.IsNullOrEmpty(context.Request["HTTP_TRUE_CLIENT_IP"]))
            {
                return context.Request["HTTP_TRUE_CLIENT_IP"];
            }
            if (!String.IsNullOrEmpty(context.Request["HTTP_X_FORWARDED_FOR"]))
            {
                return context.Request["HTTP_X_FORWARDED_FOR"].Split(',')[0];
            }
            if (!String.IsNullOrEmpty(context.Request["HTTP_X_CLIENTSIDE"]))
            {
                return context.Request["HTTP_X_CLIENTSIDE"];
            }
            if (!String.IsNullOrEmpty(context.Request["HTTP_X_CLUSTER_CLIENT_IP"]))
            {
                return context.Request["HTTP_X_CLUSTER_CLIENT_IP"];
            }
            return context.Request["REMOTE_ADDR"];
        }

        public ILoggingEventContextBuilder IncludeResponse(int? responseTruncateLength)
        {
            if (_response != null)
            {
                _props.Set("StatusCode", _response.StatusCode);

                var outputStream = _response.OutputStream;
                if (outputStream != null && outputStream.CanRead)
                {
                    outputStream.Position = 0;
                    _props.Set("ResponseBody", Truncate(outputStream.ReadAllText(), responseTruncateLength));
                }

                _props.SafeSetProp("ResponseHeaders", () => _response.Headers.ToQuery());
            }
            return this;
        }

        public ILoggingEventPropertiesBuilder FinishContext()
        {
            return this;
        }
        #endregion Context properties
    }
}