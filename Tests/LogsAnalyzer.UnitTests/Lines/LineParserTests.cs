using LogsAnalyzer.Lines;

namespace LogsAnalyzer.UnitTests.Lines
{
    public class LineParserTests
    {
        [Test]
        public void ShouldParseCorrectLineShortVersion()
        {
            LineParser parser = new LineParser(";");
            var customerId = 123456;
            var pageId = 98765;
            
            var parsingResult = parser.ParseShort($"some time;{customerId};{pageId}");
            
            Assert.That(parsingResult.Success, Is.True);
            Assert.That(parsingResult.Value.customerId, Is.EqualTo(customerId));
            Assert.That(parsingResult.Value.pageId, Is.EqualTo(pageId));
        }

        [Test]
        [TestCase("some time;123", "Invalid line")]
        [TestCase("some time;abc123;456", "Invalid customer id:")]
        [TestCase("some time;123;-456", "Invalid page id:")]
        public void ShouldNotParseInCorrectLineShortVersion(string line, string partOfErrorMessage)
        {
            LineParser parser = new LineParser(";");

            var parsingResult = parser.ParseShort(line);

            Assert.That(parsingResult.Success, Is.False);
            Assert.That(parsingResult.ErrorMessage, Is.Not.Null);
            Assert.That(parsingResult.ErrorMessage.Contains(partOfErrorMessage), Is.True);
        }

        [Test]
        public void ShouldParseCorrectLineFullVersion()
        {
            LineParser parser = new LineParser(";");
            var customerId = 123456;
            var pageId = 98765;

            var parsingResult = parser.ParseShort($"2024-10-19T22:19:31;{customerId};{pageId}");

            Assert.That(parsingResult.Success, Is.True);
            Assert.That(parsingResult.Value.customerId, Is.EqualTo(customerId));
            Assert.That(parsingResult.Value.pageId, Is.EqualTo(pageId));
        }

        [Test]
        [TestCase("2024-10-19T22:19:31;123", "Invalid line")]
        [TestCase("some time;abc123;456", "Invalid time mark:")]
        [TestCase("2024-10-19T22:19:31;abc123;456", "Invalid customer id:")]
        [TestCase("2024-10-19T22:19:31;123;-456", "Invalid page id:")]
        public void ShouldNotParseInCorrectLineFullVersion(string line, string partOfErrorMessage)
        {
            LineParser parser = new LineParser(";");

            var parsingResult = parser.Parse(line);

            Assert.That(parsingResult.Success, Is.False);
            Assert.That(parsingResult.ErrorMessage, Is.Not.Null);
            Assert.That(parsingResult.ErrorMessage.Contains(partOfErrorMessage), Is.True);
        }
    }
}