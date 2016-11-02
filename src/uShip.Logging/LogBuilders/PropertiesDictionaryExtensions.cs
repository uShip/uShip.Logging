using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static IDictionary NewSanitizedDictionary(this IDictionary data)
        {
            if (data == null)
            {
                return null;
            }

            IDictionary sanitizedData = new Dictionary<String, object>();

            foreach (var key in data.Keys)
            {
                if (data[key] == null)
                {
                    continue;
                }
                if (data[key] is string)
                {
                    sanitizedData[key] = Sanitize(data[key].ToString());
                }
                else // expect the object to have been sanitized already
                {
                    sanitizedData[key] = data[key];
                }
            }

            return sanitizedData;
        }

        public static void Set(this PropertiesDictionary dictionary, string key, object value)
        {
            if (value is string)
            {
                dictionary[key] = Sanitize(value.ToString());
            }
            else if (value is IDictionary) // Sql Params
            {
                dictionary[key] = ((IDictionary) dictionary[key]).NewSanitizedDictionary();
            }
            else // assume the object has been sanitized already ie LoggableException
            {
                dictionary[key] = value;
            }  
        }

        internal static void SafeSetProp(this PropertiesDictionary props, string propKey, Func<string> valueGetter)
        {
            try
            {
                props.Set(propKey, valueGetter());
            }
            catch (Exception ex)
            {
                props.Set(propKey, String.Format("Failed setting {0} key in logger because {1}", propKey, ex.Message));
            }
        }
    }
}