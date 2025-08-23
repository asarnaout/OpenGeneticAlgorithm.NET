using OpenGA.Net.ReplacementStrategies;
using Xunit;

namespace OpenGA.Net.Tests.ReplacementStrategies;

public class AgeBasedReplacementStrategyTests
{
    private class TestChromosome : Chromosome<int>
    {
        public TestChromosome(IList<int> genes) : base(genes) { }

        public override double CalculateFitness() => Genes.Sum();

        public override void Mutate()
        {
            // Simple mutation for testing
            if (Genes.Count > 0)
            {
                var random = new Random();
                var index = random.Next(Genes.Count);
                Genes[index] = random.Next(100);
            }
        }

        public override Chromosome<int> DeepCopy()
        {
            return new TestChromosome(new List<int>(Genes));
        }
    }

    [Fact]
    public void SelectChromosomesForElimination_EmptyPopulation_ReturnsEmpty()
    {
        // Arrange
        var strategy = new AgeBasedReplacementStrategy<int>();
        var population = Array.Empty<Chromosome<int>>();
        var offspring = new[] { new TestChromosome([1, 2, 3]) };
        var random = new Random(42);

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SelectChromosomesForElimination_EmptyOffspring_ReturnsEmpty()
    {
        // Arrange
        var strategy = new AgeBasedReplacementStrategy<int>();
        var population = new[] { new TestChromosome([1, 2, 3]) };
        var offspring = Array.Empty<Chromosome<int>>();
        var random = new Random(42);

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SelectChromosomesForElimination_AllChromosomesAgeZero_SelectsRandomly()
    {
        // Arrange
        var strategy = new AgeBasedReplacementStrategy<int>();
        var population = new[]
        {
            new TestChromosome([1, 2, 3]),
            new TestChromosome([4, 5, 6]),
            new TestChromosome([7, 8, 9])
        };
        var offspring = new[] { new TestChromosome([10, 11, 12]) };
        var random = new Random(42);

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result[0], population);
    }

    [Fact]
    public void SelectChromosomesForElimination_DifferentAges_PrefersOlderChromosomes()
    {
        // Arrange
        var strategy = new AgeBasedReplacementStrategy<int>();
        var youngChromosome = new TestChromosome([1, 2, 3]);
        var oldChromosome = new TestChromosome([4, 5, 6]);
        
        // Manually set ages
        for (int i = 0; i < 10; i++)
        {
            oldChromosome.IncrementAge();
        }
        
        var population = new[] { youngChromosome, oldChromosome };
        var offspring = new[] { new TestChromosome([7, 8, 9]) };
        var random = new Random(42);

        // Act
        var eliminations = new List<Chromosome<int>>();
        for (int i = 0; i < 100; i++) // Run multiple times to check probability
        {
            var result = strategy.SelectChromosomesForElimination(population, offspring, random).Single();
            eliminations.Add(result);
        }

        // Assert
        var oldChromosomeEliminations = eliminations.Count(c => c == oldChromosome);
        var youngChromosomeEliminations = eliminations.Count(c => c == youngChromosome);
        
        // Older chromosome should be eliminated more often than younger one
        Assert.True(oldChromosomeEliminations > youngChromosomeEliminations);
    }

    [Fact]
    public void ApplyReplacement_MaintainsPopulationSize()
    {
        // Arrange
        var strategy = new AgeBasedReplacementStrategy<int>();
        var population = new[]
        {
            new TestChromosome([1, 2, 3]),
            new TestChromosome([4, 5, 6]),
            new TestChromosome([7, 8, 9])
        };
        var offspring = new[] { new TestChromosome([10, 11, 12]) };
        var random = new Random(42);

        // Act
        var newPopulation = strategy.ApplyReplacement(population, offspring, random);

        // Assert
        Assert.Equal(population.Length, newPopulation.Length);
        Assert.Contains(offspring[0], newPopulation);
    }
}
