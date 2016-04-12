using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web;

namespace uShip.Logging
{
    public interface IFluentLoggerWriter : IFluentLogger
    {
        /// <summary>
        /// Fluent interface for setting severity to fatal.
        /// </summary>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Fatal();

        /// <summary>
        /// Fluent interface for setting severity to error.
        /// </summary>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Error();

        /// <summary>
        /// Fluent interface for setting severity to debug.
        /// </summary>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Debug();

        /// <summary>
        /// Fluent interface for setting severity to warn.
        /// </summary>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Warn();

        /// <summary>
        /// Fluent interface for setting severity to info.
        /// </summary>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Info();

        /// <summary>
        /// Fluent interface for adding data to a log.
        /// </summary>
        /// <param name="name">The key to be logged.</param>
        /// <param name="value">The value to be logged.</param>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Data(string name, string value);

        /// <summary>
        /// Fluent interface for adding data to a log.
        /// </summary>
        /// <param name="name">The key to be logged.</param>
        /// <param name="value">The value to be logged.</param>
        /// <remarks>This calls .ToString() on the boolean value for you</remarks>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Data(string name, bool value);

        /// <summary>
        /// Fluent interface for adding numeric data to a log. (appends "+Decimal")
        /// </summary>
        /// <param name="name">The key to be logged.</param>
        /// <param name="value">The value to be logged.</param>
        /// <remarks>This appends "+Decimal" to your chosen name to avoid any data conflicts.  Be aware of this when searching</remarks>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Data(string name, decimal value);

        /// <summary>
        /// Fluent interface for adding numeric data to a log. (appends "+Int32")
        /// </summary>
        /// <param name="name">The key to be logged.</param>
        /// <param name="value">The value to be logged.</param>
        /// <remarks>This appends "+Int32" to your chosen name to avoid any data conflicts.  Be aware of this when searching</remarks>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Data(string name, int value);

        /// <summary>
        /// Fluent interface for adding numeric data to a log. (appends "+Int64")
        /// </summary>
        /// <param name="name">The key to be logged.</param>
        /// <param name="value">The value to be logged.</param>
        /// <remarks>This appends "+Int64" to your chosen name to avoid any data conflicts.  Be aware of this when searching</remarks>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Data(string name, long value);

        /// <summary>
        /// Fluent interface for tagging the log.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Tag(params string[] tags);

        /// <summary>
        /// Fluent interface for marking as a sql exception.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Sql(string sql, IEnumerable<KeyValuePair<string, object>> parameters);

        /// <summary>
        /// Fluent interface for adding the request to the log.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <remarks>Replaces the default <see cref="HttpContext.Request"/> from <see cref="HttpContext.Current"/> if not null.</remarks>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Request(HttpRequestBase request);

        /// <summary>
        /// Fluent interface for adding the response to the log.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <remarks>Replaces the default <see cref="HttpContext.Response"/> from <see cref="HttpContext.Current"/> if not null.</remarks>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Response(HttpResponseBase response);

        /// <summary>
        /// Fluent interface for removes bodies the request to the log.
        /// </summary>
        /// <remarks>Removes body from any <see cref="HttpContext.Request"/> body from <see cref="HttpContext.Current"/> (if not null) from the current log.</remarks>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter OmitRequestBody();


        /// <summary>
        /// Fluent interface for writing the log. This commits the collected data to a log write.
        /// </summary>
        void Write();

    }
}