using System.Diagnostics;

namespace uShip.Logging.Extensions
{
    internal static class StackFrameExtensions
    {
        internal static string ToDescription(this StackFrame frame)
        {
            return string.Format("{0}.{1}(), {2} line {3}",
                frame.GetMethod().DeclaringType.FullName,
                frame.GetMethod().Name,
                frame.GetFileName(),
                frame.GetFileLineNumber());
        }
    }
}