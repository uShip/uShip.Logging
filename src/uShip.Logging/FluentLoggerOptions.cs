namespace uShip.Logging
{
    public class FluentLoggerOptions
    {
        public FluentLoggerOptions()
        {
            TruncateRequestBodyCharactersTo = 1000;
            TruncateResponseBodyCharactersTo = 1000;
        }

        /// <summary>
        /// The length to truncate the request body to, defaulted to 1000 because UDP on our network fails.  Set to null to disable truncation.
        /// </summary>
        public int? TruncateRequestBodyCharactersTo { get; set; }

        /// <summary>
        /// The length to truncate the response body to, defaulted to 1000 because UDP on our network fails.  Set to null to disable truncation.
        /// </summary>
        public int? TruncateResponseBodyCharactersTo { get; set; }

        internal void Merge(FluentLoggerOptions newOptions)
        {
            TruncateRequestBodyCharactersTo = newOptions.TruncateRequestBodyCharactersTo;
            TruncateResponseBodyCharactersTo = newOptions.TruncateResponseBodyCharactersTo;
        }
    }
}