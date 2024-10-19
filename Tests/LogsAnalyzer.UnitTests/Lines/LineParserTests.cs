using LogsAnalyzer.Lines;

namespace LogsAnalyzer.UnitTests.Lines
{
    public class LineParserTests
    {
        [Test]
        public void ShouldParseCorrectLine()
        {
            LineParser parser = new LineParser();
            var customerId = 123456;
            var pageId = 98765;
            
            var parsingResult = parser.Parse($"some time;{customerId};{pageId}");
            
            Assert.That(parsingResult.Success, Is.True);
            Assert.That(parsingResult.Value.customerId, Is.EqualTo(customerId));
            Assert.That(parsingResult.Value.pageId, Is.EqualTo(pageId));
        }

        [Test]
        [TestCase("some time;123", "Invalid line")]
        [TestCase("some time;abc123;456", "Invalid customer id:")]
        [TestCase("some time;123;-456", "Invalid page id:")]
        public void ShouldNotParseInCorrectLine(string line, string partOfErrorMessage)
        {
            LineParser parser = new LineParser();

            var parsingResult = parser.Parse(line);

            Assert.That(parsingResult.Success, Is.False);
            Assert.That(parsingResult.ErrorMessage, Is.Not.Null);
            Assert.That(parsingResult.ErrorMessage.Contains(partOfErrorMessage), Is.True);
        }
    }
}