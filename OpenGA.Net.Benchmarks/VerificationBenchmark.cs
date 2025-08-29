using System.Diagnostics;
using OpenGA.Net;
using OpenGA.Net.Benchmarks.Problems;

namespace OpenGA.Net.Benchmarks;

/// <summary>
/// Comprehensive verification benchmark runner for README accuracy.
/// </summary>
public static class VerificationBenchmark
{
    public static async Task RunVerificationBenchmarks()
    {
        Console.WriteLine("🔍 OpenGA.Net Verification Benchmarks (Multiple Runs)");
        Console.WriteLine("======================================================");
        Console.WriteLine();

        const int numRuns = 5;
        
        await RunTspVerification(numRuns);
        await RunKnapsackVerification(numRuns);
        await RunBinPackingVerification(numRuns);
    }

    private static async Task RunTspVerification(int numRuns)
    {
        Console.WriteLine($"TRAVELING SALESMAN PROBLEM - Verification ({numRuns} runs)");
        Console.WriteLine("-".PadRight(65, '-'));
        
        var times30 = new List<long>();
        var distances30 = new List<double>();
        var improvements30 = new List<double>();
        
        var times50 = new List<long>();
        var distances50 = new List<double>();
        var improvements50 = new List<double>();
        
        for (int run = 0; run < numRuns; run++)
        {
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
            var randomBaseline30 = CalculateRandomTourBaseline(distanceMatrix30, 1000);
            var improvement30 = (randomBaseline30 - distance30) / randomBaseline30 * 100;
            
            times30.Add(sw.ElapsedMilliseconds);
            distances30.Add(distance30);
            improvements30.Add(improvement30);
            
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
            
            times50.Add(sw.ElapsedMilliseconds);
            distances50.Add(distance50);
            improvements50.Add(improvement50);
        }
        
        Console.WriteLine($"TSP 30 Cities (avg of {numRuns} runs):");
        Console.WriteLine($"  Avg Time: {times30.Average():F0}ms (range: {times30.Min()}-{times30.Max()}ms)");
        Console.WriteLine($"  Avg Distance: {distances30.Average():F0}");
        Console.WriteLine($"  Avg Improvement: {improvements30.Average():F1}%");
        Console.WriteLine();
        
        Console.WriteLine($"TSP 50 Cities (avg of {numRuns} runs):");
        Console.WriteLine($"  Avg Time: {times50.Average():F0}ms (range: {times50.Min()}-{times50.Max()}ms)");
        Console.WriteLine($"  Avg Distance: {distances50.Average():F0}");
        Console.WriteLine($"  Avg Improvement: {improvements50.Average():F1}%");
        Console.WriteLine();
    }

    private static async Task RunKnapsackVerification(int numRuns)
    {
        Console.WriteLine($"KNAPSACK PROBLEM - Verification ({numRuns} runs)");
        Console.WriteLine("-".PadRight(65, '-'));
        
        var times50 = new List<long>();
        var values50 = new List<double>();
        var efficiencies50 = new List<double>();
        
        var times100 = new List<long>();
        var values100 = new List<double>();
        var efficiencies100 = new List<double>();
        
        for (int run = 0; run < numRuns; run++)
        {
            // Knapsack 50 items
            var (weights50, values50List, capacity50) = KnapsackInstanceGenerator.GenerateRandomInstance(50, 42);
            var population50 = KnapsackInstanceGenerator.GenerateInitialPopulation(100, weights50, values50List, capacity50, 42);
            var upperBound50 = KnapsackInstanceGenerator.CalculateUpperBound(weights50, values50List, capacity50);
            
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
            
            times50.Add(sw.ElapsedMilliseconds);
            values50.Add(value50);
            efficiencies50.Add(efficiency50);
            
            // Knapsack 100 items
            var (weights100, values100List, capacity100) = KnapsackInstanceGenerator.GenerateRandomInstance(100, 42);
            var population100 = KnapsackInstanceGenerator.GenerateInitialPopulation(100, weights100, values100List, capacity100, 42);
            var upperBound100 = KnapsackInstanceGenerator.CalculateUpperBound(weights100, values100List, capacity100);
            
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
            
            times100.Add(sw.ElapsedMilliseconds);
            values100.Add(value100);
            efficiencies100.Add(efficiency100);
        }
        
        Console.WriteLine($"Knapsack 50 Items (avg of {numRuns} runs):");
        Console.WriteLine($"  Avg Time: {times50.Average():F0}ms (range: {times50.Min()}-{times50.Max()}ms)");
        Console.WriteLine($"  Avg Value: {values50.Average():F1}");
        Console.WriteLine($"  Avg Efficiency: {efficiencies50.Average():F2}%");
        Console.WriteLine();
        
        Console.WriteLine($"Knapsack 100 Items (avg of {numRuns} runs):");
        Console.WriteLine($"  Avg Time: {times100.Average():F0}ms (range: {times100.Min()}-{times100.Max()}ms)");
        Console.WriteLine($"  Avg Value: {values100.Average():F1}");
        Console.WriteLine($"  Avg Efficiency: {efficiencies100.Average():F2}%");
        Console.WriteLine();
    }

    private static async Task RunBinPackingVerification(int numRuns)
    {
        Console.WriteLine($"BIN PACKING PROBLEM - Verification ({numRuns} runs)");
        Console.WriteLine("-".PadRight(65, '-'));
        
        var times50 = new List<long>();
        var bins50 = new List<int>();
        var utilizations50 = new List<double>();
        
        var times100 = new List<long>();
        var bins100 = new List<int>();
        var utilizations100 = new List<double>();
        
        for (int run = 0; run < numRuns; run++)
        {
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
            
            times50.Add(sw.ElapsedMilliseconds);
            bins50.Add(binsUsed50);
            utilizations50.Add(utilization50);
            
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
            
            times100.Add(sw.ElapsedMilliseconds);
            bins100.Add(binsUsed100);
            utilizations100.Add(utilization100);
        }
        
        Console.WriteLine($"Bin Packing 50 Items (avg of {numRuns} runs):");
        Console.WriteLine($"  Avg Time: {times50.Average():F0}ms (range: {times50.Min()}-{times50.Max()}ms)");
        Console.WriteLine($"  Avg Bins: {bins50.Average():F1} (vs 18 optimal)");
        Console.WriteLine($"  Avg Utilization: {utilizations50.Average():P2}");
        Console.WriteLine();
        
        Console.WriteLine($"Bin Packing 100 Items (avg of {numRuns} runs):");
        Console.WriteLine($"  Avg Time: {times100.Average():F0}ms (range: {times100.Min()}-{times100.Max()}ms)");
        Console.WriteLine($"  Avg Bins: {bins100.Average():F1} (vs 36 optimal)");
        Console.WriteLine($"  Avg Utilization: {utilizations100.Average():P2}");
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
