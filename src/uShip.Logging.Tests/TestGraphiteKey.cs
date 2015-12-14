namespace uShip.Logging.Tests
{
    public class GraphiteKey : IGraphiteKey
    {
        public static GraphiteKey Test = new GraphiteKey("Test");

        private readonly string _key;

        private GraphiteKey(string key)
        {
            _key = key;
        }

        public string Key { get { return _key; } }
    }
}