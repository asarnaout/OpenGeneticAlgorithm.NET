using System.Diagnostics;
using OpenGA.Net;
using OpenGA.Net.Benchmarks.Problems;

namespace OpenGA.Net.Benchmarks;

/// <summary>
/// Timing benchmark runner specifically for 500 generations to verify README claims.
/// </summary>
public static class TimingBenchmark500
{
    public static async Task RunTimingBenchmarks()
    {
        Console.WriteLine("⏱️ OpenGA.Net Timing Benchmarks (500 Generations)");
        Console.WriteLine("===================================================");
        Console.WriteLine();

        await RunTspTimingBenchmarks();
        await RunKnapsackTimingBenchmarks();
        await RunBinPackingTimingBenchmarks();
    }

    private static async Task RunTspTimingBenchmarks()
    {
        Console.WriteLine("TRAVELING SALESMAN PROBLEM - Timing Results (500 Generations)");
        Console.WriteLine("-".PadRight(70, '-'));
        
        // TSP 30 cities
        var (distanceMatrix30, _) = TspInstanceGenerator.GenerateRandomInstance(30, 42);
        var population30 = TspInstanceGenerator.GenerateInitialPopulation(100, distanceMatrix30, 42);
        
        var sw = Stopwatch.StartNew();
        var result30 = await OpenGARunner<int>
            .Initialize(population30)
            .WithRandomSeed(42)
            .MutationRate(0.1f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(500))
            .RunToCompletionAsync();
        sw.Stop();
        
        var tsp30 = (TspChromosome)result30;
        var distance30 = tsp30.GetTotalDistance();
        
        // Calculate random tour baseline for comparison
        var randomBaseline30 = CalculateRandomTourBaseline(distanceMatrix30, 1000);
        var improvement30 = (randomBaseline30 - distance30) / randomBaseline30 * 100;
        
        Console.WriteLine($"TSP 30 Cities (500 generations):");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  Distance: {distance30:F2}");
        Console.WriteLine($"  Improvement: {improvement30:F1}% better than random");
        Console.WriteLine();
        
        // TSP 50 cities
        var (distanceMatrix50, _) = TspInstanceGenerator.GenerateRandomInstance(50, 42);
        var population50 = TspInstanceGenerator.GenerateInitialPopulation(100, distanceMatrix50, 42);
        
        sw.Restart();
        var result50 = await OpenGARunner<int>
            .Initialize(population50)
            .WithRandomSeed(42)
            .MutationRate(0.08f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.KPointCrossover(3)))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(500))
            .RunToCompletionAsync();
        sw.Stop();
        
        var tsp50 = (TspChromosome)result50;
        var distance50 = tsp50.GetTotalDistance();
        var randomBaseline50 = CalculateRandomTourBaseline(distanceMatrix50, 1000);
        var improvement50 = (randomBaseline50 - distance50) / randomBaseline50 * 100;
        
        Console.WriteLine($"TSP 50 Cities (500 generations):");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  Distance: {distance50:F2}");
        Console.WriteLine($"  Improvement: {improvement50:F1}% better than random");
        Console.WriteLine();
    }

    private static async Task RunKnapsackTimingBenchmarks()
    {
        Console.WriteLine("KNAPSACK PROBLEM - Timing Results (500 Generations)");
        Console.WriteLine("-".PadRight(70, '-'));
        
        // Knapsack 50 items
        var (weights50, values50, capacity50) = KnapsackInstanceGenerator.GenerateRandomInstance(50, 42);
        var population50 = KnapsackInstanceGenerator.GenerateInitialPopulation(100, weights50, values50, capacity50, 42);
        var upperBound50 = KnapsackInstanceGenerator.CalculateUpperBound(weights50, values50, capacity50);
        
        var sw = Stopwatch.StartNew();
        var result50 = await OpenGARunner<bool>
            .Initialize(population50)
            .WithRandomSeed(42)
            .MutationRate(0.25f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(500))
            .RunToCompletionAsync();
        sw.Stop();
        
        var knapsack50 = (KnapsackChromosome)result50;
        var value50 = knapsack50.GetTotalValue();
        var efficiency50 = (value50 / upperBound50) * 100;
        
        Console.WriteLine($"Knapsack 50 Items (500 generations):");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  Value: {value50:F2}");
        Console.WriteLine($"  Efficiency: {efficiency50:F2}%");
        Console.WriteLine();
        
        // Knapsack 100 items
        var (weights100, values100, capacity100) = KnapsackInstanceGenerator.GenerateRandomInstance(100, 42);
        var population100 = KnapsackInstanceGenerator.GenerateInitialPopulation(100, weights100, values100, capacity100, 42);
        var upperBound100 = KnapsackInstanceGenerator.CalculateUpperBound(weights100, values100, capacity100);
        
        sw.Restart();
        var result100 = await OpenGARunner<bool>
            .Initialize(population100)
            .WithRandomSeed(42)
            .MutationRate(0.2f)
            .ParentSelection(c => c.RegisterSingle(s => s.Elitist()))
            .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Generational()))
            .Termination(t => t.MaximumEpochs(500))
            .RunToCompletionAsync();
        sw.Stop();
        
        var knapsack100 = (KnapsackChromosome)result100;
        var value100 = knapsack100.GetTotalValue();
        var efficiency100 = (value100 / upperBound100) * 100;
        
        Console.WriteLine($"Knapsack 100 Items (500 generations):");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  Value: {value100:F2}");
        Console.WriteLine($"  Efficiency: {efficiency100:F2}%");
        Console.WriteLine();
    }

    private static async Task RunBinPackingTimingBenchmarks()
    {
        Console.WriteLine("BIN PACKING PROBLEM - Timing Results (500 Generations)");
        Console.WriteLine("-".PadRight(70, '-'));
        
        // Bin Packing 50 items
        var (itemSizes50, binCapacity50) = BinPackingInstanceGenerator.GenerateRandomInstance(50, 100.0, 42);
        var population50 = BinPackingInstanceGenerator.GenerateInitialPopulation(100, itemSizes50, binCapacity50, 42);
        var lowerBound50 = BinPackingInstanceGenerator.CalculateLowerBound(itemSizes50, binCapacity50);
        
        var sw = Stopwatch.StartNew();
        var result50 = await OpenGARunner<int>
            .Initialize(population50)
            .WithRandomSeed(42)
            .MutationRate(0.25f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(500))
            .RunToCompletionAsync();
        sw.Stop();
        
        var binPacking50 = (BinPackingChromosome)result50;
        var (binsUsed50, utilization50, isValid50) = binPacking50.GetPackingMetrics();
        
        Console.WriteLine($"Bin Packing 50 Items (500 generations):");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  Bins Used: {binsUsed50} (vs {lowerBound50} optimal)");
        Console.WriteLine($"  Utilization: {utilization50:P2}");
        Console.WriteLine();
        
        // Bin Packing 100 items
        var (itemSizes100, binCapacity100) = BinPackingInstanceGenerator.GenerateRandomInstance(100, 100.0, 42);
        var population100 = BinPackingInstanceGenerator.GenerateInitialPopulation(100, itemSizes100, binCapacity100, 42);
        var lowerBound100 = BinPackingInstanceGenerator.CalculateLowerBound(itemSizes100, binCapacity100);
        
        sw.Restart();
        var result100 = await OpenGARunner<int>
            .Initialize(population100)
            .WithRandomSeed(42)
            .MutationRate(0.2f)
            .ParentSelection(c => c.RegisterSingle(s => s.Elitist()))
            .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Generational()))
            .Termination(t => t.MaximumEpochs(500))
            .RunToCompletionAsync();
        sw.Stop();
        
        var binPacking100 = (BinPackingChromosome)result100;
        var (binsUsed100, utilization100, isValid100) = binPacking100.GetPackingMetrics();
        
        Console.WriteLine($"Bin Packing 100 Items (500 generations):");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  Bins Used: {binsUsed100} (vs {lowerBound100} optimal)");
        Console.WriteLine($"  Utilization: {utilization100:P2}");
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
