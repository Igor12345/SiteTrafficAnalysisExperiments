This solution is not a production-ready application, nor even close to one!!! It has been created solely for some experiments with reading and processing medium and potentially large datasets, as well as for playing with various algorithms and data structures. It is a proof of concept with very fragile and over-engineered code. The application will evolve over time.

Since the first version was developed somewhat hastily, it is quite possible that it contains bugs that will be fixed later. It requires refactoring, and its test coverage is also critically low.

The solution includes an application for generating test data, analyzing logs (with a specified structure), and a set of benchmarks that demonstrate the impact of allocated memory on performance.

The data generation and log analysis applications are configured via configuration files. By default, two files will be created in the folder ..\TestDataGenerator\FileCreator\bin\Debug\net8.0\Logs, each containing 1,000,000 records.

To use these as data for analysis, the simplest approach is to copy the Logs folder to ..\ConsoleUI\bin\Debug\net8.0.

The benchmarks can be run without any prior setup.

For experiments on the impact of memory on performance, two algorithms for determining active users were implemented:

1 - A user is considered active if they visited several different pages over time, regardless of whether this was on the same day or on different days. In this case, storing the visited pages is unnecessary, which reduces the load on the GC.

2 - A user is considered active if they visited several different pages on different days. Here, it's necessary to store the visited pages, which increases the load on the GC.

Reading log entries from the file was done using both the "regular" TextReader (i.e., line as string) and as a byte stream. The performance impact of these methods is clearly visible in the results.

1 - Without history (Dictionary<ulong, int>)
| Method              | Mean       | Error    | StdDev   | Median     | Gen0       | Allocated |
|-------------------- |-----------:|---------:|---------:|-----------:|-----------:|----------:|
| AnalyseLogAsStrings | 1,204.3 ms | 22.87 ms | 51.16 ms | 1,184.6 ms | 16000.0000 | 649.02 MB |
| AnalyseLogAsBytes   |   811.0 ms | 15.87 ms | 20.07 ms |   806.4 ms |          - |   9.07 MB |

2 - With keeping history (Dictionary<ulong, UserHistory>)
| Method              | Mean    | Error    | StdDev   | Gen0      | Gen1      | Gen2      | Allocated |
|-------------------- |--------:|---------:|---------:|----------:|----------:|----------:|----------:|
| AnalyseLogAsStrings | 1.875 s | 0.0374 s | 0.0836 s | 3000.0000 | 2000.0000 | 1000.0000 | 755.51 MB |
| AnalyseLogAsBytes   | 1.352 s | 0.0268 s | 0.0319 s | 1000.0000 |         - |         - | 115.51 MB |

In the nearest plans is the implementation of the definition of the K most active users (implementation has been started with the creation of a priority queue)
Covering unit tests, fixing errors, improving code, ...

