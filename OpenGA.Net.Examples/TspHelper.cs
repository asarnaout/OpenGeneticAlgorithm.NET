namespace OpenGA.Net.Examples;

/// <summary>
/// Helper class for creating TSP problems and generating initial populations.
/// </summary>
public static class TspHelper
{
    private static readonly Random Random = new();

    /// <summary>
    /// Creates a symmetric distance matrix for a given number of cities with random distances.
    /// </summary>
    /// <param name="numberOfCities">The number of cities in the TSP problem</param>
    /// <param name="minDistance">Minimum distance between cities</param>
    /// <param name="maxDistance">Maximum distance between cities</param>
    /// <returns>A symmetric distance matrix</returns>
    public static double[,] CreateRandomDistanceMatrix(int numberOfCities, double minDistance = 1.0, double maxDistance = 100.0)
    {
        var matrix = new double[numberOfCities, numberOfCities];

        for (int i = 0; i < numberOfCities; i++)
        {
            for (int j = 0; j < numberOfCities; j++)
            {
                if (i == j)
                {
                    matrix[i, j] = 0; // Distance from a city to itself is 0
                }
                else if (i < j)
                {
                    // Generate random distance
                    var distance = Random.NextDouble() * (maxDistance - minDistance) + minDistance;
                    matrix[i, j] = distance;
                    matrix[j, i] = distance; // Make it symmetric
                }
            }
        }

        return matrix;
    }

    /// <summary>
    /// Creates a distance matrix based on Euclidean coordinates.
    /// </summary>
    /// <param name="coordinates">Array of (x, y) coordinates for each city</param>
    /// <returns>A distance matrix based on Euclidean distances</returns>
    public static double[,] CreateEuclideanDistanceMatrix((double x, double y)[] coordinates)
    {
        int numberOfCities = coordinates.Length;
        var matrix = new double[numberOfCities, numberOfCities];

        for (int i = 0; i < numberOfCities; i++)
        {
            for (int j = 0; j < numberOfCities; j++)
            {
                if (i == j)
                {
                    matrix[i, j] = 0;
                }
                else
                {
                    var dx = coordinates[i].x - coordinates[j].x;
                    var dy = coordinates[i].y - coordinates[j].y;
                    matrix[i, j] = Math.Sqrt(dx * dx + dy * dy);
                }
            }
        }

        return matrix;
    }

    /// <summary>
    /// Generates an initial population of random TSP tours.
    /// </summary>
    /// <param name="populationSize">Number of chromosomes to generate</param>
    /// <param name="numberOfCities">Number of cities in the TSP problem</param>
    /// <param name="distanceMatrix">The distance matrix for the TSP problem</param>
    /// <returns>Array of TSP chromosomes representing random tours</returns>
    public static TspChromosome[] GenerateInitialPopulation(int populationSize, int numberOfCities, double[,] distanceMatrix)
    {
        var population = new TspChromosome[populationSize];

        for (int i = 0; i < populationSize; i++)
        {
            // Create a list of cities (0 to numberOfCities-1)
            var cities = Enumerable.Range(0, numberOfCities).ToList();
            
            // Shuffle the cities to create a random tour
            ShuffleList(cities);

            population[i] = new TspChromosome(cities, distanceMatrix);
        }

        return population;
    }

    /// <summary>
    /// Shuffles a list using the Fisher-Yates shuffle algorithm.
    /// </summary>
    private static void ShuffleList<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Creates a sample TSP problem with predefined city coordinates.
    /// </summary>
    /// <returns>A tuple containing the distance matrix and city coordinates</returns>
    public static (double[,] distanceMatrix, (double x, double y)[] coordinates) CreateSampleTspProblem()
    {
        // Define coordinates for 10 cities
        var coordinates = new (double x, double y)[]
        {
            (60, 200),   // City 0
            (180, 200),  // City 1
            (80, 180),   // City 2
            (140, 180),  // City 3
            (20, 160),   // City 4
            (100, 160),  // City 5
            (200, 160),  // City 6
            (140, 140),  // City 7
            (40, 120),   // City 8
            (100, 120)   // City 9
        };

        var distanceMatrix = CreateEuclideanDistanceMatrix(coordinates);
        return (distanceMatrix, coordinates);
    }
}
