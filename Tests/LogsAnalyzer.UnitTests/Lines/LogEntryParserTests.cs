using LogsAnalyzer.LogEntries;

namespace LogsAnalyzer.UnitTests.Lines
{
    public class LogEntryParserTests
    {
        //Violation of DRY, only because this is just experiments
        #region Parse strings

        [Test]
        public void ShouldParseCorrectLineShortVersion()
        {
            LogEntryParser parser = new LogEntryParser(";");
            var customerId = 123456;
            var pageId = 98765;
            
            var parsingResult = parser.ParseShort($"some time;{customerId};{pageId}");
            
            Assert.That(parsingResult.Success, Is.True);
            Assert.That(parsingResult.Value.CustomerId, Is.EqualTo(customerId));
            Assert.That(parsingResult.Value.PageId, Is.EqualTo(pageId));
        }

        [Test]
        [TestCase("some time;123", "Invalid line")]
        [TestCase("some time;abc123;456", "Invalid customer id:")]
        [TestCase("some time;123;-456", "Invalid page id:")]
        public void ShouldNotParseInCorrectLineShortVersion(string line, string partOfErrorMessage)
        {
            LogEntryParser parser = new LogEntryParser(";");

            var parsingResult = parser.ParseShort(line);

            Assert.That(parsingResult.Success, Is.False);
            Assert.That(parsingResult.ErrorMessage, Is.Not.Null);
            Assert.That(parsingResult.ErrorMessage.Contains(partOfErrorMessage), Is.True);
        }

        [Test]
        public void ShouldParseCorrectLineFullVersion()
        {
            LogEntryParser parser = new LogEntryParser(";");
            var customerId = 123456;
            var pageId = 98765;

            var parsingResult = parser.ParseShort($"2024-10-19T22:19:31;{customerId};{pageId}");

            Assert.That(parsingResult.Success, Is.True);
            Assert.That(parsingResult.Value.CustomerId, Is.EqualTo(customerId));
            Assert.That(parsingResult.Value.PageId, Is.EqualTo(pageId));
        }

        [Test]
        [TestCase("2024-10-19T22:19:31;123", "Invalid line")]
        [TestCase("some time;abc123;456", "Invalid time mark:")]
        [TestCase("2024-10-19T22:19:31;abc123;456", "Invalid customer id:")]
        [TestCase("2024-10-19T22:19:31;123;-456", "Invalid page id:")]
        public void ShouldNotParseInCorrectLineFullVersion(string line, string partOfErrorMessage)
        {
            LogEntryParser parser = new LogEntryParser(";");

            var parsingResult = parser.Parse(line);

            Assert.That(parsingResult.Success, Is.False);
            Assert.That(parsingResult.ErrorMessage, Is.Not.Null);
            Assert.That(parsingResult.ErrorMessage.Contains(partOfErrorMessage), Is.True);
        }
        
        #endregion


        #region Parse byte[]

        [Test]
        public void ShouldParseCorrectLineAsBytes()
        {
            LogEntryParser parser = new LogEntryParser(";");
            var customerId = 123456;
            var pageId = 98765;
            var timeMark = "2024-10-19T22:19:31";
            string line = $"{timeMark};{customerId};{pageId}" + Environment.NewLine;
            byte[] bytes = line.Select(c => (byte)c).ToArray();

            var parsingResult = parser.Parse(bytes);

            Assert.That(parsingResult.Success, Is.True);
            Assert.That(parsingResult.Value.CustomerId, Is.EqualTo(customerId));
            Assert.That(parsingResult.Value.PageId, Is.EqualTo(pageId));
            Assert.That(parsingResult.Value.DateTime.ToString("s"), Is.EqualTo(timeMark));
        }

        [Test]
        [TestCase("2024-10-19T22:19:31;123", "Invalid line")]
        [TestCase("some time;abc123;456", "Invalid time mark:")]
        [TestCase("2024-10-19T22:19:31;abc123;456", "Invalid customer id:")]
        [TestCase("2024-10-19T22:19:31;123;-456", "Invalid page id:")]
        public void ShouldNotParseInCorrectLineAsBytes(string line, string partOfErrorMessage)
        {
            LogEntryParser parser = new LogEntryParser(";");

            byte[] bytes = line.Select(c => (byte)c).ToArray();
            var parsingResult = parser.Parse(bytes);

            Assert.That(parsingResult.Success, Is.False);
            Assert.That(parsingResult.ErrorMessage, Is.Not.Null);
            Assert.That(parsingResult.ErrorMessage.Contains(partOfErrorMessage), Is.True);
        }

        #endregion
    }
}