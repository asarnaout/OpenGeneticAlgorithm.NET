using OpenGA.Net;
using OpenGA.Net.Examples;

namespace OpenGA.Net.Examples;

/// <summary>
/// Comprehensive TSP solver that combines helper methods and solving functionality.
/// Includes support for creating distance matrices, generating populations, and solving TSP problems.
/// </summary>
public static class TspSolver
{
    private static readonly Random Random = new();

    #region Distance Matrix Creation Methods

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

    #endregion

    #region Population Generation Methods

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

    #endregion

    #region Problem Creation Methods

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

    /// <summary>
    /// Creates a large, complex TSP problem with dozens of cities arranged in clusters.
    /// This creates a more realistic and challenging problem structure.
    /// </summary>
    /// <param name="numberOfCities">Total number of cities (recommended: 30-100)</param>
    /// <param name="numberOfClusters">Number of city clusters (recommended: 3-8)</param>
    /// <param name="mapSize">Size of the map area</param>
    /// <returns>A tuple containing the distance matrix and city coordinates</returns>
    public static (double[,] distanceMatrix, (double x, double y)[] coordinates) CreateComplexTspProblem(
        int numberOfCities = 50, 
        int numberOfClusters = 5, 
        double mapSize = 1000.0)
    {
        var coordinates = new (double x, double y)[numberOfCities];
        var citiesPerCluster = numberOfCities / numberOfClusters;
        var remainingCities = numberOfCities % numberOfClusters;

        int cityIndex = 0;

        // Generate cluster centers
        var clusterCenters = new (double x, double y)[numberOfClusters];
        for (int cluster = 0; cluster < numberOfClusters; cluster++)
        {
            clusterCenters[cluster] = (
                Random.NextDouble() * mapSize * 0.8 + mapSize * 0.1, // Keep clusters away from edges
                Random.NextDouble() * mapSize * 0.8 + mapSize * 0.1
            );
        }

        // Generate cities around each cluster center
        for (int cluster = 0; cluster < numberOfClusters; cluster++)
        {
            var citiesInThisCluster = citiesPerCluster + (cluster < remainingCities ? 1 : 0);
            var clusterRadius = mapSize * 0.08; // Cluster radius

            for (int cityInCluster = 0; cityInCluster < citiesInThisCluster; cityInCluster++)
            {
                // Generate city position around cluster center
                var angle = Random.NextDouble() * 2 * Math.PI;
                var distance = Random.NextDouble() * clusterRadius;
                
                var x = clusterCenters[cluster].x + Math.Cos(angle) * distance;
                var y = clusterCenters[cluster].y + Math.Sin(angle) * distance;

                // Ensure coordinates are within bounds
                x = Math.Max(0, Math.Min(mapSize, x));
                y = Math.Max(0, Math.Min(mapSize, y));

                coordinates[cityIndex] = (x, y);
                cityIndex++;
            }
        }

        // Add some random scattered cities for complexity
        var scatteredCities = Math.Min(numberOfCities / 10, 5);
        for (int i = 0; i < scatteredCities && cityIndex < numberOfCities; i++)
        {
            coordinates[cityIndex] = (
                Random.NextDouble() * mapSize,
                Random.NextDouble() * mapSize
            );
            cityIndex++;
        }

        var distanceMatrix = CreateEuclideanDistanceMatrix(coordinates);
        return (distanceMatrix, coordinates);
    }

    /// <summary>
    /// Creates a grid-based TSP problem where cities are arranged in a rough grid pattern.
    /// </summary>
    /// <param name="gridSize">Size of the grid (gridSize x gridSize cities)</param>
    /// <param name="noiseLevel">Amount of random noise to add to grid positions (0.0-1.0)</param>
    /// <param name="spacing">Distance between grid points</param>
    /// <returns>A tuple containing the distance matrix and city coordinates</returns>
    public static (double[,] distanceMatrix, (double x, double y)[] coordinates) CreateGridTspProblem(
        int gridSize = 8, 
        double noiseLevel = 0.3, 
        double spacing = 100.0)
    {
        var numberOfCities = gridSize * gridSize;
        var coordinates = new (double x, double y)[numberOfCities];
        
        int cityIndex = 0;
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                var baseX = col * spacing;
                var baseY = row * spacing;
                
                // Add noise to make it more interesting
                var noiseX = (Random.NextDouble() - 0.5) * spacing * noiseLevel;
                var noiseY = (Random.NextDouble() - 0.5) * spacing * noiseLevel;
                
                coordinates[cityIndex] = (baseX + noiseX, baseY + noiseY);
                cityIndex++;
            }
        }

        var distanceMatrix = CreateEuclideanDistanceMatrix(coordinates);
        return (distanceMatrix, coordinates);
    }

    #endregion

    #region Solving Methods

    /// <summary>
    /// Solves a random TSP problem and displays simplified results.
    /// </summary>
    public static void SolveRandomTsp(int numberOfCities, double[,] distanceMatrix, (double x, double y)[] coordinates)
    {
        Console.WriteLine($"Random TSP Problem: {numberOfCities} cities");
        
        // Generate initial population
        var populationSize = Math.Max(50, numberOfCities * 6);
        var epochs = Math.Max(100, numberOfCities * 12);
        var initialPopulation = GenerateInitialPopulation(populationSize, numberOfCities, distanceMatrix);
        
        // Calculate initial statistics
        var initialBestDistance = initialPopulation.Min(c => c.GetTotalDistance());
        var initialAvgDistance = initialPopulation.Average(c => c.GetTotalDistance());

        Console.WriteLine($"Initial best distance: {initialBestDistance:F2}");
        Console.WriteLine($"Initial average distance: {initialAvgDistance:F2}");
        Console.WriteLine($"Running GA with {populationSize} population for {epochs} epochs...\n");

        // Configure and run the genetic algorithm
        var runner = OpenGARunner<int>
                        .Initialize(initialPopulation, 0.5f, 1.0f) // min 50%, max 100% (same as initial)
                        .MutationRate(0.15f)
                        .ApplyReproductionSelector(c => c.ApplyElitistReproductionSelector())
                        .Crossover(s => s.Rate(0.85f).RegisterSingle(o => o.OnePointCrossover()))
                        .ApplyReplacementStrategy(c => c.ApplyElitistReplacementStrategy())
                        .ApplyTerminationStrategies(c => c.ApplyMaximumEpochsTerminationStrategy(epochs))
                        ;

        // Run the genetic algorithm
        var bestChromosome = runner.RunToCompletion() as TspChromosome;

        // Get the best solution distance
        var finalBestDistance = bestChromosome!.GetTotalDistance();

        // Calculate improvement percentage (initial best vs final best)
        var improvementVsBest = ((initialBestDistance - finalBestDistance) / initialBestDistance) * 100;

        // Display results
        Console.WriteLine("=== SOLUTION FOUND ===");
        Console.WriteLine($"Initial best distance: {initialBestDistance:F2}");
        Console.WriteLine($"Final best distance: {finalBestDistance:F2}");
        Console.WriteLine($"Improvement: {improvementVsBest:F1}% better");
        Console.WriteLine($"Optimal tour: {string.Join(" → ", bestChromosome.Genes)} → {bestChromosome.Genes[0]}");
    }

    /// <summary>
    /// Solves a TSP problem and displays detailed progress and results.
    /// </summary>
    public static void SolveTsp(string problemName, double[,] distanceMatrix, (double x, double y)[] coordinates, 
                               int populationSize = 100, int epochs = 200, float mutationRate = 0.15f)
    {
        Console.WriteLine($"\n=== {problemName} ===");
        var numberOfCities = coordinates.Length;
        
        Console.WriteLine($"Problem size: {numberOfCities} cities");
        Console.WriteLine("City coordinates:");
        for (int i = 0; i < Math.Min(numberOfCities, 10); i++) // Show first 10 cities only
        {
            Console.WriteLine($"  City {i}: ({coordinates[i].x:F0}, {coordinates[i].y:F0})");
        }
        if (numberOfCities > 10)
        {
            Console.WriteLine($"  ... and {numberOfCities - 10} more cities");
        }

        // Generate initial population
        var initialPopulation = GenerateInitialPopulation(populationSize, numberOfCities, distanceMatrix);
        
        var initialBestDistance = initialPopulation.Min(c => c.GetTotalDistance());
        var initialWorstDistance = initialPopulation.Max(c => c.GetTotalDistance());
        var initialAvgDistance = initialPopulation.Average(c => c.GetTotalDistance());

        Console.WriteLine($"\nInitial Population Statistics:");
        Console.WriteLine($"  Population size: {populationSize}");
        Console.WriteLine($"  Best distance: {initialBestDistance:F2}");
        Console.WriteLine($"  Worst distance: {initialWorstDistance:F2}");
        Console.WriteLine($"  Average distance: {initialAvgDistance:F2}");

        Console.WriteLine($"\nGA Configuration:");
        Console.WriteLine($"  Epochs: {epochs}");
        Console.WriteLine($"  Mutation rate: {mutationRate:P1}");
        Console.WriteLine($"  Crossover rate: 85%");
        Console.WriteLine($"  Selection: Tournament + Elitist");
        Console.WriteLine($"  Replacement: Elitist");

        // Configure and run the genetic algorithm
        Console.WriteLine("\nStarting genetic algorithm...");

        var runner = OpenGARunner<int>
                        .Initialize(initialPopulation, 0.5f, 1.0f) // min 50%, max 100% (same as initial)
                        .MutationRate(mutationRate)
                        .ApplyReproductionSelector(c => c.ApplyElitistReproductionSelector())
                        .Crossover(s => s.Rate(0.85f).RegisterSingle(o => o.OnePointCrossover()))
                        .ApplyReplacementStrategy(c => c.ApplyElitistReplacementStrategy())
                        .ApplyTerminationStrategies(c => c.ApplyMaximumEpochsTerminationStrategy(epochs));

        // Start the genetic algorithm
        var bestChromosome = runner.RunToCompletion() as TspChromosome;

        // Analyze results
        var finalBestDistance = bestChromosome!.GetTotalDistance();

        Console.WriteLine("Genetic algorithm completed!\n");

        Console.WriteLine("=== FINAL RESULTS ===");
        Console.WriteLine($"Best tour distance: {finalBestDistance:F2}");
        Console.WriteLine($"Best tour fitness: {bestChromosome.CalculateFitness():F6}");

        // Calculate improvement
        var improvement = (initialBestDistance - finalBestDistance) / initialBestDistance * 100;

        Console.WriteLine($"\nImprovement:");
        Console.WriteLine($"  Best solution: {improvement:F1}% better ({initialBestDistance:F2} → {finalBestDistance:F2})");

        Console.WriteLine("\nOptimal tour sequence:");
        var tourSequence = string.Join(" → ", bestChromosome.Genes);
        Console.WriteLine($"  {tourSequence} → {bestChromosome.Genes[0]}");

        // Show tour path only for smaller problems
        if (numberOfCities <= 15)
        {
            Console.WriteLine("\nTour path coordinates:");
            for (int i = 0; i < bestChromosome.Genes.Count; i++)
            {
                var cityIndex = bestChromosome.Genes[i];
                var nextCityIndex = bestChromosome.Genes[(i + 1) % bestChromosome.Genes.Count];
                var distance = distanceMatrix[cityIndex, nextCityIndex];
                
                Console.WriteLine($"  City {cityIndex} ({coordinates[cityIndex].x:F0}, {coordinates[cityIndex].y:F0}) → " +
                                $"City {nextCityIndex} ({coordinates[nextCityIndex].x:F0}, {coordinates[nextCityIndex].y:F0}) " +
                                $"[distance: {distance:F1}]");
            }
        }
        else
        {
            Console.WriteLine($"\n(Tour path details omitted for large problem with {numberOfCities} cities)");
        }
    }

    /// <summary>
    /// Solves a complex TSP problem with many cities and detailed analysis.
    /// </summary>
    public static void SolveComplexTsp(int numberOfCities = 50, int numberOfClusters = 5)
    {
        Console.WriteLine($"\n=== COMPLEX TSP CHALLENGE ===");
        Console.WriteLine($"Generating complex TSP problem with {numberOfCities} cities in {numberOfClusters} clusters...\n");

        // Create the complex problem
        var (distanceMatrix, coordinates) = CreateComplexTspProblem(numberOfCities, numberOfClusters);

        // Calculate some problem statistics
        double totalDistance = 0;
        int edgeCount = 0;
        for (int i = 0; i < numberOfCities; i++)
        {
            for (int j = i + 1; j < numberOfCities; j++)
            {
                totalDistance += distanceMatrix[i, j];
                edgeCount++;
            }
        }
        var avgDistance = totalDistance / edgeCount;

        Console.WriteLine($"Problem Statistics:");
        Console.WriteLine($"  Cities: {numberOfCities}");
        Console.WriteLine($"  Clusters: {numberOfClusters}");
        Console.WriteLine($"  Average inter-city distance: {avgDistance:F2}");
        Console.WriteLine($"  Total possible edges: {edgeCount:N0}");

        // Use more aggressive parameters for complex problems
        var populationSize = Math.Max(100, numberOfCities * 4);
        var epochs = Math.Max(300, numberOfCities * 8);
        var mutationRate = 0.12f; // Slightly lower mutation for larger problems

        Console.WriteLine($"\nGA Parameters (tuned for complexity):");
        Console.WriteLine($"  Population size: {populationSize}");
        Console.WriteLine($"  Epochs: {epochs}");
        Console.WriteLine($"  Mutation rate: {mutationRate:P1}");

        SolveTsp($"Complex Clustered TSP ({numberOfCities} cities)", distanceMatrix, coordinates, 
                populationSize, epochs, mutationRate);
    }

    /// <summary>
    /// Runs multiple TSP test cases including complex problems.
    /// </summary>
    public static void RunTestSuite()
    {
        Console.WriteLine("=== TSP Genetic Algorithm Test Suite ===");
        
        // Test Case 1: Small symmetric problem
        Console.WriteLine("\n" + new string('=', 60));
        var (matrix1, coords1) = CreateSampleTspProblem();
        SolveTsp("Small TSP Problem (10 cities)", matrix1, coords1, 50, 100, 0.20f);

        // Test Case 2: Medium random problem
        Console.WriteLine("\n" + new string('=', 60));
        var matrix2 = CreateRandomDistanceMatrix(8, 10, 100);
        var coords2 = GenerateRandomCoordinates(8, 0, 200);
        SolveTsp("Medium Random TSP (8 cities)", matrix2, coords2, 80, 150, 0.15f);

        // Test Case 3: Grid-based problem
        Console.WriteLine("\n" + new string('=', 60));
        var (matrix3, coords3) = CreateGridTspProblem(6, 0.2, 80);
        SolveTsp("Grid-based TSP (36 cities)", matrix3, coords3, 120, 200, 0.12f);

        // Test Case 4: Complex clustered problem
        Console.WriteLine("\n" + new string('=', 60));
        SolveComplexTsp(30, 4);

        // Test Case 5: Large complex problem
        Console.WriteLine("\n" + new string('=', 60));
        SolveComplexTsp(50, 6);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Generates random coordinates within specified bounds.
    /// </summary>
    private static (double x, double y)[] GenerateRandomCoordinates(int count, double min, double max)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var coords = new (double x, double y)[count];
        
        for (int i = 0; i < count; i++)
        {
            coords[i] = (
                random.NextDouble() * (max - min) + min,
                random.NextDouble() * (max - min) + min
            );
        }
        
        return coords;
    }

    #endregion
}
