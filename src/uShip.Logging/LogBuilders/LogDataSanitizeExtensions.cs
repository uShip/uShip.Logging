using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace uShip.Logging.LogBuilders
{
    internal static class LogDataSanitizeExtensions
    {

        public static readonly string ScrubbedConstant = "****";
        public static readonly IReadOnlyList<string> SensitiveFieldNames =
            uShipLogging.Config.JsonReplacements
                .Cast<uShipLoggingConfigurationSection.JsonReplacementsElementCollection.AddElement>()
                .Select(x => x.Field)
                .Concat<string>(
                    uShipLogging.Config.UrlFormEncodedReplacements
                        .Cast<uShipLoggingConfigurationSection.UrlFormEncodedReplacementsElementCollection.AddElement>()
                        .Select(x => x.Field))
                .Concat<string>(
                    uShipLogging.Config.RegexReplacements
                        .Cast<uShipLoggingConfigurationSection.RegexReplacementsElementCollection.AddElement>()
                        .Select(x => x.Field))
                .OrderBy(str => str)
                .Distinct()
                .ToList();

        private static readonly RegexReplacement[] SensitiveInfoPatterns =
            uShipLogging.Config.JsonReplacements
                .Cast<uShipLoggingConfigurationSection.JsonReplacementsElementCollection.AddElement>().Select(x => new JsonReplacement(x.Field))
                .Concat<RegexReplacement>(
                    uShipLogging.Config.UrlFormEncodedReplacements
                        .Cast<uShipLoggingConfigurationSection.UrlFormEncodedReplacementsElementCollection.AddElement>()
                        .Select(x => new UrlFormEncodedReplacement(x.Field)))
                .Concat(
                    uShipLogging.Config.RegexReplacements
                        .Cast<uShipLoggingConfigurationSection.RegexReplacementsElementCollection.AddElement>()
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
            private const string JsonPattern = @"([\\]?[""]?{0}(""|\\"")?\s*:\s*)(?:(?=[\\]?[""])([\\]?[""])[^""\r\n]*([""]?)|[^,""\r\n]*)";
            private static readonly string JsonValueObfuscation = string.Format("$1$2{0}$3", ScrubbedConstant);

            public JsonReplacement(string name)
                : base(string.Format(JsonPattern, name), JsonValueObfuscation)
            {
            }
        }

        private class UrlFormEncodedReplacement : RegexReplacement
        {
            private const string UrlFormEncodedPattern = @"((?:^|&){0}=)(?:[^&]+)($|&)";
            private const string UrlFormEncodedObfuscation = "$1****$2";

            public UrlFormEncodedReplacement(string name)
                : base(string.Format(UrlFormEncodedPattern, name), UrlFormEncodedObfuscation)
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