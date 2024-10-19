using System.Text;

namespace LogsAnalyzer.IOOperations;

public class FileReaderFactory
{
    public TextReader Create(string fileName)
    {
        FileStream fs = File.OpenRead(fileName);
        var reader = new StreamReader(fs, Encoding.UTF8);
        return reader;
    }
}