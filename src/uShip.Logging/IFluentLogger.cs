using System;
using System.Diagnostics.Contracts;

namespace uShip.Logging
{
    public interface IFluentLogger
    {
        /// <summary>
        /// Fluent interface for setting a message on a log.
        /// 
        /// The message text should not be variable. It should be unchanged per instance.
        /// </summary>
        /// <param name="message">The message to be logged. The message should not be variable. It should be unchanged per instance.</param>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Message(string message);

        /// <summary>
        /// Fluent interface for setting an exception on a log.
        /// </summary>
        /// <param name="exception">The exception to be logged.</param>
        /// <returns></returns>
        [Pure]
        [JetBrains.Annotations.Pure]
        IFluentLoggerWriter Exception(Exception exception);
    }
}