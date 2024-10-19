﻿namespace FileCreator;

internal sealed class FileCreatorConfiguration
{
    public string FileName { get; set; }
    public int LogEveryThsLine { get; set; }
    public ulong LinesNumber { get; set; }
    public long PagesNumber { get; set; }
    public long CustomersNumber { get; set; }
    public long IdLowBoundary { get; set; }
    public long CustomersMaxId => IdLowBoundary + CustomersNumber;
    public long PagesMaxId => IdLowBoundary + PagesNumber;

    public string OutputDirectory { get; set; }
}