using System.Text;

namespace Infrastructure.IOOperations;

public class FileReaderFactory
{
    public TextReader Create(string fileName)
    {
        FileStream fs = File.OpenRead(fileName);
        var reader = new StreamReader(fs, Encoding.UTF8);
        return reader;
    }

    public IBytesProducer CreateBytesProducer(string fileName, CancellationToken cancellationToken = new())
    {
        return new BytesProducer(fileName, cancellationToken);
    }
}