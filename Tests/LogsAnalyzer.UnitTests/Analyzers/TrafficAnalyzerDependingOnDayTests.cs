﻿using LogsAnalyzer.Analyzers;
using LogsAnalyzer.Exception;
using LogsAnalyzer.Lines;

namespace LogsAnalyzer.UnitTests.Analyzers;

internal class TrafficAnalyzerDependingOnDayTests
{
    //todo add tests that check the analyzer's tolerance for individual invalid entries 

    [Test]
    [TestCaseSource(nameof(Logs), new object[] { 1 })]
    public async Task ShouldThrowExceptionWhenWholeLogsAreIncorrect(
        (IAsyncEnumerable<string> logRecordsSource, int expectedLoyalUsers, ulong[] loyalUsers) td)
    {
        //using the wrong delimiter, so the parser can't parse this log
        LineParser parser = new LineParser("_");
        ITrafficAnalyzer analyzer = new TrafficAnalyzerDependingOnDay(parser);

        Assert.ThrowsAsync<IncorrectLogRecordsException>(() => analyzer.FindLoyalUsersAsync(td.logRecordsSource));
    }

    [Test]
    [TestCaseSource(nameof(Logs), new object[] { 1 })]
    [TestCaseSource(nameof(Logs), new object[] { 2 })]
    [TestCaseSource(nameof(Logs), new object[] { 3 })]
    public async Task ShouldFindLoyalUsers(
        (IAsyncEnumerable<string> logRecordsSource, int expectedLoyalUsers, ulong[] loyalUsers) td)
    {
        LineParser parser = new LineParser(";");
        ITrafficAnalyzer analyzer = new TrafficAnalyzerDependingOnDay(parser);

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
            yield return (LogsSource(1), 0, []);
        }
        else if (forDays == 2)
        {
            yield return (LogsSource(2), 1, [42]);
        }
        else
        {
            yield return (LogsSource(3), 3, [77, 42, 55]);
        }
    }

    private static async IAsyncEnumerable<string> LogsSource(int forDays)
    {
        await foreach (string line in FirstDayLogsSource())
            yield return line;

        if (forDays < 2)
            yield break;

        await foreach (string line in SecondDayLogsSource())
            yield return line;

        if (forDays < 3)
            yield break;

        await foreach (string line in ThirdDayLogsSource())
            yield return line;
    }

    private static async IAsyncEnumerable<string> FirstDayLogsSource()
    {
        ulong loyalUser1 = 42;
        ulong loyalUser2 = 55;
        ulong loyalUser3 = 77;
        ulong conservativeUser = 24;

        ulong[] accidentalUsers = [12, 23];

        yield return $"2024-10-19T19:15:23;{conservativeUser};19";
        yield return $"2024-10-19T19:15:23;{accidentalUsers[0]};45";
        yield return $"2024-10-19T19:15:23;{accidentalUsers[1]};56";
        yield return $"2024-10-19T19:15:23;{conservativeUser};19";
        yield return $"2024-10-19T19:15:23;{accidentalUsers[0]};45";

        yield return $"2024-10-19T19:15:23;{loyalUser2};72";
        yield return $"2024-10-19T19:15:23;{accidentalUsers[0]};45";
        yield return $"2024-10-19T19:15:23;{loyalUser1};45";
        yield return $"2024-10-19T19:15:23;{conservativeUser};39";
        yield return $"2024-10-19T19:15:23;{accidentalUsers[0]};45";

        yield return $"2024-10-19T19:15:23;{loyalUser2};72";
        yield return $"2024-10-19T19:15:23;{loyalUser1};9";
        yield return $"2024-10-19T19:15:23;{accidentalUsers[1]};56";
    }

    private static async IAsyncEnumerable<string> SecondDayLogsSource()
    {
        ulong loyalUser1 = 42;
        ulong loyalUser2 = 55;
        ulong loyalUser3 = 77;
        ulong conservativeUser = 24;

        //new users
        ulong[] accidentalUsers = [112, 123];

        yield return $"2024-10-20T11:12:23;{accidentalUsers[0]};45";
        yield return $"2024-10-20T11:12:23;{accidentalUsers[1]};156";

        //conservativeUser visited the same pages
        yield return $"2024-10-20T11:12:23;{conservativeUser};19";
        yield return $"2024-10-20T11:12:23;{accidentalUsers[0]};145";

        //loyalUser3 first time visited the site
        yield return $"2024-10-20T11:12:23;{loyalUser3};27";
        yield return $"2024-10-20T11:12:23;{accidentalUsers[0]};45";

        //new page for loyalUser1
        yield return $"2024-10-20T11:12:23;{loyalUser1};129";
        yield return $"2024-10-20T11:12:23;{accidentalUsers[0]};45";

        yield return $"2024-10-20T11:12:23;{conservativeUser};39";

        yield return $"2024-10-20T11:12:23;{loyalUser3};57";
        yield return $"2024-10-20T11:12:23;{loyalUser1};9";
        yield return $"2024-10-20T11:12:23;{accidentalUsers[1]};56";
    }

    private static async IAsyncEnumerable<string> ThirdDayLogsSource()
    {
        ulong loyalUser1 = 42;
        ulong loyalUser2 = 55;
        ulong loyalUser3 = 77;
        ulong conservativeUser = 24;

        //new users
        ulong[] accidentalUsers = [312, 323];

        yield return $"2024-10-21T14:22:33;{accidentalUsers[0]};45";
        yield return $"2024-10-21T14:22:33;{accidentalUsers[1]};156";

        //conservativeUser visited the same pages
        yield return $"2024-10-21T14:22:33;{conservativeUser};19";
        yield return $"2024-10-21T14:22:33;{accidentalUsers[0]};145";

        //loyalUser3 first time visited the site
        yield return $"2024-10-21T14:22:33;{loyalUser3};27";
        yield return $"2024-10-21T14:22:33;{accidentalUsers[0]};45";

        //new page for loyalUser2
        yield return $"2024-10-21T14:22:33;{loyalUser2};129";
        yield return $"2024-10-21T14:22:33;{accidentalUsers[0]};45";

        yield return $"2024-10-21T14:22:33;{conservativeUser};39";

        //new page for loyalUser3
        yield return $"2024-10-21T14:22:33;{loyalUser3};357";
        yield return $"2024-10-21T14:22:33;{loyalUser2};72";
        yield return $"2024-10-21T14:22:33;{accidentalUsers[1]};56";
    }
}