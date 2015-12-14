using FluentAssertions;
using log4net;
using NSubstitute;
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

        [Test]
        public void should_get_Logstash_logger()
        {
            var logFactory = Substitute.For<LogFactory>();

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logFactory.Received().Create(ConfiguredLogger.Logstash);
        }

        [Test]
        public void should_get_Graphite_logger()
        {
            var logFactory = Substitute.For<LogFactory>();

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logFactory.Received().Create(ConfiguredLogger.Graphite);
        }

        [Test]
        public void should_format_message_properly_for_graphite_counter()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test);

            log.Received().Info("GraphiteMetricPath.Test:1|c");
        }

        [Test]
        public void should_format_message_properly_for_graphite_counter_with_subkey()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test, "SubKey");

            log.Received().Info("GraphiteMetricPath.Test.SubKey:1|c");
        }

        [Test]
        public void should_format_message_properly_for_graphite_timer()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test, null, milliseconds: 100);

            log.Received().Info("Test:100|ms");
        }

        [Test]
        public void should_format_message_properly_for_graphite_timer_with_subkey()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test, "SubKey", milliseconds: 100);

            log.Received().Info("Test.SubKey:100|ms");
        }
    }
}