using OpenGA.Net;
using OpenGA.Net.Examples;

namespace OpenGA.Net.Examples;

/// <summary>
/// Enhanced TSP solver with progress tracking and multiple test cases.
/// </summary>
public static class TspSolver
{
    /// <summary>
    /// Solves a random TSP problem and displays simplified results.
    /// </summary>
    public static void SolveRandomTsp(int numberOfCities, double[,] distanceMatrix, (double x, double y)[] coordinates)
    {
        Console.WriteLine($"Random TSP Problem: {numberOfCities} cities");
        
        // Generate initial population
        var populationSize = Math.Max(50, numberOfCities * 6);
        var epochs = Math.Max(100, numberOfCities * 12);
        var initialPopulation = TspHelper.GenerateInitialPopulation(populationSize, numberOfCities, distanceMatrix);
        
        // Calculate initial statistics
        var initialBestDistance = initialPopulation.Min(c => c.GetTotalDistance());
        var initialAvgDistance = initialPopulation.Average(c => c.GetTotalDistance());

        Console.WriteLine($"Initial best distance: {initialBestDistance:F2}");
        Console.WriteLine($"Initial average distance: {initialAvgDistance:F2}");
        Console.WriteLine($"Running GA with {populationSize} population for {epochs} epochs...\n");

        // Configure and run the genetic algorithm
        var runner = OpenGARunner<int>
                        .Init(initialPopulation)
                        .Epochs(epochs)
                        .MaxPopulationSize(populationSize)
                        .MutationRate(0.15f)
                        .CrossoverRate(0.85f)
                        .ApplyReproductionSelector(c => c.ApplyElitistReproductionSelector())
                        .ApplyCrossoverStrategy(c => c.ApplyOnePointCrossoverStrategy())
                        .ApplyReplacementStrategy(c => c.ApplyElitistReplacementStrategy());

        // Run the genetic algorithm
        var finalPopulation = runner.RunToCompletion();

        // Get the best solution
        var bestChromosome = finalPopulation.OrderByDescending(c => c.CalculateFitness()).First() as TspChromosome;
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
    /// Solves a TSP problem and displays progress and results.
    /// </summary>
    public static void SolveTsp(string problemName, double[,] distanceMatrix, (double x, double y)[] coordinates, 
                               int populationSize = 100, int epochs = 200, float mutationRate = 0.15f)
    {
        Console.WriteLine($"\n=== {problemName} ===");
        var numberOfCities = coordinates.Length;
        
        Console.WriteLine($"Problem size: {numberOfCities} cities");
        Console.WriteLine("City coordinates:");
        for (int i = 0; i < numberOfCities; i++)
        {
            Console.WriteLine($"  City {i}: ({coordinates[i].x:F0}, {coordinates[i].y:F0})");
        }

        // Generate initial population
        var initialPopulation = TspHelper.GenerateInitialPopulation(populationSize, numberOfCities, distanceMatrix);
        
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
                        .Init(initialPopulation)
                        .Epochs(epochs)
                        .MaxPopulationSize(populationSize)
                        .MutationRate(mutationRate)
                        .CrossoverRate(0.85f)
                        .ApplyReproductionSelector(c => c.ApplyElitistReproductionSelector())
                        .ApplyCrossoverStrategy(c => c.ApplyOnePointCrossoverStrategy())
                        .ApplyReplacementStrategy(c => c.ApplyElitistReplacementStrategy());

        // Start the genetic algorithm
        var finalPopulation = runner.RunToCompletion();

        // Analyze results
        var bestChromosome = finalPopulation.OrderByDescending(c => c.CalculateFitness()).First() as TspChromosome;
        var finalBestDistance = bestChromosome!.GetTotalDistance();
        var finalAvgDistance = finalPopulation.Average(c => (c as TspChromosome)!.GetTotalDistance());

        Console.WriteLine("Genetic algorithm completed!\n");

        Console.WriteLine("=== FINAL RESULTS ===");
        Console.WriteLine($"Best tour distance: {finalBestDistance:F2}");
        Console.WriteLine($"Average distance: {finalAvgDistance:F2}");
        Console.WriteLine($"Best tour fitness: {bestChromosome.CalculateFitness():F6}");

        // Calculate improvement
        var improvement = (initialBestDistance - finalBestDistance) / initialBestDistance * 100;
        var avgImprovement = (initialAvgDistance - finalAvgDistance) / initialAvgDistance * 100;

        Console.WriteLine($"\nImprovement:");
        Console.WriteLine($"  Best solution: {improvement:F1}% better ({initialBestDistance:F2} → {finalBestDistance:F2})");
        Console.WriteLine($"  Average solution: {avgImprovement:F1}% better ({initialAvgDistance:F2} → {finalAvgDistance:F2})");

        Console.WriteLine("\nOptimal tour sequence:");
        var tourSequence = string.Join(" → ", bestChromosome.Genes);
        Console.WriteLine($"  {tourSequence} → {bestChromosome.Genes[0]}");

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

    /// <summary>
    /// Runs multiple TSP test cases.
    /// </summary>
    public static void RunTestSuite()
    {
        Console.WriteLine("=== TSP Genetic Algorithm Test Suite ===");
        
        // Test Case 1: Small symmetric problem
        Console.WriteLine("\n" + new string('=', 60));
        var (matrix1, coords1) = TspHelper.CreateSampleTspProblem();
        SolveTsp("Small TSP Problem (10 cities)", matrix1, coords1, 50, 100, 0.20f);

        // Test Case 2: Medium random problem
        Console.WriteLine("\n" + new string('=', 60));
        var matrix2 = TspHelper.CreateRandomDistanceMatrix(8, 10, 100);
        var coords2 = GenerateRandomCoordinates(8, 0, 200);
        SolveTsp("Medium Random TSP (8 cities)", matrix2, coords2, 80, 150, 0.15f);

        // Test Case 3: Tiny problem for testing
        Console.WriteLine("\n" + new string('=', 60));
        var coords3 = new (double x, double y)[]
        {
            (0, 0), (10, 0), (10, 10), (0, 10), (5, 5)
        };
        var matrix3 = TspHelper.CreateEuclideanDistanceMatrix(coords3);
        SolveTsp("Tiny Square TSP (5 cities)", matrix3, coords3, 30, 80, 0.25f);
    }

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
}
