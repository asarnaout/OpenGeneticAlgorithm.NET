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
        await RunKnapsackBenchmarks();
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

    private static async Task RunKnapsackBenchmarks()
    {
        Console.WriteLine("KNAPSACK PROBLEM - Performance Results");
        Console.WriteLine("-".PadRight(60, '-'));
        
        // Knapsack 50 items
        var (weights50, values50, capacity50) = KnapsackInstanceGenerator.GenerateRandomInstance(50, 42);
        var population50 = KnapsackInstanceGenerator.GenerateInitialPopulation(50, weights50, values50, capacity50, 42);
        var upperBound50 = KnapsackInstanceGenerator.CalculateUpperBound(weights50, values50, capacity50);
        var (greedyValue50, greedyWeight50, greedyItems50) = KnapsackInstanceGenerator.CalculateGreedyBaseline(weights50, values50, capacity50);
        
        var sw = Stopwatch.StartNew();
        var result50 = await OpenGARunner<bool>
            .Initialize(population50)
            .WithRandomSeed(42)
            .MutationRate(0.1f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(200))
            .RunToCompletionAsync();
        sw.Stop();
        
        var knapsack50 = (KnapsackChromosome)result50;
        var gaValue50 = knapsack50.GetTotalValue();
        var gaWeight50 = knapsack50.GetTotalWeight();
        var improvement50 = ((gaValue50 - greedyValue50) / greedyValue50) * 100;
        var efficiencyVsOptimal50 = (gaValue50 / upperBound50) * 100;
        
        Console.WriteLine($"Knapsack 50 Items:");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  GA Value: {gaValue50:F1} (Weight: {gaWeight50:F1}/{capacity50:F1})");
        Console.WriteLine($"  Greedy Value: {greedyValue50:F1}");
        Console.WriteLine($"  Upper Bound: {upperBound50:F1}");
        Console.WriteLine($"  Improvement over Greedy: {improvement50:F1}%");
        Console.WriteLine($"  Efficiency vs Upper Bound: {efficiencyVsOptimal50:F1}%");
        Console.WriteLine($"  Valid Solution: {knapsack50.IsValidSolution()}");
        Console.WriteLine($"  Fitness: {await knapsack50.CalculateFitnessAsync():F1}");
        Console.WriteLine();
        
        // Knapsack 100 items
        var (weights100, values100, capacity100) = KnapsackInstanceGenerator.GenerateRandomInstance(100, 42);
        var population100 = KnapsackInstanceGenerator.GenerateInitialPopulation(50, weights100, values100, capacity100, 42);
        var upperBound100 = KnapsackInstanceGenerator.CalculateUpperBound(weights100, values100, capacity100);
        var (greedyValue100, greedyWeight100, greedyItems100) = KnapsackInstanceGenerator.CalculateGreedyBaseline(weights100, values100, capacity100);
        
        sw.Restart();
        var result100 = await OpenGARunner<bool>
            .Initialize(population100)
            .WithRandomSeed(42)
            .MutationRate(0.08f)
            .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
            .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
            .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
            .Termination(t => t.MaximumEpochs(250))
            .RunToCompletionAsync();
        sw.Stop();
        
        var knapsack100 = (KnapsackChromosome)result100;
        var gaValue100 = knapsack100.GetTotalValue();
        var gaWeight100 = knapsack100.GetTotalWeight();
        var improvement100 = ((gaValue100 - greedyValue100) / greedyValue100) * 100;
        var efficiencyVsOptimal100 = (gaValue100 / upperBound100) * 100;
        
        Console.WriteLine($"Knapsack 100 Items:");
        Console.WriteLine($"  Time: {sw.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  GA Value: {gaValue100:F1} (Weight: {gaWeight100:F1}/{capacity100:F1})");
        Console.WriteLine($"  Greedy Value: {greedyValue100:F1}");
        Console.WriteLine($"  Upper Bound: {upperBound100:F1}");
        Console.WriteLine($"  Improvement over Greedy: {improvement100:F1}%");
        Console.WriteLine($"  Efficiency vs Upper Bound: {efficiencyVsOptimal100:F1}%");
        Console.WriteLine($"  Valid Solution: {knapsack100.IsValidSolution()}");
        Console.WriteLine($"  Fitness: {await knapsack100.CalculateFitnessAsync():F1}");
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
