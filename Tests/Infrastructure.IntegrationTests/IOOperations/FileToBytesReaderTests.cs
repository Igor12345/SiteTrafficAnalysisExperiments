using Infrastructure.DataStructures;
using Infrastructure.IOOperations;

namespace Infrastructure.IntegrationTests.IOOperations;

[TestFixture]
public class FileToBytesReaderTests
{
    private string[] _files = [];
    private readonly string _testDataFolder = "Test";
    private bool _testFolderCreated;
    private readonly int[] _fileSizes = [123, 5];
    private readonly string[] _fileNames = ["one", "two"];

    [OneTimeSetUp]
    public void CreateFiles()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var testFolder = Path.Combine(currentDirectory, _testDataFolder);
        if (!Directory.Exists(testFolder))
        {
            Directory.CreateDirectory(testFolder);
            _testFolderCreated = true;
        }

        _files = _fileNames.Select(f => Path.Combine(testFolder, f)).ToArray();

        for (int i = 0; i < _files.Length; i++)
        {
            CreateFile(_files[i], _fileSizes[i]);
        }
    }


    [OneTimeTearDown]
    public Task CleanUp()
    {
        foreach (string file in _files)
        {
            File.Delete(file);
        }
        if(_testFolderCreated)
            Directory.Delete(_testDataFolder);
        return Task.CompletedTask;
    }

    [Test]
    public async Task ShouldReadAllBytesFromFile()
    {
        for (int i = 0; i < _files.Length; i++)
        {
            var length = await ReadAllBytesFromFile(_files[i]);
            Assert.That(length, Is.EqualTo(_fileSizes[i]));
        }
    }


    public async Task<int> ReadAllBytesFromFile(string file)
    {
        await using IBytesProducer<char> fileReader = new FileToBytesReader<char>(file);
        byte[] buffer = new byte[20];
        DataChunkContainer<char> inputContainer =
            new DataChunkContainer<char>(buffer, ExpandableStorage<char>.Empty, false);
        int fullLength = 0;

        bool lastContainer;
        do
        {
            var filledContainer = await fileReader.WriteBytesToBufferAsync(inputContainer);
            lastContainer = filledContainer.IsLastPart;
            fullLength += filledContainer.WrittenBytesLength;

        } while (!lastContainer);

        return fullLength;
    }

    private void CreateFile(string file, int size)
    {
        byte[] bytes = new byte[size];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = 1;
        }

        using FileStream inStream = File.Open(file, FileMode.Create);
        inStream.Write(bytes);
    }
}