using Infrastructure.DataStructures;

namespace Infrastructure.IOOperations;

public record DataChunkContainer<T>(byte[] RowData, ExpandableStorage<T> ParsedRecords, bool IsLastPart)
{
    private static readonly DataChunkContainer<T> _emptyPackage =
        new(Array.Empty<byte>(), new ExpandableStorage<T>(0),false);

    public static DataChunkContainer<T> Empty => _emptyPackage;
    public int PrePopulatedBytesLength { get; set; }
    public int WrittenBytesLength { get; init; }
}