using FluentAssertions;
using NUnit.Framework;

namespace uShip.Logging.Tests
{
    [TestFixture]
    public class ProgramTests
    {
        [Test]
        public void Message_should_be_hello_world()
        {
            Program.Message.Should().Be("Hello, World!");
        }
    }
}
