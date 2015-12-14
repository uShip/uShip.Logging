using System;
using System.IO;
using System.Net.Http;
using System.Web;
using JetBrains.Annotations;

namespace uShip.Logging.Extensions
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Gets the content (body) of an HttpRequest message as found on HttpContext.Current
        /// </summary>
        /// <param name="request">The request from HttpContext.Current (perhaps coming in from an HttpRequestMessage)</param>
        /// <returns>The string representation of the content (body) of the given request</returns>
        /// <exception cref="System.ArgumentNullException">request</exception>
        public static string GetContent([NotNull] this HttpRequest request)
        {
            if (null == request) throw new ArgumentNullException("request");

            return new HttpRequestWrapper(request).GetContent();
        }

        /// <summary>
        /// Gets the content (body) of an HttpRequest message as found on HttpContext.Current
        /// </summary>
        /// <param name="request">The request from HttpContext.Current (perhaps coming in from an HttpRequestMessage)</param>
        /// <returns>The string representation of the content (body) of the given request</returns>
        /// <exception cref="System.ArgumentNullException">request</exception>
        public static string GetContent([NotNull] this HttpRequestBase request)
        {
            if (null == request) throw new ArgumentNullException("request");

            var inputStream = GetStreamAfterResetting(request);

            // Check for null, and return null content if so
            if (null == inputStream
                || !inputStream.CanRead)
            {
                return null;
            }

            var startPos = inputStream.Position;
            try
            {
                inputStream.Position = 0; // reset to the start
                return new StreamContent(inputStream).ReadAsStringAsync().Result; // Use .NET 4.0 StreamContent for HttpRequestMessage
            }
            finally
            {
                // We are just peeking, so set back positions and pretend we were never here
                inputStream.Position = startPos;
            }
        }

        /// <summary>
        /// Gets the stream after resetting by calling the GetBufferedInputStream()
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">request</exception>
        private static Stream GetStreamAfterResetting([NotNull]HttpRequestBase request)
        {
            if (null == request) throw new ArgumentNullException("request");
            try
            {
                // See http://stackoverflow.com/questions/17602845/post-error-either-binaryread-form-files-or-inputstream-was-accessed-before-t
                var bufferedStream = request.GetBufferedInputStream();
                if (null == bufferedStream) return null;
                
                new StreamReader(bufferedStream).ReadToEnd();
            }
            catch (HttpException)
            {
                // let's not log in the logger to avoid potential infinite loop
            }
            return request.InputStream;
        }
    }
}