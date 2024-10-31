using Infrastructure;
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
            
            switch (parsingResult)
            {
                case Failure<LogEntry>:
                    Assert.Fail();
                    break;
                case Success<LogEntry> success:
                    Assert.That(success.Value.CustomerId, Is.EqualTo(customerId));
                    Assert.That(success.Value.PageId, Is.EqualTo(pageId));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parsingResult));
            }
        }

        [Test]
        [TestCase("some time;123", "Invalid line")]
        [TestCase("some time;abc123;456", "Invalid customer id:")]
        [TestCase("some time;123;-456", "Invalid page id:")]
        public void ShouldNotParseInCorrectLineShortVersion(string line, string partOfErrorMessage)
        {
            LogEntryParser parser = new LogEntryParser(";");

            var parsingResult = parser.ParseShort(line);
            
            switch (parsingResult)
            {
                case Failure<LogEntry> failure:
                    Assert.That(failure.ErrorMessage, Is.Not.Null);
                    Assert.That(failure.ErrorMessage.Contains(partOfErrorMessage), Is.True);
                    break;
                case Success<LogEntry>:
                    Assert.Fail();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parsingResult));
            }
        }

        [Test]
        public void ShouldParseCorrectLineFullVersion()
        {
            LogEntryParser parser = new LogEntryParser(";");
            var customerId = 123456;
            var pageId = 98765;

            var parsingResult = parser.ParseShort($"2024-10-19T22:19:31;{customerId};{pageId}");

            switch (parsingResult)
            {
                case Failure<LogEntry>:
                    Assert.Fail();
                    break;
                case Success<LogEntry> success:
                    Assert.That(success.Value.CustomerId, Is.EqualTo(customerId));
                    Assert.That(success.Value.PageId, Is.EqualTo(pageId));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parsingResult));
            }
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
            
            switch (parsingResult)
            {
                case Failure<LogEntry> failure:
                    Assert.That(failure.ErrorMessage, Is.Not.Null);
                    Assert.That(failure.ErrorMessage.Contains(partOfErrorMessage), Is.True);
                    break;
                case Success<LogEntry>:
                    Assert.Fail();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parsingResult));
            }
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

            switch (parsingResult)
            {
                case Failure<LogEntry>:
                    Assert.Fail();
                    break;
                case Success<LogEntry> success:
                    Assert.That(success.Value.CustomerId, Is.EqualTo(customerId));
                    Assert.That(success.Value.PageId, Is.EqualTo(pageId));
                    Assert.That(success.Value.DateTime.ToString("s"), Is.EqualTo(timeMark));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parsingResult));
            }
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

            switch (parsingResult)
            {
                case Failure<LogEntry> failure:
                    Assert.That(failure.ErrorMessage, Is.Not.Null);
                    Assert.That(failure.ErrorMessage.Contains(partOfErrorMessage), Is.True);
                    break;
                case Success<LogEntry>:
                    Assert.Fail();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(parsingResult));
            }
        }

        #endregion
    }
}