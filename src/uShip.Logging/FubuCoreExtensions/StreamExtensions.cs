using System.IO;

namespace uShip.Logging.Extensions
{
    internal static class StreamExtensions
    {
        internal static string ReadAllText(this Stream stream)
        {
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}