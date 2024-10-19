namespace FileCreator;

internal sealed class FileCreatorConfiguration
{
    public string FileName { get; set; }
    public int LogEveryThsLine { get; set; }
    public ulong LinesNumber { get; set; }
    public long PagesNumber { get; set; }
    public int CustomersNumber { get; set; }
    public int IdLowBoundary { get; set; }
    
    public long CustomersMaxId => IdLowBoundary + 10 * CustomersNumber;
    public long PagesMaxId => IdLowBoundary + 10 * PagesNumber;

    public string OutputDirectory { get; set; }
}