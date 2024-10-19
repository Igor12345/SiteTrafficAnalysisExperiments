using BenchmarkDotNet.Running;
using DataProcessingBenchmarks.ByteOperations;
using DataProcessingBenchmarks.IOOperations;

// var longToBytesAlgorithmsComparison = BenchmarkRunner.Run<NumberToBytesConversion>();
var bytesVsStringsComparison = BenchmarkRunner.Run<BytesVsStringsComparisonDuringFileWriting>();