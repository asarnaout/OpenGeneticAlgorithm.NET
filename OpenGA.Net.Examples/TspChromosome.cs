using OpenGA.Net;

namespace OpenGA.Net.Examples;

/// <summary>
/// Represents a chromosome for the Travelling Salesman Problem.
/// Each gene is an integer representing a city, and the chromosome represents
/// a complete tour visiting all cities exactly once.
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
    /// Calculates the fitness of the chromosome as the inverse of the total tour distance.
    /// Lower distance = higher fitness.
    /// </summary>
    public override Task<double> CalculateFitnessAsync()
    {
        double totalDistance = 0;

        for (int i = 0; i < Genes.Count; i++)
        {
            int currentCity = Genes[i];
            int nextCity = Genes[(i + 1) % Genes.Count]; // Wrap around to start city
            totalDistance += _distanceMatrix[currentCity, nextCity];
        }

        // Return inverse of distance (higher fitness = shorter tour)
        // Add 1 to avoid division by zero
        return Task.FromResult(1.0 / (totalDistance + 1));
    }

    /// <summary>
    /// Mutates the chromosome by swapping two random cities in the tour.
    /// </summary>
    public override Task MutateAsync()
    {
        if (Genes.Count < 2) return Task.CompletedTask;

        int index1 = _random.Next(Genes.Count);
        int index2 = _random.Next(Genes.Count);

        // Ensure we're swapping different cities
        while (index1 == index2)
        {
            index2 = _random.Next(Genes.Count);
        }

        // Swap the cities
        (Genes[index1], Genes[index2]) = (Genes[index2], Genes[index1]);
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ensures the chromosome represents a valid TSP tour by removing duplicates
    /// and ensuring all cities are represented exactly once.
    /// </summary>
    public override Task GeneticRepairAsync()
    {
        var totalCities = _distanceMatrix.GetLength(0);
        var presentCities = new HashSet<int>(Genes);
        var missingCities = new List<int>();

        // Find missing cities
        for (int i = 0; i < totalCities; i++)
        {
            if (!presentCities.Contains(i))
            {
                missingCities.Add(i);
            }
        }

        // If we have duplicates or missing cities, fix the tour
        if (missingCities.Count > 0 || Genes.Count != totalCities)
        {
            var validTour = new List<int>();
            var usedCities = new HashSet<int>();

            // Add unique cities from current genes
            foreach (var city in Genes)
            {
                if (city >= 0 && city < totalCities && !usedCities.Contains(city))
                {
                    validTour.Add(city);
                    usedCities.Add(city);
                }
            }

            // Add missing cities
            foreach (var missingCity in missingCities)
            {
                if (validTour.Count < totalCities)
                {
                    validTour.Add(missingCity);
                }
            }

            // Clear and repopulate the genes list
            Genes.Clear();
            foreach (var city in validTour)
            {
                Genes.Add(city);
            }
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a deep copy of the chromosome.
    /// </summary>
    public override Task<Chromosome<int>> DeepCopyAsync()
    {
        var copiedGenes = new List<int>(Genes);
        return Task.FromResult<Chromosome<int>>(new TspChromosome(copiedGenes, _distanceMatrix));
    }

    /// <summary>
    /// Gets the total distance of the tour represented by this chromosome.
    /// </summary>
    public double GetTotalDistance()
    {
        double totalDistance = 0;

        for (int i = 0; i < Genes.Count; i++)
        {
            int currentCity = Genes[i];
            int nextCity = Genes[(i + 1) % Genes.Count];
            totalDistance += _distanceMatrix[currentCity, nextCity];
        }

        return totalDistance;
    }
}
