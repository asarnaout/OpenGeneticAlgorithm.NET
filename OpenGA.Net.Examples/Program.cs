using OpenGA.Net;
using OpenGA.Net.Examples;

Console.WriteLine("=== Random TSP Problem Solver ===");
Console.WriteLine("Generating random TSP problem and solving...\n");

// Generate a random TSP problem with 8-12 cities
var random = new Random();
var numCities = random.Next(8, 13);
var distanceMatrix = TspHelper.CreateRandomDistanceMatrix(numCities, 10, 150);
var coordinates = GenerateRandomCoordinates(numCities);

// Solve the problem
TspSolver.SolveRandomTsp(numCities, distanceMatrix, coordinates);

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

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
                