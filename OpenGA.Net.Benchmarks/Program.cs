using BenchmarkDotNet.Running;
using OpenGA.Net.Benchmarks;

Console.WriteLine("🧬 OpenGA.Net Benchmark Suite");
Console.WriteLine("==============================");
Console.WriteLine();

// Check if running in benchmark mode or analysis mode
var commandArgs = Environment.GetCommandLineArgs();
bool runAnalysis = commandArgs.Contains("--analysis") || commandArgs.Contains("-a");
bool runSimple = commandArgs.Contains("--simple") || commandArgs.Contains("-s");

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
else
{
    Console.WriteLine("Running comprehensive BenchmarkDotNet performance benchmarks...");
    Console.WriteLine("Use --analysis flag to run solution quality analysis instead.");
    Console.WriteLine("Use --simple flag to run quick performance tests.");
    Console.WriteLine();
    
    // Run BenchmarkDotNet performance benchmarks
    var summary = BenchmarkRunner.Run<GeneticAlgorithmBenchmarks>();
    
    Console.WriteLine();
    Console.WriteLine("Benchmark completed. Results saved to BenchmarkDotNet.Artifacts folder.");
}
