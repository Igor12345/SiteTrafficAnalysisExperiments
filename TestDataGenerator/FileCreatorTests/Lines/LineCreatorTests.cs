using FileCreator;
using FileCreator.Lines;
using Infrastructure.ByteOperations;

namespace FileCreatorTests.Lines
{
    internal class LineCreatorTests
    {
        [Test]
        public void ShouldCreateLineWithThreeParts()
        {
            FileCreatorConfiguration configuration = new FileCreatorConfiguration()
            {
                IdLowBoundary = 10,
                CustomersNumber = 5,
                PagesNumber = 4
            };
            ILineCreator creator = new LineCreator(configuration);

            Span<byte> buffer = stackalloc byte[100];
            int length = creator.WriteLine(buffer);
            var line = ByteToStringConverter.Convert(buffer[..length]);

            Assert.That(line, Is.Not.Null);
            var parts = line.Split(LineCreator.Delimiter);
            Assert.That(parts.Length, Is.EqualTo(3));

            Assert.That(DateTime.TryParse(parts[0], out _), Is.True);
            Assert.That(Int64.TryParse(parts[1], out _), Is.True);
            Assert.That(Int32.TryParse(parts[2], out _), Is.True);
        }
    }
}
