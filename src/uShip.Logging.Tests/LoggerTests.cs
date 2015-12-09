using FluentAssertions;
using NUnit.Framework;

namespace uShip.Logging.Tests
{
    [TestFixture]
    public class LoggerTests
    {
        [Test]
        public void Should_pass()
        {
            ILogger logger = null;
            logger.Should().BeNull();
        }
    }
}