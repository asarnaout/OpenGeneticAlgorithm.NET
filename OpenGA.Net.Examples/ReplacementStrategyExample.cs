using OpenGA.Net;
using OpenGA.Net.ReplacementStrategies;

namespace OpenGA.Net.Examples;

/// <summary>
/// Example demonstrating how to use the RandomEliminationReplacementStrategy
/// with truly random elimination behavior
/// </summary>
public static class ReplacementStrategyExample
{
    public static void RunExample()
    {
        var random = new Random(42);

        // Create initial population
        var population = new[]
        {
            new ExampleChromosome([1.0f, 2.0f, 3.0f]),
            new ExampleChromosome([4.0f, 5.0f, 6.0f]),
            new ExampleChromosome([7.0f, 8.0f, 9.0f]),
            new ExampleChromosome([10.0f, 11.0f, 12.0f]),
            new ExampleChromosome([13.0f, 14.0f, 15.0f])
        };

        // Create offspring (new chromosomes generated through crossover)
        var offspring = new[]
        {
            new ExampleChromosome([16.0f, 17.0f, 18.0f]),
            new ExampleChromosome([19.0f, 20.0f, 21.0f])
        };

        // Create replacement strategy for random elimination
        var replacementStrategy = new RandomEliminationReplacementStrategy<float>();

        Console.WriteLine("=== Random Elimination Replacement Strategy Demo ===");
        Console.WriteLine($"Original population size: {population.Length}");
        Console.WriteLine($"Offspring size: {offspring.Length}");
        Console.WriteLine($"Total chromosomes before replacement: {population.Length + offspring.Length}");
        Console.WriteLine("Strategy: Randomly eliminates chromosomes to maintain population size");

        // Apply replacement strategy - population size will be maintained
        var newPopulation = replacementStrategy.ApplyReplacement(
            population, 
            offspring, 
            random);

        Console.WriteLine($"\nNew population size after replacement: {newPopulation.Length}");
        Console.WriteLine("(Note: Size maintained by eliminating exactly the number of offspring added)\n");

        // Display which chromosomes survived
        Console.WriteLine("Final population:");
        for (int i = 0; i < newPopulation.Length; i++)
        {
            var genes = string.Join(", ", newPopulation[i].Genes);
            var isOffspring = offspring.Any(o => o.Genes.SequenceEqual(newPopulation[i].Genes));
            var type = isOffspring ? "[OFFSPRING]" : "[ORIGINAL]";
            Console.WriteLine($"  Chromosome {i + 1}: [{genes}] {type}");
        }

        Console.WriteLine($"\nAll offspring survive, {offspring.Length} original chromosomes randomly eliminated to maintain population size.");
    }
}
