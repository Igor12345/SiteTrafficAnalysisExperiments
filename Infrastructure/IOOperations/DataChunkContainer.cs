using Infrastructure.DataStructures;

namespace Infrastructure.IOOperations;

public record DataChunkContainer(byte[] RowData, ExpandableStorage<LogEntry> ParsedRecords, bool IsLastPart)
{
    private static readonly DataChunkContainer _emptyPackage =
        new(Array.Empty<byte>(), new ExpandableStorage<LogEntry>(0),false);

    public static DataChunkContainer Empty => _emptyPackage;
    public int PrePopulatedBytesLength { get; set; }
    public int WrittenBytesLength { get; init; }
}