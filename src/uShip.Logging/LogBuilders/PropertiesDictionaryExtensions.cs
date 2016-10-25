using System;
using log4net.Util;
using uShip.Logging.Extensions;

namespace uShip.Logging.LogBuilders
{
    public static class PropertiesDictionaryExtensions
    {
        private static string Sanitize(string input)
        {
            return input.IfNotNull(x => x.SanitizeSensitiveInfo().RemoveViewState());
        }

        public static void Set(this PropertiesDictionary dictionary, string key, object value)
        {
            if (value is string)
            {
                dictionary[key] = Sanitize(value.ToString());
                return;
            }
            dictionary[key] = value;
        }

        internal static void SafeSetProp(this PropertiesDictionary props, string propKey, Func<string> valueGetter)
        {
            try
            {
                props.Set(propKey, valueGetter());
            }
            catch (Exception ex)
            {
                props.Set(propKey, string.Format("Failed setting {0} key in logger because {1}", propKey, ex.Message));
            }
        }
    }
}