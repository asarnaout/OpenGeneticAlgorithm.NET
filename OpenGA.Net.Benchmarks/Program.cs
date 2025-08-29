using BenchmarkDotNet.Running;
using OpenGA.Net.Benchmarks;

Console.WriteLine("🧬 OpenGA.Net Benchmark Suite");
Console.WriteLine("==============================");
Console.WriteLine();

// Check if running in benchmark mode or analysis mode
var commandArgs = Environment.GetCommandLineArgs();
bool runAnalysis = commandArgs.Contains("--analysis") || commandArgs.Contains("-a");
bool runSimple = commandArgs.Contains("--simple") || commandArgs.Contains("-s");
bool runTiming500 = commandArgs.Contains("--timing500") || commandArgs.Contains("-t");
bool runVerification = commandArgs.Contains("--verify") || commandArgs.Contains("-v");

if (runAnalysis)
{
    Console.WriteLine("Running detailed solution quality analysis...");
    Console.WriteLine();
    await BenchmarkAnalyzer.RunDetailedAnalysis();
}
else if (runSimple)
{
    Console.WriteLine("Running simple performance benchmarks...");
    Console.WriteLine();
    await SimpleBenchmark.RunSimpleBenchmarks();
}
else if (runTiming500)
{
    Console.WriteLine("Running timing benchmarks with 500 generations...");
    Console.WriteLine();
    await TimingBenchmark500.RunTimingBenchmarks();
}
else if (runVerification)
{
    Console.WriteLine("Running verification benchmarks (multiple runs)...");
    Console.WriteLine();
    await VerificationBenchmark.RunVerificationBenchmarks();
}
else
{
    Console.WriteLine("Running comprehensive BenchmarkDotNet performance benchmarks...");
    Console.WriteLine("Use --analysis flag to run solution quality analysis instead.");
    Console.WriteLine("Use --simple flag to run quick performance tests.");
    Console.WriteLine("Use --timing500 flag to run timing tests with 500 generations.");
    Console.WriteLine("Use --verify flag to run verification tests with multiple runs.");
    Console.WriteLine();
    
    // Run BenchmarkDotNet performance benchmarks
    var summary = BenchmarkRunner.Run<GeneticAlgorithmBenchmarks>();
    
    Console.WriteLine();
    Console.WriteLine("Benchmark completed. Results saved to BenchmarkDotNet.Artifacts folder.");
}
