using BenchmarkDotNet.Running;
using DataProcessingBenchmarks.IOOperations;

// var longToBytesAlgorithmsComparison = BenchmarkRunner.Run<NumberToBytesConversion>();
// var bytesVsStringsComparison = BenchmarkRunner.Run<BytesVsStringsComparisonDuringFileWriting>();
var analiseLogsPerformance = BenchmarkRunner.Run<TrafficAnalyzerPerformanceStringsVsByteArray>();