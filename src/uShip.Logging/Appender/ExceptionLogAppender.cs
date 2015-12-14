using log4net.Appender;
using log4net.Core;
using log4net.Util;
using System;
using System.IO;

namespace uShip.Logging.Appender
{
    public class ExceptionLogAppender : AppenderSkeleton
    {
        public string BaseDirectory { get; set; }
        public SecurityContext SecurityContext { get; set; }

        protected override bool RequiresLayout
        {
            get { return false; }
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            if (SecurityContext == null)
                SecurityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
            using (SecurityContext.Impersonate(this))
                BaseDirectory = ConvertToFullPath(BaseDirectory.Trim());
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                using (SecurityContext.Impersonate(this))
                {
                    var fileName = string.Format("{0}.config", Guid.NewGuid().ToString("N"));
                    var levelPath = Path.Combine(BaseDirectory, loggingEvent.Level.Name);

                    Directory.CreateDirectory(levelPath);

                    var fullPath = Path.Combine(levelPath, fileName);
                    File.WriteAllText(fullPath, ExceptionFormatter.Format(loggingEvent));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Error(string.Format("Unable to write exception log file. {0}", loggingEvent.RenderedMessage), ex, ErrorCode.WriteFailure);
            }
        }

        protected static string ConvertToFullPath(string path)
        {
            return SystemInfo.ConvertToFullPath(path);
        }
    }
}