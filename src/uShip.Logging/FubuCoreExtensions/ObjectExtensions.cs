using System;

namespace uShip.Logging.Extensions
{
    internal static class ObjectExtensions
    {
        /// <remarks>https://github.com/DarthFubuMVC/fubucore/blob/master/src/FubuCore/BasicExtensions.cs</remarks>
        internal static TOut IfNotNull<TTarget, TOut>(this TTarget target, Func<TTarget, TOut> valueFunc)
            where TTarget : class
        {
            return target == null ? default(TOut) : valueFunc(target);
        }
    }
}