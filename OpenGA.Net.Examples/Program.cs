using OpenGA.Net;
using OpenGA.Net.Examples;

Console.WriteLine("=== OpenGA.Net TSP Solver Demo ===");
Console.WriteLine("Choose a demo to run:");
Console.WriteLine("1. Simple Random TSP (8-12 cities)");
Console.WriteLine("2. Complex Clustered TSP (30+ cities)");
Console.WriteLine("3. Large Grid TSP (64 cities)");
Console.WriteLine("4. Extreme Challenge TSP (100 cities)");
Console.WriteLine("5. Full Test Suite");
Console.WriteLine();

Console.Write("Enter your choice (1-5): ");
var choice = Console.ReadLine();

switch (choice)
{
    case "1":
        RunSimpleRandomTsp();
        break;
    case "2":
        RunComplexClusteredTsp();
        break;
    case "3":
        RunLargeGridTsp();
        break;
    case "4":
        RunExtremeChallengeeTsp();
        break;
    case "5":
        TspSolver.RunTestSuite();
        break;
    default:
        Console.WriteLine("Invalid choice. Running simple random TSP...");
        RunSimpleRandomTsp();
        break;
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

static void RunSimpleRandomTsp()
{
    Console.WriteLine("\n=== Simple Random TSP Problem ===");
    
    // Generate a random TSP problem with 8-12 cities
    var random = new Random();
    var numCities = random.Next(8, 13);
    var distanceMatrix = TspSolver.CreateRandomDistanceMatrix(numCities, 10, 150);
    var coordinates = GenerateRandomCoordinates(numCities);

    TspSolver.SolveRandomTsp(numCities, distanceMatrix, coordinates);
}

static void RunComplexClusteredTsp()
{
    Console.WriteLine("\n=== Complex Clustered TSP Challenge ===");
    TspSolver.SolveComplexTsp(35, 5);
}

static void RunLargeGridTsp()
{
    Console.WriteLine("\n=== Large Grid TSP Challenge ===");
    var (distanceMatrix, coordinates) = TspSolver.CreateGridTspProblem(8, 0.25, 100); // 8x8 = 64 cities
    TspSolver.SolveTsp("Large Grid TSP (64 cities)", distanceMatrix, coordinates, 200, 400, 0.10f);
}

static void RunExtremeChallengeeTsp()
{
    Console.WriteLine("\n=== EXTREME TSP CHALLENGE ===");
    Console.WriteLine("WARNING: This will take several minutes to complete!");
    Console.Write("Are you sure you want to continue? (y/N): ");
    
    var confirmation = Console.ReadLine();
    if (confirmation?.ToLower() == "y" || confirmation?.ToLower() == "yes")
    {
        TspSolver.SolveComplexTsp(100, 8);
    }
    else
    {
        Console.WriteLine("Extreme challenge cancelled. Running medium complex problem instead...");
        TspSolver.SolveComplexTsp(50, 6);
    }
}

// Helper method for generating random coordinates
static (double x, double y)[] GenerateRandomCoordinates(int numCities)
{
    var random = new Random();
    var coords = new (double x, double y)[numCities];
    
    for (int i = 0; i < numCities; i++)
    {
        coords[i] = (
            random.NextDouble() * 200 + 10,  // X between 10-210
            random.NextDouble() * 200 + 10   // Y between 10-210
        );
    }
    
    return coords;
}
                