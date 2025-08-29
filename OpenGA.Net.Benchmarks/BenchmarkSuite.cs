using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using OpenGA.Net;
using OpenGA.Net.Benchmarks.Problems;
using System.Text.Json;

namespace OpenGA.Net.Benchmarks;

/// <summary>
/// Comprehensive benchmark suite for OpenGA.Net library.
/// Tests performance and solution quality on well-known optimization problems.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class GeneticAlgorithmBenchmarks
{
    // Configuration parameters
    private const int POPULATION_SIZE = 100;
    private const int MAX_GENERATIONS = 500;
    private const int BENCHMARK_SEED = 42;
    
    // TSP instances
    private double[,] _tsp30DistanceMatrix = null!;
    private double[,] _tsp50DistanceMatrix = null!;
    private TspChromosome[] _tsp30Population = null!;
    private TspChromosome[] _tsp50Population = null!;
    
    // Bin Packing instances
    private double[] _binPacking50ItemSizes = null!;
    private double[] _binPacking100ItemSizes = null!;
    private double _binPackingCapacity = 100.0;
    private BinPackingChromosome[] _binPacking50Population = null!;
    private BinPackingChromosome[] _binPacking100Population = null!;

    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine("Setting up benchmark instances...");
        
        // Setup TSP instances
        (_tsp30DistanceMatrix, _) = TspInstanceGenerator.GenerateRandomInstance(30, BENCHMARK_SEED);
        (_tsp50DistanceMatrix, _) = TspInstanceGenerator.GenerateRandomInstance(50, BENCHMARK_SEED);
        _tsp30Population = TspInstanceGenerator.GenerateInitialPopulation(POPULATION_SIZE, _tsp30DistanceMatrix, BENCHMARK_SEED);
        _tsp50Population = TspInstanceGenerator.GenerateInitialPopulation(POPULATION_SIZE, _tsp50DistanceMatrix, BENCHMARK_SEED);
        
        // Setup Bin Packing instances
        (_binPacking50ItemSizes, _binPackingCapacity) = BinPackingInstanceGenerator.GenerateRandomInstance(50, 100.0, BENCHMARK_SEED);
        (_binPacking100ItemSizes, _) = BinPackingInstanceGenerator.GenerateRandomInstance(100, 100.0, BENCHMARK_SEED);
        _binPacking50Population = BinPackingInstanceGenerator.GenerateInitialPopulation(POPULATION_SIZE, _binPacking50ItemSizes, _binPackingCapacity, BENCHMARK_SEED);
        _binPacking100Population = BinPackingInstanceGenerator.GenerateInitialPopulation(POPULATION_SIZE, _binPacking100ItemSizes, _binPackingCapacity, BENCHMARK_SEED);
        
        Console.WriteLine("Benchmark setup completed.");
    }

    #region TSP Benchmarks

    [Benchmark(Description = "TSP-30 Cities (Tournament + OnePoint + Elitist)")]
    public async Task<TspChromosome> TSP30_Tournament_OnePoint_Elitist()
    {
        var population = CloneTspPopulation(_tsp30Population);
        
        var result = await OpenGARunner<int>
            .Initialize(population)
            .WithRandomSeed(BENCHMARK_SEED)
            .MutationRate(0.1f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(MAX_GENERATIONS))
            .RunToCompletionAsync();
            
        return (TspChromosome)result;
    }

    [Benchmark(Description = "TSP-30 Cities (RouletteWheel + Uniform + Generational)")]
    public async Task<TspChromosome> TSP30_RouletteWheel_Uniform_Generational()
    {
        var population = CloneTspPopulation(_tsp30Population);
        
        var result = await OpenGARunner<int>
            .Initialize(population)
            .WithRandomSeed(BENCHMARK_SEED)
            .MutationRate(0.15f)
            .ParentSelection(c => c.RegisterSingle(s => s.RouletteWheel()))
            .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Generational()))
            .Termination(t => t.MaximumEpochs(MAX_GENERATIONS))
            .RunToCompletionAsync();
            
        return (TspChromosome)result;
    }

    [Benchmark(Description = "TSP-50 Cities (Tournament + KPoint + Elitist)")]
    public async Task<TspChromosome> TSP50_Tournament_KPoint_Elitist()
    {
        var population = CloneTspPopulation(_tsp50Population);
        
        var result = await OpenGARunner<int>
            .Initialize(population)
            .WithRandomSeed(BENCHMARK_SEED)
            .MutationRate(0.08f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.KPointCrossover(3)))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(MAX_GENERATIONS))
            .RunToCompletionAsync();
            
        return (TspChromosome)result;
    }

    #endregion

    #region Bin Packing Benchmarks

    [Benchmark(Description = "BinPacking-50 Items (Tournament + OnePoint + Elitist)")]
    public async Task<BinPackingChromosome> BinPacking50_Tournament_OnePoint_Elitist()
    {
        var population = CloneBinPackingPopulation(_binPacking50Population);
        
        var result = await OpenGARunner<int>
            .Initialize(population)
            .WithRandomSeed(BENCHMARK_SEED)
            .MutationRate(0.25f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(MAX_GENERATIONS))
            .RunToCompletionAsync();
            
        return (BinPackingChromosome)result;
    }

    [Benchmark(Description = "BinPacking-100 Items (Elitist + Uniform + Generational)")]
    public async Task<BinPackingChromosome> BinPacking100_Elitist_Uniform_Generational()
    {
        var population = CloneBinPackingPopulation(_binPacking100Population);
        
        var result = await OpenGARunner<int>
            .Initialize(population)
            .WithRandomSeed(BENCHMARK_SEED)
            .MutationRate(0.2f)
            .ParentSelection(c => c.RegisterSingle(s => s.Elitist()))
            .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Generational()))
            .Termination(t => t.MaximumEpochs(MAX_GENERATIONS))
            .RunToCompletionAsync();
            
        return (BinPackingChromosome)result;
    }

    #endregion

    #region Helper Methods

    private TspChromosome[] CloneTspPopulation(TspChromosome[] original)
    {
        return original.Select(async chromosome => await chromosome.DeepCopyAsync())
                      .Select(task => task.Result)
                      .Cast<TspChromosome>()
                      .ToArray();
    }

    private BinPackingChromosome[] CloneBinPackingPopulation(BinPackingChromosome[] original)
    {
        return original.Select(async chromosome => await chromosome.DeepCopyAsync())
                      .Select(task => task.Result)
                      .Cast<BinPackingChromosome>()
                      .ToArray();
    }

    #endregion
}

/// <summary>
/// Detailed solution quality analysis for genetic algorithm results.
/// </summary>
public static class BenchmarkAnalyzer
{
    public static async Task RunDetailedAnalysis()
    {
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("OPENGA.NET BENCHMARK ANALYSIS");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        await AnalyzeTSP();
        await AnalyzeBinPacking();
        
        Console.WriteLine("Analysis completed.");
    }

    private static async Task AnalyzeTSP()
    {
        Console.WriteLine("TRAVELING SALESMAN PROBLEM ANALYSIS");
        Console.WriteLine("-".PadRight(50, '-'));
        
        // TSP 30 cities analysis
        var (distanceMatrix30, _) = TspInstanceGenerator.GenerateRandomInstance(30, 42);
        var population30 = TspInstanceGenerator.GenerateInitialPopulation(100, distanceMatrix30, 42);
        
        var bestTsp30 = await OpenGARunner<int>
            .Initialize(population30)
            .WithRandomSeed(42)
            .MutationRate(0.1f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(500))
            .RunToCompletionAsync();
            
        var tspResult = (TspChromosome)bestTsp30;
        var distance = tspResult.GetTotalDistance();
        var fitness = await tspResult.CalculateFitnessAsync();
        
        // Calculate improvement over random baseline
        var randomBaseline30 = CalculateRandomTourBaseline(distanceMatrix30, 1000);
        var improvement30 = (randomBaseline30 - distance) / randomBaseline30 * 100;
        
        Console.WriteLine($"TSP 30 Cities:");
        Console.WriteLine($"  Best Distance: {distance:F2}");
        Console.WriteLine($"  Random Baseline: {randomBaseline30:F2}");
        Console.WriteLine($"  Improvement: {improvement30:F1}% over random");
        Console.WriteLine($"  Fitness: {fitness:F6}");
        Console.WriteLine($"  Route: {string.Join(" → ", tspResult.Genes.Take(10))}...");
        Console.WriteLine();
        
        // TSP 50 cities analysis
        var (distanceMatrix50, _) = TspInstanceGenerator.GenerateRandomInstance(50, 42);
        var population50 = TspInstanceGenerator.GenerateInitialPopulation(100, distanceMatrix50, 42);
        
        var bestTsp50 = await OpenGARunner<int>
            .Initialize(population50)
            .WithRandomSeed(42)
            .MutationRate(0.08f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.KPointCrossover(3)))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(500))
            .RunToCompletionAsync();
            
        var tspResult50 = (TspChromosome)bestTsp50;
        var distance50 = tspResult50.GetTotalDistance();
        var fitness50 = await tspResult50.CalculateFitnessAsync();
        
        Console.WriteLine($"TSP 50 Cities:");
        Console.WriteLine($"  Best Distance: {distance50:F2}");
        Console.WriteLine($"  Fitness: {fitness50:F6}");
        Console.WriteLine($"  Route: {string.Join(" → ", tspResult50.Genes.Take(10))}...");
        Console.WriteLine();
    }

    private static async Task AnalyzeBinPacking()
    {
        Console.WriteLine("BIN PACKING PROBLEM ANALYSIS");
        Console.WriteLine("-".PadRight(50, '-'));
        
        // Bin Packing 50 items analysis
        var (itemSizes50, binCapacity50) = BinPackingInstanceGenerator.GenerateRandomInstance(50, 100.0, 42);
        var population50 = BinPackingInstanceGenerator.GenerateInitialPopulation(100, itemSizes50, binCapacity50, 42);
        var lowerBound50 = BinPackingInstanceGenerator.CalculateLowerBound(itemSizes50, binCapacity50);
        
        var bestBinPacking50 = await OpenGARunner<int>
            .Initialize(population50)
            .WithRandomSeed(42)
            .MutationRate(0.25f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(500))
            .RunToCompletionAsync();
            
        var binPackingResult50 = (BinPackingChromosome)bestBinPacking50;
        var (binsUsed50, utilization50, isValid50) = binPackingResult50.GetPackingMetrics();
        var fitness50 = await binPackingResult50.CalculateFitnessAsync();
        
        Console.WriteLine($"Bin Packing 50 Items:");
        Console.WriteLine($"  Bins Used: {binsUsed50} (Lower Bound: {lowerBound50})");
        Console.WriteLine($"  Utilization: {utilization50:P2}");
        Console.WriteLine($"  Valid Solution: {isValid50}");
        Console.WriteLine($"  Fitness: {fitness50:F6}");
        Console.WriteLine();
        
        // Bin Packing 100 items analysis
        var (itemSizes100, binCapacity100) = BinPackingInstanceGenerator.GenerateRandomInstance(100, 100.0, 42);
        var population100 = BinPackingInstanceGenerator.GenerateInitialPopulation(100, itemSizes100, binCapacity100, 42);
        var lowerBound100 = BinPackingInstanceGenerator.CalculateLowerBound(itemSizes100, binCapacity100);
        
        var bestBinPacking100 = await OpenGARunner<int>
            .Initialize(population100)
            .WithRandomSeed(42)
            .MutationRate(0.2f)
            .ParentSelection(c => c.RegisterSingle(s => s.Elitist()))
            .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Generational()))
            .Termination(t => t.MaximumEpochs(500))
            .RunToCompletionAsync();
            
        var binPackingResult100 = (BinPackingChromosome)bestBinPacking100;
        var (binsUsed100, utilization100, isValid100) = binPackingResult100.GetPackingMetrics();
        var fitness100 = await binPackingResult100.CalculateFitnessAsync();
        
        Console.WriteLine($"Bin Packing 100 Items:");
        Console.WriteLine($"  Bins Used: {binsUsed100} (Lower Bound: {lowerBound100})");
        Console.WriteLine($"  Utilization: {utilization100:P2}");
        Console.WriteLine($"  Valid Solution: {isValid100}");
        Console.WriteLine($"  Fitness: {fitness100:F6}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Calculate baseline performance by averaging random tours.
    /// </summary>
    private static double CalculateRandomTourBaseline(double[,] distanceMatrix, int numSamples)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var numCities = distanceMatrix.GetLength(0);
        var totalDistance = 0.0;
        
        for (int i = 0; i < numSamples; i++)
        {
            // Generate random tour
            var cities = Enumerable.Range(0, numCities).OrderBy(x => random.Next()).ToList();
            
            // Calculate tour distance
            var tourDistance = 0.0;
            for (int j = 0; j < cities.Count - 1; j++)
            {
                tourDistance += distanceMatrix[cities[j], cities[j + 1]];
            }
            tourDistance += distanceMatrix[cities[^1], cities[0]]; // Return to start
            
            totalDistance += tourDistance;
        }
        
        return totalDistance / numSamples;
    }
}
