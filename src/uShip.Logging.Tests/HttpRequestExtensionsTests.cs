using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System;
using System.IO;
using System.Web;
using uShip.Logging.Extensions;

namespace uShip.Logging.Tests
{
    [TestFixture]
    public class HttpRequestExtensionsTests
    {
        private static HttpRequestBase GetMockRequestGiven(Stream content = null)
        {
            var request = Substitute.For<HttpRequestBase>();
            request.GetBufferedInputStream().Returns(content);
            request.InputStream.Returns(content);
            return request;
        }

        public void PerformAssertionThatGivenContentMatchesExtractedContent(
            Stream givenContent,
            string expectedContent,
            string message,
            Action<HttpRequestBase> actionToBeFormedOnRequest = null)
        {
            // Arrange
            var request = GetMockRequestGiven(givenContent);
            if (null != actionToBeFormedOnRequest)
            {
                actionToBeFormedOnRequest(request);
            }

            // Act
            var extractedContent = request.GetContent();

            // Assert
            extractedContent.Should().Be(expectedContent, message);
        }

        [TestCase("")]
        [TestCase("some string")]
        public void When_getting_content_for_the_first_time_should_get_whatever_was_originally_set_as_the_content(
            string content)
        {
            const string errorMessage =
                "the content extracted from the request message did not match that which was originally put on the InputStream";
            PerformAssertionThatGivenContentMatchesExtractedContent(content.ToStream(), content, errorMessage);
        }

        [TestCase("")]
        [TestCase("some string")]
        public void When_getting_content_after_stream_has_already_been_read_should_still_be_able_to_get_content(
            string content)
        {
            // Arrange
            const string errorMessage =
                "the content extracted from the request message did not match that which was originally put on the InputStream";
            var stream = content.ToStream();
            var reader = new StreamReader(stream);
            reader.ReadToEnd();
            PerformAssertionThatGivenContentMatchesExtractedContent(stream, content, errorMessage);
        }

        [Test]
        public void When_getting_content_null_should_get_empty_string_as_the_content()
        {
            const string errorMessage =
                "the content extracted from the request message did not match that which was originally put on the InputStream";
            PerformAssertionThatGivenContentMatchesExtractedContent(new MemoryStream(), "", errorMessage);
        }
    }
}