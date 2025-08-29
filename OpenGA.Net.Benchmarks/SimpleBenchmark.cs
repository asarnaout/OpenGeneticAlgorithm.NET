using System.Diagnostics;
using OpenGA.Net;
using OpenGA.Net.Benchmarks.Problems;

namespace OpenGA.Net.Benchmarks;

/// <summary>
/// Simple performance benchmark runner that measures execution time and solution quality.
/// </summary>
public static class SimpleBenchmark
{
    public static async Task RunSimpleBenchmarks()
    {
        Console.WriteLine("🚀 OpenGA.Net Performance Benchmarks");
        Console.WriteLine("=====================================");
        Console.WriteLine();

        await RunTspBenchmarks();
        await RunNQueensBenchmarks();
        await RunBinPackingBenchmarks();
    }

    private static async Task RunTspBenchmarks()
    {
        Console.WriteLine("TRAVELING SALESMAN PROBLEM - Performance Results");
        Console.WriteLine("-".PadRight(60, '-'));
        
        // TSP 30 cities
        var (distanceMatrix30, _) = TspInstanceGenerator.GenerateRandomInstance(30, 42);
        var population30 = TspInstanceGenerator.GenerateInitialPopulation(50, distanceMatrix30, 42);
        
        var sw = Stopwatch.StartNew();
        var result30 = await OpenGARunner<int>
            .Initialize(population30)
            .WithRandomSeed(42)
            .MutationRate(0.1f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(200))
            .RunToCompletionAsync();
        sw.Stop();
        
        var tsp30 = (TspChromosome)result30;
        var distance30 = tsp30.GetTotalDistance();
        
        // Calculate random tour baseline for comparison
        var randomBaseline30 = CalculateRandomTourBaseline(distanceMatrix30, 100);
        var improvement30 = (randomBaseline30 - distance30) / randomBaseline30 * 100;
        
        Console.WriteLine($"TSP 30 Cities:");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  GA Distance: {distance30:F0}");
        Console.WriteLine($"  Random Avg: {randomBaseline30:F0}");
        Console.WriteLine($"  Improvement: {improvement30:F1}% better than random");
        Console.WriteLine($"  Fitness: {await tsp30.CalculateFitnessAsync():F6}");
        Console.WriteLine();
        
        // TSP 50 cities
        var (distanceMatrix50, _) = TspInstanceGenerator.GenerateRandomInstance(50, 42);
        var population50 = TspInstanceGenerator.GenerateInitialPopulation(50, distanceMatrix50, 42);
        var randomBaseline50 = CalculateRandomTourBaseline(distanceMatrix50, 100);
        
        sw.Restart();
        var result50 = await OpenGARunner<int>
            .Initialize(population50)
            .WithRandomSeed(42)
            .MutationRate(0.08f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.KPointCrossover(3)))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(200))
            .RunToCompletionAsync();
        sw.Stop();
        
        var tsp50 = (TspChromosome)result50;
        var gaDistance50 = tsp50.GetTotalDistance();
        var improvement50 = ((randomBaseline50 - gaDistance50) / randomBaseline50) * 100;
        Console.WriteLine($"TSP 50 Cities:");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  GA Distance: {gaDistance50:F0}");
        Console.WriteLine($"  Random Avg: {randomBaseline50:F0}");
        Console.WriteLine($"  Improvement: {improvement50:F1}% better than random");
        Console.WriteLine($"  Fitness: {await tsp50.CalculateFitnessAsync():F6}");
        Console.WriteLine();
    }

    private static async Task RunNQueensBenchmarks()
    {
        Console.WriteLine("N-QUEENS PROBLEM - Performance Results");
        Console.WriteLine("-".PadRight(60, '-'));
        
        // N-Queens 16x16
        var population16 = NQueensInstanceGenerator.GenerateInitialPopulation(50, 16, 42);
        
        var sw = Stopwatch.StartNew();
        var result16 = await OpenGARunner<int>
            .Initialize(population16)
            .WithRandomSeed(42)
            .MutationRate(0.2f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(200))
            .RunToCompletionAsync();
        sw.Stop();
        
        var nQueens16 = (NQueensChromosome)result16;
        Console.WriteLine($"N-Queens 16x16:");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  Conflicts: {nQueens16.GetConflicts()}");
        Console.WriteLine($"  Valid Solution: {nQueens16.IsSolution()}");
        Console.WriteLine($"  Fitness: {await nQueens16.CalculateFitnessAsync():F6}");
        Console.WriteLine();
        
        // N-Queens 32x32
        var population32 = NQueensInstanceGenerator.GenerateInitialPopulation(50, 32, 42);
        
        sw.Restart();
        var result32 = await OpenGARunner<int>
            .Initialize(population32)
            .WithRandomSeed(42)
            .MutationRate(0.15f)
            .ParentSelection(c => c.RegisterSingle(s => s.RouletteWheel()))
            .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Tournament()))
            .Termination(t => t.MaximumEpochs(200))
            .RunToCompletionAsync();
        sw.Stop();
        
        var nQueens32 = (NQueensChromosome)result32;
        Console.WriteLine($"N-Queens 32x32:");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  Conflicts: {nQueens32.GetConflicts()}");
        Console.WriteLine($"  Valid Solution: {nQueens32.IsSolution()}");
        Console.WriteLine($"  Fitness: {await nQueens32.CalculateFitnessAsync():F6}");
        Console.WriteLine();
    }

    private static async Task RunBinPackingBenchmarks()
    {
        Console.WriteLine("BIN PACKING PROBLEM - Performance Results");
        Console.WriteLine("-".PadRight(60, '-'));
        
        // Bin Packing 50 items
        var (itemSizes50, binCapacity50) = BinPackingInstanceGenerator.GenerateRandomInstance(50, 100.0, 42);
        var population50 = BinPackingInstanceGenerator.GenerateInitialPopulation(50, itemSizes50, binCapacity50, 42);
        var lowerBound50 = BinPackingInstanceGenerator.CalculateLowerBound(itemSizes50, binCapacity50);
        
        var sw = Stopwatch.StartNew();
        var result50 = await OpenGARunner<int>
            .Initialize(population50)
            .WithRandomSeed(42)
            .MutationRate(0.25f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(200))
            .RunToCompletionAsync();
        sw.Stop();
        
        var binPacking50 = (BinPackingChromosome)result50;
        var (binsUsed50, utilization50, isValid50) = binPacking50.GetPackingMetrics();
        Console.WriteLine($"Bin Packing 50 Items:");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  Bins Used: {binsUsed50} (Lower Bound: {lowerBound50})");
        Console.WriteLine($"  Utilization: {utilization50:P2}");
        Console.WriteLine($"  Valid Solution: {isValid50}");
        Console.WriteLine($"  Fitness: {await binPacking50.CalculateFitnessAsync():F6}");
        Console.WriteLine();
        
        // Bin Packing 100 items
        var (itemSizes100, binCapacity100) = BinPackingInstanceGenerator.GenerateRandomInstance(100, 100.0, 42);
        var population100 = BinPackingInstanceGenerator.GenerateInitialPopulation(50, itemSizes100, binCapacity100, 42);
        var lowerBound100 = BinPackingInstanceGenerator.CalculateLowerBound(itemSizes100, binCapacity100);
        
        sw.Restart();
        var result100 = await OpenGARunner<int>
            .Initialize(population100)
            .WithRandomSeed(42)
            .MutationRate(0.2f)
            .ParentSelection(c => c.RegisterSingle(s => s.Elitist()))
            .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Generational()))
            .Termination(t => t.MaximumEpochs(200))
            .RunToCompletionAsync();
        sw.Stop();
        
        var binPacking100 = (BinPackingChromosome)result100;
        var (binsUsed100, utilization100, isValid100) = binPacking100.GetPackingMetrics();
        Console.WriteLine($"Bin Packing 100 Items:");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  Bins Used: {binsUsed100} (Lower Bound: {lowerBound100})");
        Console.WriteLine($"  Utilization: {utilization100:P2}");
        Console.WriteLine($"  Valid Solution: {isValid100}");
        Console.WriteLine($"  Fitness: {await binPacking100.CalculateFitnessAsync():F6}");
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
