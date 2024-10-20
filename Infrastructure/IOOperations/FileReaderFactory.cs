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

    public IBytesProducer CreateBytesProducer(string fileName, int lastProcessedPackage = 0, CancellationToken cancellationToken = new())
    {
        return new BytesProducer(fileName, lastProcessedPackage, cancellationToken);
    }
}