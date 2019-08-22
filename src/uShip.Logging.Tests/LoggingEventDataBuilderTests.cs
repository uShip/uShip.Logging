using System;
using System.Diagnostics;
using log4net.Util;
using NUnit.Framework;

namespace uShip.Logging.Tests
{
    [TestFixture]
    public class LoggingEventDataBuilderTests
    {
        [Test]
        public void Should_sanitize_passwords_in_message()
        {
            // arrange
            const string messageWithPassword = "A potentially dangerous Request.Form value was detected from the client (_ctl0:ContentBody:conSignIn:txtExistingPassword=\"Aa9W7+&#+%@\").";
            var frame = new StackTrace().GetFrame(0);
            var classUnderTest = new LoggingEventDataBuilder();

            // act
            var eventData = classUnderTest.Build(frame, Severity.Info, messageWithPassword, null, new PropertiesDictionary());

            // assert
            Assert.AreEqual("A potentially dangerous Request.Form value was detected from the client (_ctl0:ContentBody:conSignIn:txtExisting************", eventData.Message);
        }

        [Test]
        public void Should_sanitize_passwords_in_exception_message()
        {
            // arrange
            const string messageWithPassword = "A potentially dangerous Request.Form value was detected from the client (_ctl0:ContentBody:conSignIn:txtExistingPassword=\"Aa9W7+&#+%@\").";
            var frame = new StackTrace().GetFrame(0);
            var classUnderTest = new LoggingEventDataBuilder();

            // act
            var eventData = classUnderTest.Build(frame, Severity.Info, null, new Exception(messageWithPassword), new PropertiesDictionary());

            // assert
            Assert.AreEqual("A potentially dangerous Request.Form value was detected from the client (_ctl0:ContentBody:conSignIn:txtExisting************", eventData.Message);
        }
    }
}