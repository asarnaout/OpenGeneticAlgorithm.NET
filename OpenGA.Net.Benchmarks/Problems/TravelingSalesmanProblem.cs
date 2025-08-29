using OpenGA.Net;

namespace OpenGA.Net.Benchmarks.Problems;

/// <summary>
/// Traveling Salesman Problem implementation for benchmarking genetic algorithms.
/// The TSP is a classic NP-hard optimization problem where the goal is to find the shortest route
/// that visits each city exactly once and returns to the starting city.
/// </summary>
public class TspChromosome : Chromosome<int>
{
    private readonly double[,] _distanceMatrix;
    private readonly Random _random = new();

    public TspChromosome(IList<int> cities, double[,] distanceMatrix) : base(cities)
    {
        _distanceMatrix = distanceMatrix;
    }

    /// <summary>
    /// Calculate fitness as the inverse of total route distance.
    /// Higher fitness values indicate shorter routes (better solutions).
    /// </summary>
    public override async Task<double> CalculateFitnessAsync()
    {
        var totalDistance = CalculateTotalDistance();
        
        // Use reciprocal with scaling to avoid division by zero and provide meaningful fitness gradients
        return await Task.FromResult(1.0 / (1.0 + totalDistance));
    }

    /// <summary>
    /// Mutation using 2-opt local search improvement.
    /// This is more sophisticated than simple random swaps and often produces better results.
    /// </summary>
    public override async Task MutateAsync(Random random)
    {
        if (Genes.Count < 4) return; // Need at least 4 cities for 2-opt

        // Perform 2-opt mutation: reverse a segment of the route
        int i = random.Next(Genes.Count - 1);
        int j = random.Next(i + 1, Genes.Count);
        
        // Reverse the segment between i and j
        var segment = Genes.Skip(i).Take(j - i + 1).Reverse().ToArray();
        for (int k = 0; k < segment.Length; k++)
        {
            Genes[i + k] = segment[k];
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Create a deep copy of this chromosome for crossover operations.
    /// </summary>
    public override async Task<Chromosome<int>> DeepCopyAsync()
    {
        return await Task.FromResult(new TspChromosome(new List<int>(Genes), _distanceMatrix));
    }

    /// <summary>
    /// Repair function to ensure each city appears exactly once.
    /// This is crucial for TSP as crossover can create invalid tours.
    /// </summary>
    public override async Task GeneticRepairAsync()
    {
        var allCities = Enumerable.Range(0, _distanceMatrix.GetLength(0)).ToHashSet();
        var currentCities = Genes.ToHashSet();
        
        // Find missing and duplicate cities
        var missingCities = allCities.Except(currentCities).ToList();
        var duplicatePositions = new List<int>();
        var seenCities = new HashSet<int>();
        
        for (int i = 0; i < Genes.Count; i++)
        {
            if (!seenCities.Add(Genes[i]))
            {
                duplicatePositions.Add(i);
            }
        }
        
        // Replace duplicates with missing cities
        for (int i = 0; i < Math.Min(duplicatePositions.Count, missingCities.Count); i++)
        {
            Genes[duplicatePositions[i]] = missingCities[i];
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Calculate the total distance of the current route.
    /// </summary>
    private double CalculateTotalDistance()
    {
        if (Genes.Count < 2) return 0;
        
        double totalDistance = 0;
        
        // Calculate distance between consecutive cities
        for (int i = 0; i < Genes.Count - 1; i++)
        {
            totalDistance += _distanceMatrix[Genes[i], Genes[i + 1]];
        }
        
        // Add distance from last city back to first city
        totalDistance += _distanceMatrix[Genes[^1], Genes[0]];
        
        return totalDistance;
    }

    /// <summary>
    /// Get the total distance for reporting purposes.
    /// </summary>
    public double GetTotalDistance() => CalculateTotalDistance();
}

/// <summary>
/// TSP instance generator and data container.
/// </summary>
public static class TspInstanceGenerator
{
    /// <summary>
    /// Generate a random TSP instance with cities distributed in a 2D plane.
    /// </summary>
    public static (double[,] distanceMatrix, (double x, double y)[] cityCoordinates) GenerateRandomInstance(int numCities, int seed = 42)
    {
        var random = new Random(seed);
        var cityCoordinates = new (double x, double y)[numCities];
        var distanceMatrix = new double[numCities, numCities];
        
        // Generate random city coordinates
        for (int i = 0; i < numCities; i++)
        {
            cityCoordinates[i] = (random.NextDouble() * 1000, random.NextDouble() * 1000);
        }
        
        // Calculate Euclidean distances
        for (int i = 0; i < numCities; i++)
        {
            for (int j = 0; j < numCities; j++)
            {
                if (i == j)
                {
                    distanceMatrix[i, j] = 0;
                }
                else
                {
                    var dx = cityCoordinates[i].x - cityCoordinates[j].x;
                    var dy = cityCoordinates[i].y - cityCoordinates[j].y;
                    distanceMatrix[i, j] = Math.Sqrt(dx * dx + dy * dy);
                }
            }
        }
        
        return (distanceMatrix, cityCoordinates);
    }

    /// <summary>
    /// Generate initial population of random valid TSP tours.
    /// </summary>
    public static TspChromosome[] GenerateInitialPopulation(int populationSize, double[,] distanceMatrix, int seed = 42)
    {
        var random = new Random(seed);
        var numCities = distanceMatrix.GetLength(0);
        var population = new TspChromosome[populationSize];
        
        for (int i = 0; i < populationSize; i++)
        {
            // Create a random permutation of cities
            var cities = Enumerable.Range(0, numCities).OrderBy(x => random.Next()).ToList();
            population[i] = new TspChromosome(cities, distanceMatrix);
        }
        
        return population;
    }

    /// <summary>
    /// Get a known benchmark TSP instance for testing.
    /// This creates a simple 10-city circular arrangement for reproducible results.
    /// </summary>
    public static (double[,] distanceMatrix, (double x, double y)[] cityCoordinates) GetBenchmarkInstance()
    {
        const int numCities = 10;
        var cityCoordinates = new (double x, double y)[numCities];
        var distanceMatrix = new double[numCities, numCities];
        
        // Create cities in a circle - optimal tour should visit them in order
        for (int i = 0; i < numCities; i++)
        {
            double angle = 2 * Math.PI * i / numCities;
            cityCoordinates[i] = (Math.Cos(angle) * 100, Math.Sin(angle) * 100);
        }
        
        // Calculate distances
        for (int i = 0; i < numCities; i++)
        {
            for (int j = 0; j < numCities; j++)
            {
                if (i == j)
                {
                    distanceMatrix[i, j] = 0;
                }
                else
                {
                    var dx = cityCoordinates[i].x - cityCoordinates[j].x;
                    var dy = cityCoordinates[i].y - cityCoordinates[j].y;
                    distanceMatrix[i, j] = Math.Sqrt(dx * dx + dy * dy);
                }
            }
        }
        
        return (distanceMatrix, cityCoordinates);
    }
}
