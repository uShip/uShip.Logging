using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace uShip.Logging.LogBuilders
{
    internal static class LogDataSanitizeExtensions
    {
        private static readonly RegexReplacement[] SensitiveInfoPatterns =
            uShipLogging.Config.JsonReplacements
                .Cast<uShipLoggingConfigurationSection.JsonReplacementsElementCollection.AddElement>().Select(x => new JsonReplacement(x.Field))
                .Concat<RegexReplacement>(
                    uShipLogging.Config.UrlFormEncodedReplacements
                        .Cast<uShipLoggingConfigurationSection.JsonReplacementsElementCollection.AddElement>()
                        .Select(x => new UrlFormEncodedReplacement(x.Field)))
                .Concat(
                    uShipLogging.Config.RegexReplacements
                        .Cast<uShipLoggingConfigurationSection.JsonReplacementsElementCollection.AddElement>()
                        .Select(x => new RegexReplacement(x.Field, "************"))).ToArray();

        public static string SanitizeSensitiveInfo(this string content)
        {
            foreach (var replacement in SensitiveInfoPatterns)
            {
                content = replacement.Replace(content);
            }
            return content;
        }

        private class JsonReplacement : RegexReplacement
        {
            public JsonReplacement(string name)
                : base(string.Format(@"([""]?{0}[""]?\s*:\s*)(?:(?=[""])([""])[^""\r\n]*([""]?)|[^,""\r\n]*)", name), "$1$2****$3")
            {
            }
        }

        private class UrlFormEncodedReplacement : RegexReplacement
        {
            public UrlFormEncodedReplacement(string name)
                : base(string.Format(@"((?:^|&){0}=)(?:[^&]+)($|&)", name), "$1****$2")
            {
            }
        }

        private class RegexReplacement
        {
            private readonly string _replacement;
            private readonly Regex _regex;
            public RegexReplacement(string regex, string replacement)
            {
                _replacement = replacement;
                _regex = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            public string Replace(string content)
            {
                return _regex.Replace(content, _replacement);
            }
        }
        
        private static readonly RegexReplacement[] ViewStateReplacements = 
        {
            new UrlFormEncodedReplacement("__VIEWSTATE"),
            new UrlFormEncodedReplacement("__EVENTVALIDATION")
        };
        public static string RemoveViewState(this string content)
        {
            foreach (var replacement in ViewStateReplacements)
            {
                content = replacement.Replace(content);
            }
            return content;
        }

        public static string CleanQueryString(this string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return string.Empty;

            query = HttpUtility.UrlDecode(query);
            if (!string.IsNullOrEmpty(query))
            {
                query = query.Replace(":8080", string.Empty);
                query = query.Replace(":80", string.Empty);

            }
            return query;
        }
    }
}