using System;
using System.Collections.Generic;
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
        public void should_format_message_properly_for_counter_source_override()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var source = "testoverride";
            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            uShipLogging.Config.CounterSourceOverride = source;
            logger.Write(GraphiteKey.Test);
            uShipLogging.Config.CounterSourceOverride = null;

            var expectedValue = String.Format("graphite.test.Test~source={0}:1|c", source);

            log.Received().Info(expectedValue);
        }

        [Test]
        public void should_format_message_properly_for_timer_source_override()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var source = "testoverride";
            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            uShipLogging.Config.TimerSourceOverride = source;
            logger.Write(GraphiteKey.Test, null, milliseconds: 100);
            uShipLogging.Config.TimerSourceOverride = null;

            var expectedValue = String.Format("Test~source={0}:100|ms", source);

            log.Received().Info(expectedValue);
        }

        [Test]
        public void should_format_message_properly_for_graphite_counter()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test);

            var hostName = Environment.MachineName;
            var expectedValue = String.Format("graphite.test.Test~source={0}:1|c", hostName);

            log.Received().Info(expectedValue);
        }

        [Test]
        public void should_format_message_properly_for_graphite_counter_with_tags()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test, tags: GetTags());

            var hostName = Environment.MachineName;
            var expectedValue = String.Format("graphite.test.Test~source={0}~key1=value1~key2=value2:1|c", hostName);

            log.Received().Info(expectedValue);
        }

        [Test]
        public void should_format_message_properly_for_graphite_counter_with_subkey()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test, "SubKey");

            var hostName = Environment.MachineName;
            var expectedValue = String.Format("graphite.test.Test.SubKey~source={0}:1|c", hostName);

            log.Received().Info(expectedValue);
        }

        [Test]
        public void should_format_message_properly_for_graphite_counter_with_subkey_with_tags()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test, "SubKey", GetTags());

            var hostName = Environment.MachineName;
            var expectedValue = String.Format("graphite.test.Test.SubKey~source={0}~key1=value1~key2=value2:1|c", hostName);

            log.Received().Info(expectedValue);
        }

        [Test]
        public void should_format_message_properly_for_graphite_timer()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test, null, milliseconds: 100);

            var hostName = Environment.MachineName;
            var expectedValue = String.Format("Test~source={0}:100|ms", hostName);

            log.Received().Info(expectedValue);
        }

        [Test]
        public void should_format_message_properly_for_graphite_timer_with_tags()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test, null, milliseconds: 100, tags: GetTags());

            var hostName = Environment.MachineName;
            var expectedValue = String.Format("Test~source={0}~key1=value1~key2=value2:100|ms", hostName);

            log.Received().Info(expectedValue);
        }

        [Test]
        public void should_format_message_properly_for_graphite_timer_with_subkey()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test, "SubKey", milliseconds: 100);

            var hostName = Environment.MachineName;
            var expectedValue = String.Format("Test.SubKey~source={0}:100|ms", hostName);

            log.Received().Info(expectedValue);
        }

        [Test]
        public void should_format_message_properly_for_graphite_timer_with_subkey_with_tags()
        {
            var logFactory = Substitute.For<LogFactory>();
            var log = Substitute.For<ILog>();
            logFactory.Create(ConfiguredLogger.Graphite).Returns(log);

            var logger = new Logger(logFactory, Substitute.For<LoggingEventDataBuilder>());
            logger.Write(GraphiteKey.Test, "SubKey", milliseconds: 100, tags: GetTags());

            var hostName = Environment.MachineName;
            var expectedValue = String.Format("Test.SubKey~source={0}~key1=value1~key2=value2:100|ms", hostName);

            log.Received().Info(expectedValue);
        }

        private Dictionary<string, string> GetTags()
        {
            var tags = new Dictionary<string, string>();
            tags.Add("key1", "value1");
            tags.Add("key2", "value2");
            return tags;
        }
    }
}