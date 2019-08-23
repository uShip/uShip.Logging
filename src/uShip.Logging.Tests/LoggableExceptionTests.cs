using System;
using NUnit.Framework;
using uShip.Logging.LogBuilders;

namespace uShip.Logging.Tests
{
    [TestFixture]
    public class LoggableExceptionTests
    {
        [Test]
        public void Should_obfuscate_passwords_in_exception_message()
        {
            // arrange
            var exception = new Exception("A potentially dangerous Request.Form value was detected from the client (_ctl0:ContentBody:conSignIn:txtExistingPassword=\"Aa9W7+&#+%@\").");

            // act
            var classUnderTest = new LoggableException(exception);

            // assert
            Assert.AreEqual(
                "A potentially dangerous Request.Form value was detected from the client (_ctl0:ContentBody:conSignIn:txtExisting************",
                classUnderTest.Message);
        }
    }
}