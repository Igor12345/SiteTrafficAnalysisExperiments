using System.Text;

namespace Infrastructure.IOOperations;

public class FileReaderFactory
{
    public ILineReader Create(string fileName)
    {
        FileStream fs = File.OpenRead(fileName);
        var reader = new StreamReader(fs, Encoding.UTF8);

        return new LineReader(reader);
    }

    public IBytesProducer<T> CreateBytesProducer<T>(string fileName, CancellationToken cancellationToken = new())
    {
        return new FileToBytesReader<T>(fileName, cancellationToken);
    }
}