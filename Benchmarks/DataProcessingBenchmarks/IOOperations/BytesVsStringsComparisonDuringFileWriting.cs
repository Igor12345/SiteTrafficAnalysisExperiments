using BenchmarkDotNet.Attributes;
using Infrastructure.ByteOperations;
using FileCreator.Lines;

namespace DataProcessingBenchmarks.IOOperations
{
    [MemoryDiagnoser]
    public class BytesVsStringsComparisonDuringFileWriting
    {
        [Params(100, 10_000)]
        public int N;

        private ulong[]? Numbers;
        private Dictionary<string, string>? _sourceFilePaths;
        private string _currentDirectory;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _currentDirectory = Directory.GetCurrentDirectory();
            _sourceFilePaths = new Dictionary<string, string>
            {
                { "ByBytes", Path.Combine(_currentDirectory, "ByBytes") },
                { "ByStrings", Path.Combine(_currentDirectory, "ByStrings") }
            };
            Numbers = new ulong[N];
            for (int i = 0; i < N; i++)
            {
                Numbers[i] = (ulong)Random.Shared.NextInt64(0, Int64.MaxValue);
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            if (_sourceFilePaths != null)
            {
                foreach (string filePath in _sourceFilePaths.Values)
                {
                    File.Delete(filePath);
                }
            }
        }

        [GcServer(true)]
        [Benchmark]
        public int WriteNumbersAsBytes()
        {
            int s = 0;
            using LinesWriter linesWriter = new LinesWriter(_sourceFilePaths["ByBytes"]);
            Span<byte> buffer = stackalloc byte[100];
            for (int i = 0; i < N; i++)
            {
                int length = LongToBytesConverter.WriteULongToBytes(Numbers[i], buffer);
                s += length;
                linesWriter.Write(buffer[..length]);
            }
            linesWriter.Flush();
            return s;
        }

        [GcServer(true)]
        [Benchmark]
        public int WriteNumbersAsStrings()
        {
            int s = 0;

            using FileStream fs = File.Create(_sourceFilePaths["ByStrings"]);
            using TextWriter writer = new StreamWriter(fs);
            
            for (int i = 0; i < N; i++)
            {
                var str = Numbers[i].ToString("D");
                s += str.Length;
                writer.WriteLine(str);
            }

            return s;
        }
    }
}
