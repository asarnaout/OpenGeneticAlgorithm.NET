using OpenGA.Net;

namespace OpenGA.Net.Benchmarks.Problems;

/// <summary>
/// 0/1 Knapsack Problem implementation for benchmarking genetic algorithms.
/// The 0/1 Knapsack problem is to select a subset of items with maximum value
/// while staying within the weight capacity constraint.
/// </summary>
public class KnapsackChromosome : Chromosome<bool>
{
    private readonly double[] _itemWeights;
    private readonly double[] _itemValues;
    private readonly double _capacity;
    private readonly Random _random = new();

    public KnapsackChromosome(IList<bool> itemSelection, double[] itemWeights, double[] itemValues, double capacity) 
        : base(itemSelection)
    {
        _itemWeights = itemWeights;
        _itemValues = itemValues;
        _capacity = capacity;
    }

    /// <summary>
    /// Calculate fitness based on the value-to-weight ratio with penalty for exceeding capacity.
    /// Perfect solution maximizes value while staying within capacity.
    /// </summary>
    public override async Task<double> CalculateFitnessAsync()
    {
        double totalValue = 0;
        double totalWeight = 0;

        for (int i = 0; i < Genes.Count; i++)
        {
            if (Genes[i])
            {
                totalValue += _itemValues[i];
                totalWeight += _itemWeights[i];
            }
        }

        // If over capacity, apply heavy penalty
        if (totalWeight > _capacity)
        {
            double overWeight = totalWeight - _capacity;
            double penalty = overWeight * 1000; // Heavy penalty for constraint violation
            return await Task.FromResult(Math.Max(0, totalValue - penalty));
        }

        return await Task.FromResult(totalValue);
    }

    /// <summary>
    /// Mutation by flipping random bits (item selection).
    /// </summary>
    public override async Task MutateAsync(Random random)
    {
        if (Genes.Count == 0) return;

        // Flip 1-3 random bits
        int numFlips = random.Next(1, Math.Min(4, Genes.Count + 1));
        for (int i = 0; i < numFlips; i++)
        {
            int index = random.Next(Genes.Count);
            Genes[index] = !Genes[index];
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Create a deep copy of this chromosome.
    /// </summary>
    public override async Task<Chromosome<bool>> DeepCopyAsync()
    {
        return await Task.FromResult(new KnapsackChromosome(new List<bool>(Genes), _itemWeights, _itemValues, _capacity));
    }

    /// <summary>
    /// Repair invalid solutions by removing items until within capacity.
    /// Uses a greedy approach removing items with worst value-to-weight ratio first.
    /// </summary>
    public override async Task GeneticRepairAsync()
    {
        double totalWeight = GetTotalWeight();
        
        if (totalWeight <= _capacity)
        {
            await Task.CompletedTask;
            return;
        }

        // Create list of selected items with their value-to-weight ratios
        var selectedItems = new List<(int index, double ratio)>();
        for (int i = 0; i < Genes.Count; i++)
        {
            if (Genes[i])
            {
                double ratio = _itemWeights[i] > 0 ? _itemValues[i] / _itemWeights[i] : 0;
                selectedItems.Add((i, ratio));
            }
        }

        // Sort by value-to-weight ratio (ascending, so worst ratios first)
        selectedItems.Sort((a, b) => a.ratio.CompareTo(b.ratio));

        // Remove items with worst ratios until within capacity
        foreach (var (index, _) in selectedItems)
        {
            if (GetTotalWeight() <= _capacity)
                break;
                
            Genes[index] = false;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Get the total weight of selected items.
    /// </summary>
    public double GetTotalWeight()
    {
        double weight = 0;
        for (int i = 0; i < Genes.Count; i++)
        {
            if (Genes[i])
                weight += _itemWeights[i];
        }
        return weight;
    }

    /// <summary>
    /// Get the total value of selected items.
    /// </summary>
    public double GetTotalValue()
    {
        double value = 0;
        for (int i = 0; i < Genes.Count; i++)
        {
            if (Genes[i])
                value += _itemValues[i];
        }
        return value;
    }

    /// <summary>
    /// Check if this is a valid solution (within capacity).
    /// </summary>
    public bool IsValidSolution() => GetTotalWeight() <= _capacity;

    /// <summary>
    /// Get the number of selected items.
    /// </summary>
    public int GetSelectedItemCount() => Genes.Count(g => g);

    /// <summary>
    /// Get capacity utilization as a percentage.
    /// </summary>
    public double GetCapacityUtilization() => GetTotalWeight() / _capacity;
}

/// <summary>
/// Knapsack instance generator and utilities.
/// </summary>
public static class KnapsackInstanceGenerator
{
    /// <summary>
    /// Generate initial population for Knapsack problem.
    /// Each chromosome represents a binary selection of items.
    /// </summary>
    public static KnapsackChromosome[] GenerateInitialPopulation(int populationSize, double[] itemWeights, double[] itemValues, double capacity, int seed = 42)
    {
        var random = new Random(seed);
        var population = new KnapsackChromosome[populationSize];

        for (int i = 0; i < populationSize; i++)
        {
            var itemSelection = new List<bool>();
            
            if (i == 0)
            {
                // First individual: empty knapsack
                for (int j = 0; j < itemWeights.Length; j++)
                    itemSelection.Add(false);
            }
            else if (i == 1 && populationSize > 1)
            {
                // Second individual: greedy solution based on value-to-weight ratio
                var items = itemWeights.Select((weight, index) => new { Index = index, Weight = weight, Value = itemValues[index], Ratio = itemValues[index] / weight })
                                      .OrderByDescending(x => x.Ratio)
                                      .ToList();

                var selection = new bool[itemWeights.Length];
                double currentWeight = 0;

                foreach (var item in items)
                {
                    if (currentWeight + item.Weight <= capacity)
                    {
                        selection[item.Index] = true;
                        currentWeight += item.Weight;
                    }
                }

                itemSelection = selection.ToList();
            }
            else
            {
                // Random selection with probability inversely related to weight
                for (int j = 0; j < itemWeights.Length; j++)
                {
                    // Higher probability for lighter items
                    double maxWeight = itemWeights.Max();
                    double probability = 0.5 * (1 - itemWeights[j] / maxWeight);
                    itemSelection.Add(random.NextDouble() < probability);
                }
            }

            population[i] = new KnapsackChromosome(itemSelection, itemWeights, itemValues, capacity);
        }

        return population;
    }

    /// <summary>
    /// Generate a random knapsack instance.
    /// </summary>
    public static (double[] weights, double[] values, double capacity) GenerateRandomInstance(int numItems, int seed = 42)
    {
        var random = new Random(seed);
        var weights = new double[numItems];
        var values = new double[numItems];

        // Generate weights between 1 and 50
        for (int i = 0; i < numItems; i++)
        {
            weights[i] = random.Next(1, 51);
        }

        // Generate values with some correlation to weight but with random variation
        for (int i = 0; i < numItems; i++)
        {
            double baseValue = weights[i] * (0.5 + random.NextDouble() * 2.0); // 0.5x to 2.5x weight
            values[i] = Math.Round(baseValue, 2);
        }

        // Set capacity to approximately 40-60% of total weight
        double totalWeight = weights.Sum();
        double capacity = Math.Round(totalWeight * (0.4 + random.NextDouble() * 0.2), 2);

        return (weights, values, capacity);
    }

    /// <summary>
    /// Calculate the theoretical upper bound using fractional knapsack (relaxation).
    /// This provides an upper bound for the 0/1 knapsack problem.
    /// </summary>
    public static double CalculateUpperBound(double[] weights, double[] values, double capacity)
    {
        var items = weights.Select((weight, index) => new { Index = index, Weight = weight, Value = values[index], Ratio = values[index] / weight })
                          .OrderByDescending(x => x.Ratio)
                          .ToList();

        double totalValue = 0;
        double remainingCapacity = capacity;

        foreach (var item in items)
        {
            if (item.Weight <= remainingCapacity)
            {
                // Take the whole item
                totalValue += item.Value;
                remainingCapacity -= item.Weight;
            }
            else
            {
                // Take fraction of the item (relaxation)
                totalValue += item.Value * (remainingCapacity / item.Weight);
                break;
            }
        }

        return totalValue;
    }

    /// <summary>
    /// Calculate a greedy baseline solution for comparison.
    /// </summary>
    public static (double value, double weight, int itemCount) CalculateGreedyBaseline(double[] weights, double[] values, double capacity)
    {
        var items = weights.Select((weight, index) => new { Index = index, Weight = weight, Value = values[index], Ratio = values[index] / weight })
                          .OrderByDescending(x => x.Ratio)
                          .ToList();

        double totalValue = 0;
        double totalWeight = 0;
        int itemCount = 0;

        foreach (var item in items)
        {
            if (totalWeight + item.Weight <= capacity)
            {
                totalValue += item.Value;
                totalWeight += item.Weight;
                itemCount++;
            }
        }

        return (totalValue, totalWeight, itemCount);
    }
}
