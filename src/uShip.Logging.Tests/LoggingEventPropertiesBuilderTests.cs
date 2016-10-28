using NUnit.Framework;
using System;
using uShip.Logging.LogBuilders;
using uShip.Logging.Tests.Common;

namespace uShip.Logging.Tests
{
    [TestFixture]
    public class LoggingEventPropertiesBuilderTests
    {
        [Test]
        public void Should_Have_Different_Origin_When_First_Target_Stack_Trace_Is_Different_But_Same_Exception()
        {
            var exceptionA = GetSystemParseExceptionA();
            var loggingPropertiesA = new LoggingEventPropertiesBuilder()
                    .WithException(exceptionA)
                    .WithUniqueOrigin(string.Empty, exceptionA)
                    .Build();

            var exceptionB = GetSystemParseExceptionB();
            var loggingPropertiesB = new LoggingEventPropertiesBuilder()
                    .WithException(exceptionB)
                    .WithUniqueOrigin(string.Empty, exceptionB)
                    .Build();

            Assert.AreNotEqual(loggingPropertiesA["Origin"].ToString(), loggingPropertiesB["Origin"].ToString());
        }

        [Test]
        public void Should_sanitize_passwords()
        {
            var loggingProperties = new LoggingEventPropertiesBuilder()
                    .Build();

            loggingProperties.Set("someInfo", "\"EmailAddress\\\":\\\"hello@meadowshotel.com\\\",\\\"Password\\\":\\\"meadows2014\\\"");
            loggingProperties.Set("moreInfo", "\"emailAddress\":\"hello@meadowshotel.com\",\"password\":\"meadows2014\"");

            Assert.AreEqual(loggingProperties["someInfo"], "\"EmailAddress\\\":\\\"hello@meadowshotel.com\\\",\\\"************\\\":\\\"****\\\"");
            Assert.AreEqual(loggingProperties["moreInfo"], "\"emailAddress\":\"hello@meadowshotel.com\",\"************\":\"****\"");
        }

        [Test]
        public void Should_sanitize_creditCard()
        {
            var loggingProperties = new LoggingEventPropertiesBuilder()
                    .Build();

            loggingProperties.Set("someInfo", "\"EmailAddress\\\":\\\"hello@meadowshotel.com\\\",\\\"creditCard\\\":\\\"1234123412341234\\\"");
            loggingProperties.Set("moreInfo", "\"emailAddress\":\"hello@meadowshotel.com\",\"creditCard\":\"1234123412341234\"");

            Assert.AreEqual(loggingProperties["someInfo"], "\"EmailAddress\\\":\\\"hello@meadowshotel.com\\\",\\\"************\\\":\\\"****\\\"");
            Assert.AreEqual(loggingProperties["moreInfo"], "\"emailAddress\":\"hello@meadowshotel.com\",\"************\":\"****\"");
        }

        [Test]
        public void Should_Have_Different_Origin_When_First_Target_Stack_Trace_Is_Same_But_Exceptions_Differ()
        {
            var exceptionA = GetVariableSystemException(parseException: true);
            var loggingPropertiesA = new LoggingEventPropertiesBuilder()
                    .WithException(exceptionA)
                    .WithUniqueOrigin(string.Empty, exceptionA)
                    .Build();

            var exceptionB = GetVariableSystemException(argumentException: true);
            var loggingPropertiesB = new LoggingEventPropertiesBuilder()
                    .WithException(exceptionB)
                    .WithUniqueOrigin(string.Empty, exceptionB)
                    .Build();

            Assert.AreNotEqual(loggingPropertiesA["Origin"].ToString(), loggingPropertiesB["Origin"].ToString());
        }

        [Test]
        public void Should_Have_Different_Origin_When_Message_Is_Different()
        {
            var exception = GetSystemParseExceptionA();
            var loggingPropertiesA = new LoggingEventPropertiesBuilder()
                    .WithException(exception)
                    .WithUniqueOrigin(string.Empty, exception)
                    .Build();

            var loggingPropertiesB = new LoggingEventPropertiesBuilder()
                    .WithException(exception)
                    .WithUniqueOrigin("This is a custom message", exception)
                    .Build();

            Assert.AreNotEqual(loggingPropertiesA["Origin"].ToString(), loggingPropertiesB["Origin"].ToString());
        }

        [Test]
        public void Should_Have_Same_Origin_When_Custom_Messages_Are_The_Same_But_Exceptions_Are_Different()
        {
            var exceptionA = GetVariableSystemException(parseException: true);
            var exceptionB = GetVariableSystemException(argumentException: true);
            var loggingPropertiesA = new LoggingEventPropertiesBuilder()
                    .WithException(exceptionA)
                    .WithUniqueOrigin("This is a custom message", exceptionA)
                    .Build();
            var loggingPropertiesB = new LoggingEventPropertiesBuilder()
                    .WithException(exceptionB)
                    .WithUniqueOrigin("This is a custom message", exceptionB)
                    .Build();
            Assert.AreEqual(loggingPropertiesA["Origin"].ToString(), loggingPropertiesB["Origin"].ToString());
        }

        [Test]
        public void Should_Have_Same_Origin_When_Exception_Thrown_From_And_Not_From_Excluded_Method()
        {
            var exceptionA = GetVariableSystemException(parseException: true);
            var exceptionB = GetVariableSystemException(parseException: true, throwViaMethod: true);
            var loggingPropertiesA = new LoggingEventPropertiesBuilder()
                    .WithException(exceptionA)
                    .WithUniqueOrigin(string.Empty, exceptionA)
                    .Build();
            var loggingPropertiesB = new LoggingEventPropertiesBuilder()
                    .WithException(exceptionB)
                    .WithUniqueOrigin(string.Empty, exceptionB)
                    .Build();
            Assert.AreEqual(loggingPropertiesA["Origin"].ToString(), loggingPropertiesB["Origin"].ToString());
        }

        private Exception GetSystemParseExceptionA()
        {
            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // ReSharper disable once UnusedVariable
                var x = int.Parse(null);
            }
            catch (Exception ex)
            {
                return ex;
            }
            throw new Exception("This shouldn't occur. You changed this test to not throw.");
        }

        private Exception GetSystemParseExceptionB()
        {
            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // ReSharper disable once UnusedVariable
                var x = int.Parse(null);
            }
            catch (Exception ex)
            {
                return ex;
            }
            throw new Exception("This shouldn't occur. You changed this test to not throw.");
        }

        private Exception GetVariableSystemException(bool parseException = false, bool argumentException = false, bool throwViaMethod = false)
        {
            if (throwViaMethod)
            {
                try
                {
                    ExceptionThrower.ThrowException();
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }

            try
            {
                
                if (parseException)
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    // ReSharper disable once AssignNullToNotNullAttribute
                    int.Parse(null);
                }
                if (argumentException)
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    Enum.GetValues(typeof (string));
                }
            }
            catch (Exception ex)
            {
                return ex;
            }

            throw new Exception("This shouldn't occur. You changed this test to not throw.");
        }
    }

    namespace Common
    {
        public class ExceptionThrower
        {
            public static void ThrowException()
            {
                int.Parse(null);
            }
        }
    }
}
