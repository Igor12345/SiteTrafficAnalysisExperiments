using FileCreator.Lines;

namespace FileCreatorTests.Lines;

internal class LinesGeneratorTests
{
    [Test]
    public void ShouldGenerateLines()
    {
        byte[] buffer = new byte[100];
        int[] lengths = { 4, 2, 5 };

        //Can't be used because of Span
        // Mock<ILineCreator> lineCreatorMoq = new Mock<ILineCreator>();
        // lineCreatorMoq.SetupSequence(creator => creator.WriteLine(buffer)).Returns(lengths[0]).Returns(lengths[1])
        //     .Returns(lengths[2]);
        
        FakeLineCreator lineCreator = new FakeLineCreator(lengths);
        LinesGenerator linesGenerator = new LinesGenerator(lineCreator);

        int number = 0;

        foreach (var lineLength in linesGenerator.Generate(buffer))
        {
            Assert.That(lineLength, Is.EqualTo(lengths[number]));
            number++;
            if (number >= lengths.Length)
                break;
        }
        Assert.That(lineCreator.Called, Is.EqualTo(lengths.Length));
        //Can't be used because of Span
        // lineCreatorMoq.Verify(creator => creator.WriteLine(buffer), Times.Exactly(lengths.Length));
    }

    private class FakeLineCreator : ILineCreator
    {
        private readonly int[] _results;
        public int Called { get; private set; }

        public FakeLineCreator(int[] results)
        {
            _results = results;
        }

        public int WriteLine(Span<byte> buffer)
        {
            Called++;
            return _results[Called - 1];
        }

        public void UpdateDate()
        {
        }
    }
}