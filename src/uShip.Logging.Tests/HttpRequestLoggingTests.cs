using FluentAssertions;
using log4net.Util;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using uShip.Logging.Extensions;

namespace uShip.Logging.Tests
{
    [TestFixture]
    public class HttpRequestLoggingTests
    {
        [Test]
        public void Should_use_current_context_if_no_context_passed_in()
        {
            var logFactory = Substitute.For<LogFactory>();
            var loggingEventDataBuilder = Substitute.For<LoggingEventDataBuilder>();
            var logger = new Logger(logFactory, loggingEventDataBuilder);
            SetHttpContext();

            logger
                .Message(string.Empty)
                .Response(Substitute.For<HttpResponseBase>())
                .Write();

            var properties = GetPropertiesDictionary(loggingEventDataBuilder);
            properties["Url"].Should().Be("http://www.example.com");
        }

        [Test]
        public void Should_use_request_from_context_when_passing_in_null()
        {
            var logFactory = Substitute.For<LogFactory>();
            var loggingEventDataBuilder = Substitute.For<LoggingEventDataBuilder>();
            var logger = new Logger(logFactory, loggingEventDataBuilder);
            SetHttpContext();

            var requestBase = Substitute.For<HttpRequestBase>();
            requestBase.Url.Returns(new Uri("http://www.example.com/passed-in"));

            logger
                .Message(string.Empty)
                .Request(null)
                .Response(Substitute.For<HttpResponseBase>())
                .Write();

            var properties = GetPropertiesDictionary(loggingEventDataBuilder);
            properties["Url"].Should().Be("http://www.example.com");
        }

        [Test]
        public void Should_use_passed_in_request_if_not_null()
        {
            var logFactory = Substitute.For<LogFactory>();
            var loggingEventDataBuilder = Substitute.For<LoggingEventDataBuilder>();
            var logger = new Logger(logFactory, loggingEventDataBuilder);
            SetHttpContext();

            var requestBase = Substitute.For<HttpRequestBase>();
            requestBase.Url.Returns(new Uri("http://www.example.com/passed-in"));

            logger
                .Message(string.Empty)
                .Request(requestBase)
                .Response(Substitute.For<HttpResponseBase>())
                .Write();

            var properties = GetPropertiesDictionary(loggingEventDataBuilder);
            properties["Url"].Should().Be("http://www.example.com/passed-in");
        }

        [Test]
        public void Should_log_appropriate_request_and_response_information()
        {
            var logFactory = Substitute.For<LogFactory>();
            var loggingEventDataBuilder = Substitute.For<LoggingEventDataBuilder>();
            var logger = new Logger(logFactory, loggingEventDataBuilder);

            var requestBase = Substitute.For<HttpRequestBase>();
            requestBase.Url.Returns(new Uri("http://www.example.com"));
            requestBase.HttpMethod.Returns("GET");

            var responseBase = Substitute.For<HttpResponseBase>();
            responseBase.StatusCode.Returns(201);
            responseBase.Headers.Returns(new NameValueCollection {{"X-Test", "value"}});
            responseBase.OutputStream.Returns("body".ToStream());

            logger
                .Message(string.Empty)
                .Request(requestBase)
                .Response(responseBase)
                .Write();

            var properties = GetPropertiesDictionary(loggingEventDataBuilder);

            // Request
            properties["Url"].Should().Be("http://www.example.com");
            properties["RequestMethod"].Should().Be("GET");

            // Response
            properties["StatusCode"].Should().Be(201);
            properties["ResponseHeaders"].Should().Be("X-Test=value");
            properties["ResponseBody"].Should().Be("body");
        }

        [Test]
        public void Should_be_able_to_log_without_an_HttpContext()
        {
            var logFactory = Substitute.For<LogFactory>();
            var loggingEventDataBuilder = Substitute.For<LoggingEventDataBuilder>();
            var logger = new Logger(logFactory, loggingEventDataBuilder);

            logger
                .Message("Hello, World!")
                .Data("data-key", "data-value")
                .Write();

            var properties = GetPropertiesDictionary(loggingEventDataBuilder);

            properties["data-key"].Should().Be("data-value");
        }

        [Test]
        public void Should_include_request_body_if_OmitRequestBody_is_NOT_called_on_the_fluent_logger()
        {
            var logFactory = Substitute.For<LogFactory>();
            var loggingEventDataBuilder = Substitute.For<LoggingEventDataBuilder>();
            var logger = new Logger(logFactory, loggingEventDataBuilder);
            const string content = "This is a message";
            var requestBase = CreatePostHttpRequestBaseWithFakeContext(content);

            logger
                .Message("Hello, World!")
                .Request(requestBase)
                // .OmitRequestBody() // Do not omit request body
                .Write();

            var properties = GetPropertiesDictionary(loggingEventDataBuilder);

            // Verify the request was mocked properly
            properties["RequestMethod"].Should().Be("POST",
                "The request was not property mocked, or the RequestMethod property is no longer logged as a data tag");
            properties["RequestBody"].Should()
                .Be(content, "OmitRequestBody was not called and somehow the content body was omitted");
        }

        [Test]
        public void Should_omit_request_body_if_OmitRequestBody_is_called_on_the_fluent_logger()
        {
            var logFactory = Substitute.For<LogFactory>();
            var loggingEventDataBuilder = Substitute.For<LoggingEventDataBuilder>();
            var logger = new Logger(logFactory, loggingEventDataBuilder);
            var requestBase = CreatePostHttpRequestBaseWithFakeContext("This is a message");

            logger
                .Message("Hello, World!")
                .Request(requestBase)
                .OmitRequestBody()
                .Write();

            var properties = GetPropertiesDictionary(loggingEventDataBuilder);

            // Verify the request was mocked properly
            properties["RequestMethod"].Should().Be("POST",
                "The request was not property mocked, or the RequestMethod property is no longer logged as a data tag");
            properties["RequestBody"].Should()
                .BeNull("OmitRequestBody should omit the body of the request, but it was still somehow set");
        }

        private static HttpRequestBase CreatePostHttpRequestBaseWithFakeContext(string message)
        {
            var requestBase = Substitute.For<HttpRequestBase>();
            requestBase.Url.Returns(new Uri("http://www.example.com"));
            const string post = "POST";
            requestBase.HttpMethod.Returns(post);
            var bytes = Encoding.ASCII.GetBytes(message.ToCharArray());
            var memoryStream = new MemoryStream(bytes, 0, bytes.Length);
            requestBase.GetBufferedInputStream().Returns(memoryStream);
            requestBase.InputStream.Returns(memoryStream);
            return requestBase;
        }

        private static void SetHttpContext()
        {
            var request = new HttpRequest("filename", "http://www.example.com", string.Empty);
            var response = new HttpResponse(new StringWriter());
            var httpContext = new HttpContext(request, response);
            HttpContext.Current = httpContext;
        }

        private static PropertiesDictionary GetPropertiesDictionary(LoggingEventDataBuilder loggingEventDataBuilder)
        {
            return (PropertiesDictionary)loggingEventDataBuilder.ReceivedCalls().Single().GetArguments().Single(x => x is PropertiesDictionary);
        }
    }

}