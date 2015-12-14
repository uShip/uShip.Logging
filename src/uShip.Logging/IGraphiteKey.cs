namespace uShip.Logging
{
    public interface IGraphiteKey
    {
        /// <summary>
        /// The metric to log to Graphite
        /// </summary>
        string Key { get; }
    }
}