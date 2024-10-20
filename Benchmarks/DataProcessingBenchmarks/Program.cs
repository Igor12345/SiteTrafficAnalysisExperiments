using BenchmarkDotNet.Running;
using DataProcessingBenchmarks.AnalyzersExecution;


// var longToBytesAlgorithmsComparison = BenchmarkRunner.Run<NumberToBytesConversion>();
// var bytesVsStringsComparison = BenchmarkRunner.Run<BytesVsStringsComparisonDuringFileWriting>();
var analiseLogsPerformance = BenchmarkRunner.Run<TrafficAnalyzerPerformanceStringsVsByteArray>();