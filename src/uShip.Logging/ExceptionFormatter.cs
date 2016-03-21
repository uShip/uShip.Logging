using System;
using System.IO;
using System.Xml;
using uShip.Logging.Extensions;
using uShip.Logging.LogBuilders;

namespace uShip.Logging
{
    internal static class ExceptionLogFormatter
    {
        public static string FormatMessage(FormattedMessage message)
        {
            var xml = new XmlDocument();
            var root = xml.CreateElement("Information");

            xml.AppendChild(root);

            Append(root, "Time", message.Time);
            Append(root, "Message", message.Message);
            Append(root, "Parameters", message.Parameters);
            Append(root, "SQLErrorMessages", message.SqlErrorMessages);
            Append(root, "Source", message.Source);
            Append(root, "StackTrace", message.StackTrace);
            Append(root, "TargetSite", message.TargetSite);

            if (message.InnerException != null)
            {
                var formattedException = FormatException(message.InnerException);
                AppendException(xml, root, formattedException);
            }

            var additionalInfo = message.UserName;
            if (message.StatusCode >= 400 && message.StatusCode < 500)
                additionalInfo += "Code: " + message.StatusCode;

            Append(root, "EnvironmentStackTrace", Environment.StackTrace);
            Append(root, "Form", message.Form);
            Append(root, "QueryString", message.QueryString);
            Append(root, "Referrer", message.Referrer);
            Append(root, "Url", message.Url);
            Append(root, "UserAgent", message.UserAgent);
            Append(root, "AdditionalInfo", additionalInfo);
            Append(root, "Server", message.Server);
            Append(root, "IPAddress", message.IpAddress);
            Append(root, "RequestHeaders", message.RequestHeaders);
            Append(root, "RequestMethod", message.RequestMethod);
            Append(root, "ResponseHeaders", message.ResponseHeaders);
            Append(root, "StatusCode", message.StatusCode);
            Append(root, "ExceptionMessage", message.ExceptionMessage);
            Append(root, "ErrorCode", message.ErrorCode);
            Append(root, "RequestBody", message.RequestBody);
            Append(root, "ResponseBody", message.ResponseBody);

            var sw = new StringWriter();
            var xtw = new XmlTextWriter(sw) { Formatting = Formatting.Indented };
            xml.WriteTo(xtw);

            var s = sw.ToString();
            return s;
        }

        private static FormattedException FormatException(LoggableException exception)
        {
            var message = new FormattedException
            {
                Message = exception.Message,
                EnvironmentStackTrace = Environment.StackTrace,
                Source = exception.Source,
                StackTrace = exception.StackTrace,
                TargetSite = exception.TargetSite.IfNotNull(x => x.ToString())
            };
            if (exception.InnerException != null)
            {
                message.InnerException = FormatException(exception.InnerException);
            }
            return message;
        }

        private static void Append(XmlElement element, string key, object value)
        {
            try
            {
                if (Convert.ToInt32(value) == 0) return;
            }
            catch
            {

            }
            if (value == null || string.IsNullOrEmpty(value.ToString())) return;

            var document = element.OwnerDocument;
            if (document == null)
                throw new Exception("Could not resolve OwnerDocument");

            var child = document.CreateElement(key);
            child.InnerText = value.ToString();
            element.AppendChild(child);
        }

        private static void AppendException(XmlDocument document, XmlElement element, FormattedException exception)
        {
            var exceptionElement = document.CreateElement("InnerException");

            Append(exceptionElement, "Message", exception.Message);
            Append(exceptionElement, "Source", exception.Source);
            Append(exceptionElement, "StackTrace", exception.StackTrace);
            Append(exceptionElement, "TargetSite", exception.TargetSite);
            if (exception.InnerException != null)
            {
                AppendException(document, exceptionElement, exception.InnerException);
            }
            Append(exceptionElement, "EnvironmentStackTrace", Environment.StackTrace);

            element.AppendChild(exceptionElement);
        }
    }

    internal class FormattedMessage
    {
        public string Time { get; set; }
        public string Message { get; set; }
        public string Parameters { get; set; }
        public string SqlErrorMessages { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public string TargetSite { get; set; }
        public LoggableException InnerException { get; set; }
        public string UserName { get; set; }
        public string Form { get; set; }
        public string QueryString { get; set; }
        public string Referrer { get; set; }
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public string Server { get; set; }
        public string IpAddress { get; set; }
        public string RequestHeaders { get; set; }
        public string RequestMethod { get; set; }
        public string ResponseHeaders { get; set; }
        public int StatusCode { get; set; }
        public string ExceptionMessage { get; set; }
        public int ErrorCode { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
    }

    internal class FormattedException
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public string TargetSite { get; set; }
        public FormattedException InnerException { get; set; }
        public string EnvironmentStackTrace { get; set; }
    }
}
