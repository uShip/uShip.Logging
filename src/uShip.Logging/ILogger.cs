using System;
using System.Collections.Generic;

namespace uShip.Logging
{
    public interface ILogger : IFluentLogger
    {
        /// <summary>
        /// Writes a counter (value of 1) to graphite.
        /// </summary>
        /// <param name="key">The graphite key that's being logged</param>
        /// <param name="subKey">The subkey for this log. If this is null, no subkey will be used.</param>
        /// <param name="tags"></param>
        void Write(IGraphiteKey key, string subKey = null, Dictionary<string, string> tags = null);

        /// <summary>
        /// Writes a counter (caller defined value) to graphite.
        /// </summary>
        /// <param name="count">the value of the counter</param>
        /// <param name="key">The graphite key that's being logged</param>
        /// <param name="subKey">The subkey for this log. If this is null, no subkey will be used.</param>
        /// <param name="tags"></param>
        void Write(int count, IGraphiteKey key, string subKey = null, Dictionary<string, string> tags = null);

        /// <summary>
        /// Writes a timer to graphite without a subkey
        /// </summary>
        /// <param name="key">The graphite key that's being logged</param>
        /// <param name="milliseconds">The number of milliseconds you're logging for a timed graphite log.</param>
        /// <param name="tags"></param>
        void Write(IGraphiteKey key, long milliseconds, Dictionary<string, string> tags = null);

        /// <summary>
        /// Writes a timer to graphite with a subkey.
        /// </summary>
        /// <param name="key">The graphite key that's being logged</param>
        /// <param name="subKey">The subkey for this log. If this is null, no subkey will be used.</param>
        /// <param name="milliseconds">The number of milliseconds you're logging for a timed graphite log.</param>
        /// <param name="tags"></param>
        void Write(IGraphiteKey key, string subKey, long milliseconds, Dictionary<string, string> tags = null);

        /// <summary>
        /// Writes raw json data to a minimal Logstash bucket
        /// </summary>
        /// <param name="jsonData">A raw json object</param>
        /// <param name="severity">Severity of the data</param>
        void WriteMinimalDataLog(string jsonData, Severity? severity = null);

        /// <summary>
        /// Writes a timer metric to graphite
        /// </summary>
        /// <param name="key">The graphite key that's being logged</param>
        /// <param name="span">The length of time you're logging for a timed graphite log.</param>
        /// <param name="tags"></param>
        void Timer(string key, TimeSpan span, Dictionary<string, string> tags = null);

        /// <summary>
        /// Writes a timer metric to graphite
        /// </summary>
        /// <param name="key">The graphite key that's being logged</param>
        /// <param name="subkey">The subkey for this log. If this is null, no subkey will be used.</param>
        /// <param name="span">The length of time you're logging for a timed graphite log.</param>
        /// <param name="tags"></param>
        void Timer(string key, string subkey, TimeSpan span, Dictionary<string, string> tags = null);

        /// <summary>
        /// Writes a counter metric to graphite
        /// </summary>
        /// <param name="key">The graphite key that's being logged</param>
        /// <param name="count">The count you're logging for a count graphite log.</param>
        /// <param name="tags"></param>
        void Count(string key, int count, Dictionary<string, string> tags = null);

        /// <summary>
        /// Writes a counter metric to graphite
        /// </summary>
        /// <param name="key">The graphite key that's being logged</param>
        /// <param name="subkey">The subkey for this log. If this is null, no subkey will be used.</param>
        /// <param name="count">The count you're logging for a count graphite log.</param>
        /// <param name="tags"></param>
        void Count(string key, string subkey, int count, Dictionary<string, string> tags = null);
    }
}