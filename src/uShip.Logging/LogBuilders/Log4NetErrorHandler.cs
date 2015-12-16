using FubuCore;
using log4net.Core;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace uShip.Logging.LogBuilders
{
    public class Log4NetErrorHandler : IErrorHandler
    {
        private readonly IPAddress _remoteAddress;
        private readonly int _remotePort;

        public Log4NetErrorHandler(IPAddress remoteAddress, int remotePort)
        {
            _remoteAddress = remoteAddress;
            _remotePort = remotePort;
        }

        public void Error(string message, Exception e, ErrorCode errorCode)
        {
            ErrorJson(JsonConvert.SerializeObject(new
            {
                Message = message,
                Exception = e.IfNotNull(x => x.ToString()),
                ErrorCode = errorCode.ToString(),
                ErrorHandler = "Log4NetErrorHandler"
            }));
        }

        public void Error(string message, Exception e)
        {
            ErrorJson(JsonConvert.SerializeObject(new
            {
                Message = message,
                Exception = e.IfNotNull(x => x.ToString()),
                ErrorHandler = "Log4NetErrorHandler"
            }));
        }

        public void Error(string message)
        {
            ErrorJson(JsonConvert.SerializeObject(new
            {
                Message = message,
                ErrorHandler = "Log4NetErrorHandler"
            }));
        }

        private void ErrorJson(string message)
        {
            var udpClient = new UdpClient();
            try
            {
                udpClient.Connect(_remoteAddress, _remotePort);

                var messageBytes = Encoding.UTF8.GetBytes(message);

                udpClient.Send(messageBytes, messageBytes.Length);
            }
            catch (Exception e)
            {
                // uh... we don't have a backup for our backup yet.
                Console.WriteLine("Something really bad is happening when udp fails: " + e);
            }
            finally
            {
                udpClient.Close();
            }
        }
    }
}