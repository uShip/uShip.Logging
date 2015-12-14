using FluentAssertions;
using NUnit.Framework;

namespace uShip.Logging.Tests
{
    [TestFixture]
    public class LoggerTests
    {
        [Test]
        public void Should_log_hello_world()
        {
            var logger = Logger.GetInstance();
            logger.Should().NotBeNull();

            logger.Message("Hello, World!").Write();
        }
    }
}