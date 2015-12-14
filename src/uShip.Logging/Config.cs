namespace uShip.Logging
{
    /// <summary>
    /// TODO: Remove this once we have config transforms working.
    /// </summary>
    internal static class Config
    {
        public const string MinimalDataLogMessage = "MinimalDataLogMessage";
        public const string GraphiteMetricPath = "GraphiteMetricPath.";

        public static string[] JsonReplacements = {"passwordFieldGoesHere", "creditCardFieldGoesHere"};
        public static string[] UrlFormEncodedReplacement = { "passwordFieldGoesHere", "creditCardFieldGoesHere" };
        public static string[] RegexReplacements = { "regexReplacementGoesHere" };
    }
}