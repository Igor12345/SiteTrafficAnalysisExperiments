using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Time.Testing;
using Moq;
using RuntimeStatistic.TrafficProducer;

namespace RuntimeStatistic.UnitTests.TrafficProducer;

//Insufficient test coverage. Only the main, basic scenario is tested
public class TrafficSourceAsyncStartTests
{
    [Test]
    public async Task ShouldProduceEventsAtRequiredPace()
    {
        var fakeTimeProvider = new FakeTimeProvider();
        Mock<IEventsGenerator<int>> generatorMock = new();
        int[] expectedValues = [123, 432, 7];
        generatorMock.SetupSequence(g => g.Next()).Returns(expectedValues[0]).Returns(expectedValues[1])
            .Returns(expectedValues[2]);

        Channel<int> channel = Channel.CreateUnbounded<int>();
        var interval = TimeSpan.FromMilliseconds(10);
        var trafficSource = new TrafficSourceAsyncStart<int>(generatorMock.Object, channel,
            interval, fakeTimeProvider);

        CancellationTokenSource cts = new();

        //For simplicity's sake. 
        //I could use the Task.Factory.StartNew method to pass TaskCreationOptions.LongRunning, but see this blog post:
        // https://blog.stephencleary.com/2013/08/startnew-is-dangerous.html
        //It would be an unnecessary complication for tests anyway.
        _ = Task.Run(async () => await trafficSource.StartGenerationAsync(cts.Token), cts.Token);

        ConcurrentQueue<int> realResults = new();
        _ = Task.Run(async () =>
        {
            await foreach (int item in channel.Reader.ReadAllAsync(cts.Token).ConfigureAwait(false))
            {
                realResults.Enqueue(item);
                if (realResults.Count > expectedValues.Length)
                    Assert.Fail("Expected count is greater than the number of events");
            }
        }, cts.Token);

        //to be sure the trafficSource is ready to generate events
        await Task.Delay(interval, cts.Token).ConfigureAwait(false);
        fakeTimeProvider.Advance(interval);
        fakeTimeProvider.Advance(interval);
        fakeTimeProvider.Advance(interval);
        
        //to let a channel reader to read all the items from the channel
        await Task.Delay(interval, cts.Token).ConfigureAwait(false);
        
        generatorMock.Verify(g => g.Next(), Times.Exactly(expectedValues.Length));
        var firedEvents = realResults.ToArray();
        Assert.That(firedEvents.Count, Is.EqualTo(expectedValues.Length));
        CollectionAssert.AreEqual(expectedValues, firedEvents);
    }
}