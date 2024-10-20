using LogsAnalyzer.Analyzers;
using LogsAnalyzer.Lines;

namespace LogsAnalyzer.UnitTests.Analyzers
{
    internal class TrafficAnalyzerRegardlessOfTheDayTests
    {
        [Test]
        [TestCaseSource(nameof(Logs), new object[] { 1 })]
        [TestCaseSource(nameof(Logs), new object[] { 2 })]
        public async Task ShouldFindLoyalUsers((IAsyncEnumerable<string> logRecordsSource, int expectedLoyalUsers, ulong[] loyalUsers) td)
        {
            LineParser parser = new LineParser(";");
            TrafficAnalyzerRegardlessOfTheDay analyzer = new TrafficAnalyzerRegardlessOfTheDay(parser);
            
            var foundUsers = await analyzer.FindLoyalUsersAsync(td.logRecordsSource);

            Assert.That(foundUsers, Is.Not.Null);
            Assert.That(foundUsers.Count, Is.EqualTo(td.expectedLoyalUsers));

            foreach (ulong userId in td.loyalUsers)
            {
                Assert.That(foundUsers.Contains(userId), Is.True);
            }
        }

        public static IEnumerable<(IAsyncEnumerable<string>, int, ulong[])> Logs(int forDays)
        {
            if (forDays == 1)
            {
                yield return (LogsSource(1), 2, [24, 42]);
            }
            else
            {
                yield return (LogsSource(2), 3, [24, 42, 55]);
            }
        }

        public static async IAsyncEnumerable<string> FirstDayLogsSource()
        {
            ulong loyalUser1 = 42;
            ulong loyalUser2 = 24;

            ulong[] accidentalUsers = [12, 23, 55];

            yield return $"some time;{loyalUser2};19";
            yield return $"some time;{accidentalUsers[0]};45";
            yield return $"some time;{accidentalUsers[1]};56";
            yield return $"some time;{loyalUser2};19";
            yield return $"some time;{accidentalUsers[0]};45";
            yield return $"some time;{accidentalUsers[2]};72";
            yield return $"some time;{accidentalUsers[0]};45";
            yield return $"some time;{loyalUser1};45";
            yield return $"some time;{loyalUser2};39";
            yield return $"some time;{accidentalUsers[0]};45";
            yield return $"some time;{accidentalUsers[2]};72";
            yield return $"some time;{loyalUser1};9";
            yield return $"some time;{accidentalUsers[1]};56";
        }

        public static async IAsyncEnumerable<string> SecondDayLogsSource()
        {
            ulong loyalUser1 = 42;

            //former accidental user
            ulong anotherLoyalUser = 55;

            ulong[] accidentalUsers = [12, 23];

            yield return $"some time;{accidentalUsers[0]};45";
            yield return $"some time;{accidentalUsers[1]};56";
            yield return $"some time;{accidentalUsers[0]};45";
            yield return $"some time;{anotherLoyalUser};27";
            yield return $"some time;{accidentalUsers[0]};45";
            yield return $"some time;{loyalUser1};9";
            yield return $"some time;{accidentalUsers[0]};45";

            //different page for the anotherLoyalUser
            yield return $"some time;{anotherLoyalUser};27";
            yield return $"some time;{loyalUser1};9";
            yield return $"some time;{accidentalUsers[1]};56";
        }

        public static async IAsyncEnumerable<string> LogsSource(int forDays)
        {
            await foreach (string line in FirstDayLogsSource())
                yield return line;

            if (forDays < 2)
                yield break;

            await foreach (string line in SecondDayLogsSource())
                yield return line;
        }
    }
}
