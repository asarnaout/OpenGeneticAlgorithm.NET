using OpenGA.Net;

namespace OpenGA.Net.Benchmarks.Problems;

/// <summary>
/// Bin Packing Problem implementation for benchmarking genetic algorithms.
/// The goal is to pack items of different sizes into the minimum number of bins,
/// where each bin has a fixed capacity.
/// </summary>
public class BinPackingChromosome : Chromosome<int>
{
    private readonly double[] _itemSizes;
    private readonly double _binCapacity;
    private readonly Random _random = new();

    public BinPackingChromosome(IList<int> itemToBinAssignment, double[] itemSizes, double binCapacity) 
        : base(itemToBinAssignment)
    {
        _itemSizes = itemSizes;
        _binCapacity = binCapacity;
    }

    /// <summary>
    /// Calculate fitness based on bin utilization and number of bins used.
    /// Higher fitness = fewer bins + better utilization.
    /// </summary>
    public override async Task<double> CalculateFitnessAsync()
    {
        var (binsUsed, utilization, isValid) = CalculatePackingMetrics();
        
        if (!isValid)
        {
            // Heavily penalize invalid solutions
            return await Task.FromResult(0.0);
        }
        
        // Fitness combines minimizing bins and maximizing utilization
        // Theoretical minimum bins (lower bound)
        double totalSize = _itemSizes.Sum();
        int theoreticalMinBins = (int)Math.Ceiling(totalSize / _binCapacity);
        
        // Normalize bins used (0 to 1, where 1 is optimal)
        double binEfficiency = theoreticalMinBins / (double)binsUsed;
        
        // Combine bin efficiency and utilization (weighted towards bin minimization)
        double fitness = 0.7 * binEfficiency + 0.3 * utilization;
        
        return await Task.FromResult(Math.Max(0, Math.Min(1, fitness)));
    }

    /// <summary>
    /// Mutation by moving random items to different bins or applying local improvements.
    /// </summary>
    public override async Task MutateAsync(Random random)
    {
        if (Genes.Count == 0) return;

        int mutationType = random.Next(3);
        
        switch (mutationType)
        {
            case 0: // Move random item to random bin
                {
                    int itemIndex = random.Next(Genes.Count);
                    int maxBin = Genes.Max();
                    // Either move to existing bin or create new bin
                    Genes[itemIndex] = random.Next(maxBin + 2);
                }
                break;
                
            case 1: // Swap assignments of two random items
                {
                    int item1 = random.Next(Genes.Count);
                    int item2 = random.Next(Genes.Count);
                    (Genes[item1], Genes[item2]) = (Genes[item2], Genes[item1]);
                }
                break;
                
            case 2: // Try to move item to a bin with more space (local improvement)
                {
                    var bins = GetBinContents();
                    var binLoads = GetBinLoads(bins);
                    
                    // Find an item in an overloaded or nearly full bin
                    for (int attempt = 0; attempt < 5; attempt++)
                    {
                        int itemIndex = random.Next(Genes.Count);
                        int currentBin = Genes[itemIndex];
                        double itemSize = _itemSizes[itemIndex];
                        
                        // Try to find a better bin for this item
                        for (int bin = 0; bin < binLoads.Length; bin++)
                        {
                            if (bin != currentBin && binLoads[bin] + itemSize <= _binCapacity)
                            {
                                Genes[itemIndex] = bin;
                                goto mutation_complete;
                            }
                        }
                        
                        // If no existing bin works, try a new bin
                        if (itemSize <= _binCapacity)
                        {
                            Genes[itemIndex] = binLoads.Length;
                            break;
                        }
                    }
                }
                break;
        }

        mutation_complete:
        await Task.CompletedTask;
    }

    /// <summary>
    /// Create a deep copy of this chromosome.
    /// </summary>
    public override async Task<Chromosome<int>> DeepCopyAsync()
    {
        return await Task.FromResult(new BinPackingChromosome(new List<int>(Genes), _itemSizes, _binCapacity));
    }

    /// <summary>
    /// Repair invalid solutions by redistributing items that cause bin overflow.
    /// </summary>
    public override async Task GeneticRepairAsync()
    {
        // Ensure all bin assignments are non-negative
        for (int i = 0; i < Genes.Count; i++)
        {
            if (Genes[i] < 0)
            {
                Genes[i] = 0;
            }
        }

        // Fix overloaded bins using First Fit Decreasing heuristic
        var bins = GetBinContents();
        var binLoads = GetBinLoads(bins);
        
        // Find items in overloaded bins
        var itemsToReassign = new List<int>();
        for (int bin = 0; bin < binLoads.Length; bin++)
        {
            if (binLoads[bin] > _binCapacity)
            {
                // Remove items from this bin until it's valid
                var itemsInBin = bins[bin].OrderByDescending(item => _itemSizes[item]).ToList();
                double currentLoad = binLoads[bin];
                
                foreach (int item in itemsInBin)
                {
                    if (currentLoad <= _binCapacity) break;
                    
                    itemsToReassign.Add(item);
                    currentLoad -= _itemSizes[item];
                }
            }
        }
        
        // Reassign displaced items using First Fit
        foreach (int item in itemsToReassign)
        {
            bool assigned = false;
            
            // Try to fit in existing bins
            bins = GetBinContents(); // Refresh bin contents
            binLoads = GetBinLoads(bins);
            
            for (int bin = 0; bin < binLoads.Length; bin++)
            {
                if (binLoads[bin] + _itemSizes[item] <= _binCapacity)
                {
                    Genes[item] = bin;
                    assigned = true;
                    break;
                }
            }
            
            // If not assigned, create new bin
            if (!assigned)
            {
                int newBin = binLoads.Length;
                Genes[item] = newBin;
            }
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get items assigned to each bin.
    /// </summary>
    private Dictionary<int, List<int>> GetBinContents()
    {
        var bins = new Dictionary<int, List<int>>();
        
        for (int i = 0; i < Genes.Count; i++)
        {
            int bin = Genes[i];
            if (!bins.ContainsKey(bin))
            {
                bins[bin] = new List<int>();
            }
            bins[bin].Add(i);
        }
        
        return bins;
    }

    /// <summary>
    /// Calculate the load (total size) for each bin.
    /// </summary>
    private double[] GetBinLoads(Dictionary<int, List<int>> bins)
    {
        if (bins.Count == 0) return [];
        
        int maxBin = bins.Keys.Max();
        var loads = new double[maxBin + 1];
        
        foreach (var kvp in bins)
        {
            loads[kvp.Key] = kvp.Value.Sum(item => _itemSizes[item]);
        }
        
        return loads;
    }

    /// <summary>
    /// Calculate packing metrics: number of bins used, utilization, and validity.
    /// </summary>
    private (int binsUsed, double utilization, bool isValid) CalculatePackingMetrics()
    {
        var bins = GetBinContents();
        var binLoads = GetBinLoads(bins);
        
        int binsUsed = binLoads.Length;
        bool isValid = binLoads.All(load => load <= _binCapacity);
        
        double totalUsed = binLoads.Sum();
        double totalCapacity = binsUsed * _binCapacity;
        double utilization = totalCapacity > 0 ? totalUsed / totalCapacity : 0;
        
        return (binsUsed, utilization, isValid);
    }

    /// <summary>
    /// Get packing metrics for reporting.
    /// </summary>
    public (int binsUsed, double utilization, bool isValid) GetPackingMetrics() => CalculatePackingMetrics();

    /// <summary>
    /// Get a string representation of the bin packing solution.
    /// </summary>
    public string GetPackingRepresentation()
    {
        var bins = GetBinContents();
        var binLoads = GetBinLoads(bins);
        var result = $"Solution uses {binLoads.Length} bins:\n";
        
        foreach (var kvp in bins.OrderBy(x => x.Key))
        {
            int bin = kvp.Key;
            var items = kvp.Value;
            double load = binLoads[bin];
            double utilization = load / _binCapacity * 100;
            
            result += $"Bin {bin}: Load {load:F2}/{_binCapacity:F2} ({utilization:F1}%) - Items: [{string.Join(", ", items)}]\n";
        }
        
        return result;
    }
}

/// <summary>
/// Bin Packing instance generator and utilities.
/// </summary>
public static class BinPackingInstanceGenerator
{
    /// <summary>
    /// Generate a random bin packing instance.
    /// </summary>
    public static (double[] itemSizes, double binCapacity) GenerateRandomInstance(int numItems, double binCapacity = 100.0, int seed = 42)
    {
        var random = new Random(seed);
        var itemSizes = new double[numItems];
        
        for (int i = 0; i < numItems; i++)
        {
            // Generate items with sizes between 10% and 70% of bin capacity
            itemSizes[i] = binCapacity * (0.1 + random.NextDouble() * 0.6);
        }
        
        return (itemSizes, binCapacity);
    }

    /// <summary>
    /// Generate initial population using different heuristics.
    /// </summary>
    public static BinPackingChromosome[] GenerateInitialPopulation(int populationSize, double[] itemSizes, double binCapacity, int seed = 42)
    {
        var random = new Random(seed);
        var population = new BinPackingChromosome[populationSize];
        
        for (int i = 0; i < populationSize; i++)
        {
            int[] assignment;
            
            if (i % 4 == 0)
            {
                // First Fit Decreasing heuristic
                assignment = FirstFitDecreasing(itemSizes, binCapacity);
            }
            else if (i % 4 == 1)
            {
                // Best Fit heuristic
                assignment = BestFit(itemSizes, binCapacity);
            }
            else if (i % 4 == 2)
            {
                // Random assignment
                assignment = RandomAssignment(itemSizes.Length, random);
            }
            else
            {
                // Next Fit heuristic
                assignment = NextFit(itemSizes, binCapacity);
            }
            
            population[i] = new BinPackingChromosome(assignment, itemSizes, binCapacity);
        }
        
        return population;
    }

    /// <summary>
    /// First Fit Decreasing heuristic.
    /// </summary>
    private static int[] FirstFitDecreasing(double[] itemSizes, double binCapacity)
    {
        var items = itemSizes.Select((size, index) => new { Size = size, Index = index })
                             .OrderByDescending(x => x.Size)
                             .ToArray();
        
        var assignment = new int[itemSizes.Length];
        var binLoads = new List<double>();
        
        foreach (var item in items)
        {
            bool assigned = false;
            
            // Try to fit in existing bins
            for (int bin = 0; bin < binLoads.Count; bin++)
            {
                if (binLoads[bin] + item.Size <= binCapacity)
                {
                    assignment[item.Index] = bin;
                    binLoads[bin] += item.Size;
                    assigned = true;
                    break;
                }
            }
            
            // Create new bin if needed
            if (!assigned)
            {
                assignment[item.Index] = binLoads.Count;
                binLoads.Add(item.Size);
            }
        }
        
        return assignment;
    }

    /// <summary>
    /// Best Fit heuristic.
    /// </summary>
    private static int[] BestFit(double[] itemSizes, double binCapacity)
    {
        var assignment = new int[itemSizes.Length];
        var binLoads = new List<double>();
        
        for (int i = 0; i < itemSizes.Length; i++)
        {
            int bestBin = -1;
            double bestFit = double.MaxValue;
            
            // Find the bin with minimum remaining space that can fit the item
            for (int bin = 0; bin < binLoads.Count; bin++)
            {
                double remainingSpace = binCapacity - binLoads[bin];
                if (remainingSpace >= itemSizes[i] && remainingSpace < bestFit)
                {
                    bestBin = bin;
                    bestFit = remainingSpace;
                }
            }
            
            if (bestBin != -1)
            {
                assignment[i] = bestBin;
                binLoads[bestBin] += itemSizes[i];
            }
            else
            {
                // Create new bin
                assignment[i] = binLoads.Count;
                binLoads.Add(itemSizes[i]);
            }
        }
        
        return assignment;
    }

    /// <summary>
    /// Next Fit heuristic.
    /// </summary>
    private static int[] NextFit(double[] itemSizes, double binCapacity)
    {
        var assignment = new int[itemSizes.Length];
        int currentBin = 0;
        double currentLoad = 0;
        
        for (int i = 0; i < itemSizes.Length; i++)
        {
            if (currentLoad + itemSizes[i] <= binCapacity)
            {
                assignment[i] = currentBin;
                currentLoad += itemSizes[i];
            }
            else
            {
                currentBin++;
                assignment[i] = currentBin;
                currentLoad = itemSizes[i];
            }
        }
        
        return assignment;
    }

    /// <summary>
    /// Random assignment.
    /// </summary>
    private static int[] RandomAssignment(int numItems, Random random)
    {
        var assignment = new int[numItems];
        int maxBins = Math.Max(1, numItems / 3); // Reasonable upper bound
        
        for (int i = 0; i < numItems; i++)
        {
            assignment[i] = random.Next(maxBins);
        }
        
        return assignment;
    }

    /// <summary>
    /// Calculate the theoretical lower bound on the number of bins needed.
    /// </summary>
    public static int CalculateLowerBound(double[] itemSizes, double binCapacity)
    {
        return (int)Math.Ceiling(itemSizes.Sum() / binCapacity);
    }

    /// <summary>
    /// Generate a benchmark instance based on known difficult cases.
    /// </summary>
    public static (double[] itemSizes, double binCapacity) GenerateBenchmarkInstance()
    {
        // This creates a challenging instance where simple heuristics don't perform well
        var binCapacity = 100.0;
        var itemSizes = new double[]
        {
            60, 50, 40, 40, 35, 35, 30, 30, 25, 25,
            20, 20, 20, 15, 15, 15, 15, 10, 10, 10,
            45, 45, 35, 25, 25, 15, 15, 55, 55, 30
        };
        
        return (itemSizes, binCapacity);
    }
}
