using OpenGA.Net;
using OpenGA.Net.Examples;

Console.WriteLine("=== Travelling Salesman Problem Solver Using Genetic Algorithm ===");

// Option 1: Run a comprehensive test suite
Console.WriteLine("\nChoose an option:");
Console.WriteLine("1. Run comprehensive test suite");
Console.WriteLine("2. Run single TSP problem");
Console.WriteLine("3. Run custom problem");

Console.Write("\nEnter your choice (1-3, or any other key for option 1): ");
var choice = Console.ReadLine();

switch (choice)
{
    case "1":
        TspSolver.RunTestSuite();
        break;
        
    case "2":
        // Single problem example
        var (distanceMatrix, coordinates) = TspHelper.CreateSampleTspProblem();
        TspSolver.SolveTsp("Standard TSP Problem", distanceMatrix, coordinates);
        break;
        
    case "3":
        // Custom problem with user input
        Console.Write("\nEnter number of cities (4-15): ");
        if (int.TryParse(Console.ReadLine(), out int numCities) && numCities >= 4 && numCities <= 15)
        {
            var customMatrix = TspHelper.CreateRandomDistanceMatrix(numCities, 5, 150);
            var customCoords = GenerateGridCoordinates(numCities);
            TspSolver.SolveTsp($"Custom TSP Problem ({numCities} cities)", customMatrix, customCoords, 
                              Math.Max(50, numCities * 8), Math.Max(100, numCities * 15));
        }
        else
        {
            Console.WriteLine("Invalid input. Running default problem...");
            goto case "2";
        }
        break;
        
    default:
        TspSolver.RunTestSuite();
        break;
}

Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("TSP solving completed! Press any key to exit...");
Console.ReadKey();

// Helper method for generating grid-based coordinates
static (double x, double y)[] GenerateGridCoordinates(int numCities)
{
    var coords = new (double x, double y)[numCities];
    var gridSize = (int)Math.Ceiling(Math.Sqrt(numCities));
    
    for (int i = 0; i < numCities; i++)
    {
        var row = i / gridSize;
        var col = i % gridSize;
        coords[i] = (col * 50 + 25, row * 50 + 25);
    }
    
    return coords;
}
                