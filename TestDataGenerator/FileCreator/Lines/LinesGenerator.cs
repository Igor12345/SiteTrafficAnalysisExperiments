using Infrastructure;

namespace FileCreator.Lines;

internal sealed class LinesGenerator
{
    private readonly ILineCreator _lineCreator;

    public LinesGenerator(ILineCreator lineCreator)
    {
        _lineCreator = Guard.NotNull(lineCreator);
    }

    public IEnumerable<int> Generate(Memory<byte> buffer)
    {
        while (true)
        {
            int lineLength = _lineCreator.WriteLine(buffer.Span);
            yield return lineLength;
        }
    }
}