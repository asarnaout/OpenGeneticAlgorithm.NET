using OpenGA.Net;
using OpenGA.Net.ReplacementStrategies;

namespace OpenGA.Net.Examples;

/// <summary>
/// Example demonstrating how to use the TournamentReplacementStrategy
/// with both deterministic and stochastic tournament selection
/// </summary>
public static class TournamentReplacementExample
{
    public static void RunExample()
    {
        var random = new Random(42);

        Console.WriteLine("=== Tournament Replacement Strategy Demo ===\n");

        // Create initial population with varying fitness levels
        var population = new[]
        {
            new ExampleChromosome([1.0f, 1.0f, 1.0f]), // Fitness = 1 (lowest)
            new ExampleChromosome([2.0f, 2.0f, 2.0f]), // Fitness = 2
            new ExampleChromosome([3.0f, 3.0f, 3.0f]), // Fitness = 3
            new ExampleChromosome([4.0f, 4.0f, 4.0f]), // Fitness = 4
            new ExampleChromosome([5.0f, 5.0f, 5.0f]), // Fitness = 5
            new ExampleChromosome([6.0f, 6.0f, 6.0f])  // Fitness = 6 (highest)
        };

        // Create offspring (new chromosomes generated through crossover)
        var offspring = new[]
        {
            new ExampleChromosome([7.0f, 7.0f, 7.0f]), // High fitness offspring
            new ExampleChromosome([3.5f, 3.5f, 3.5f])  // Medium fitness offspring
        };

        Console.WriteLine($"Original population size: {population.Length}");
        Console.WriteLine($"Offspring size: {offspring.Length}");
        Console.WriteLine("Original population fitness levels:");
        for (int i = 0; i < population.Length; i++)
        {
            Console.WriteLine($"  Chromosome {i + 1}: Fitness = {population[i].CalculateFitness():F1}");
        }

        Console.WriteLine("\n--- Deterministic Tournament Replacement ---");
        RunDeterministicTournamentExample(population, offspring, random);

        Console.WriteLine("\n--- Stochastic Tournament Replacement ---");
        RunStochasticTournamentExample(population, offspring, random);
    }

    private static void RunDeterministicTournamentExample(ExampleChromosome[] population, ExampleChromosome[] offspring, Random random)
    {
        // Create deterministic tournament replacement strategy
        var deterministicStrategy = new TournamentReplacementStrategy<float>(3, false);

        Console.WriteLine($"Tournament size: {deterministicStrategy.TournamentSize}");
        Console.WriteLine($"Stochastic: {deterministicStrategy.StochasticTournament}");

        // Apply replacement strategy
        var newPopulation = deterministicStrategy.ApplyReplacement(population, offspring, random);

        Console.WriteLine($"New population size: {newPopulation.Length}");
        Console.WriteLine("Surviving chromosomes (sorted by fitness):");
        
        var sortedSurvivors = newPopulation.OrderBy(c => c.CalculateFitness()).ToArray();
        for (int i = 0; i < sortedSurvivors.Length; i++)
        {
            var genes = string.Join(", ", sortedSurvivors[i].Genes);
            var isOffspring = offspring.Any(o => o.Genes.SequenceEqual(sortedSurvivors[i].Genes));
            var type = isOffspring ? "[OFFSPRING]" : "[ORIGINAL]";
            Console.WriteLine($"  Chromosome {i + 1}: Fitness = {sortedSurvivors[i].CalculateFitness():F1} {type}");
        }

        Console.WriteLine("Note: In deterministic tournaments, the least fit chromosomes are always eliminated.");
    }

    private static void RunStochasticTournamentExample(ExampleChromosome[] population, ExampleChromosome[] offspring, Random random)
    {
        // Create stochastic tournament replacement strategy
        var stochasticStrategy = new TournamentReplacementStrategy<float>(3, true);

        Console.WriteLine($"Tournament size: {stochasticStrategy.TournamentSize}");
        Console.WriteLine($"Stochastic: {stochasticStrategy.StochasticTournament}");

        // Apply replacement strategy
        var newPopulation = stochasticStrategy.ApplyReplacement(population, offspring, random);

        Console.WriteLine($"New population size: {newPopulation.Length}");
        Console.WriteLine("Surviving chromosomes (sorted by fitness):");
        
        var sortedSurvivors = newPopulation.OrderBy(c => c.CalculateFitness()).ToArray();
        for (int i = 0; i < sortedSurvivors.Length; i++)
        {
            var genes = string.Join(", ", sortedSurvivors[i].Genes);
            var isOffspring = offspring.Any(o => o.Genes.SequenceEqual(sortedSurvivors[i].Genes));
            var type = isOffspring ? "[OFFSPRING]" : "[ORIGINAL]";
            Console.WriteLine($"  Chromosome {i + 1}: Fitness = {sortedSurvivors[i].CalculateFitness():F1} {type}");
        }

        Console.WriteLine("Note: In stochastic tournaments, elimination is probabilistic based on fitness.");
        Console.WriteLine("Lower fitness chromosomes are more likely to be eliminated, but it's not guaranteed.");

        // Demonstrate multiple runs to show variability
        Console.WriteLine("\nRunning stochastic tournament 3 more times to show variability:");
        for (int run = 1; run <= 3; run++)
        {
            var result = stochasticStrategy.ApplyReplacement(population, offspring, new Random(42 + run));
            var minFitness = result.Min(c => c.CalculateFitness());
            var maxFitness = result.Max(c => c.CalculateFitness());
            Console.WriteLine($"  Run {run}: Surviving fitness range = {minFitness:F1} to {maxFitness:F1}");
        }
    }
}
