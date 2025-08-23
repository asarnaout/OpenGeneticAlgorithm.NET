using OpenGA.Net.CrossoverStrategies;
using Xunit;

namespace OpenGA.Net.Tests.CrossoverStrategies;

public class CrossoverAgeResetTests
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
    public void OnePointCrossover_OffspringHaveAgeZero()
    {
        // Arrange
        var strategy = new OnePointCrossoverStrategy<int>();
        var parentA = new TestChromosome([1, 2, 3, 4, 5]);
        var parentB = new TestChromosome([6, 7, 8, 9, 10]);
        
        // Age the parents
        for (int i = 0; i < 5; i++)
        {
            parentA.IncrementAge();
            parentB.IncrementAge();
        }
        
        var couple = Couple<int>.Pair(parentA, parentB);
        var random = new Random(42);

        // Act
        var offspring = strategy.Crossover(couple, random).ToList();

        // Assert
        Assert.Equal(2, offspring.Count);
        Assert.All(offspring, child => Assert.Equal(0, child.Age));
        
        // Verify parents still have their original age
        Assert.Equal(5, parentA.Age);
        Assert.Equal(5, parentB.Age);
    }

    [Fact]
    public void UniformCrossover_OffspringHaveAgeZero()
    {
        // Arrange
        var strategy = new UniformCrossoverStrategy<int>();
        var parentA = new TestChromosome([1, 2, 3, 4, 5]);
        var parentB = new TestChromosome([6, 7, 8, 9, 10]);
        
        // Age the parents
        for (int i = 0; i < 10; i++)
        {
            parentA.IncrementAge();
            parentB.IncrementAge();
        }
        
        var couple = Couple<int>.Pair(parentA, parentB);
        var random = new Random(42);

        // Act
        var offspring = strategy.Crossover(couple, random).ToList();

        // Assert
        Assert.Single(offspring);
        Assert.Equal(0, offspring[0].Age);
        
        // Verify parents still have their original age
        Assert.Equal(10, parentA.Age);
        Assert.Equal(10, parentB.Age);
    }

    [Fact]
    public void KPointCrossover_OffspringHaveAgeZero()
    {
        // Arrange
        var strategy = new KPointCrossoverStrategy<int>(2);
        var parentA = new TestChromosome([1, 2, 3, 4, 5]);
        var parentB = new TestChromosome([6, 7, 8, 9, 10]);
        
        // Age the parents
        for (int i = 0; i < 3; i++)
        {
            parentA.IncrementAge();
            parentB.IncrementAge();
        }
        
        var couple = Couple<int>.Pair(parentA, parentB);
        var random = new Random(42);

        // Act
        var offspring = strategy.Crossover(couple, random).ToList();

        // Assert
        Assert.Equal(2, offspring.Count);
        Assert.All(offspring, child => Assert.Equal(0, child.Age));
        
        // Verify parents still have their original age
        Assert.Equal(3, parentA.Age);
        Assert.Equal(3, parentB.Age);
    }
}
