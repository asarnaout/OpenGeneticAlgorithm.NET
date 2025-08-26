using OpenGA.Net.CrossoverStrategies;

namespace OpenGA.Net.Tests.CrossoverStrategies;

/// <summary>
/// Integration test demonstrating K-Point crossover usage in a genetic algorithm workflow
/// </summary>
public class KPointCrossoverIntegrationTests
{
    [Fact]
    public void KPointCrossover_IntegrationTest_ShouldWorkInGeneticAlgorithmWorkflow()
    {
        // Create a population of dummy chromosomes
        var population = new List<DummyChromosome>
        {
            new([1, 2, 3, 4, 5, 6, 7, 8]),
            new([9, 10, 11, 12, 13, 14, 15, 16]),
            new([17, 18, 19, 20, 21, 22, 23, 24]),
            new([25, 26, 27, 28, 29, 30, 31, 32])
        };

        // Select two parents for crossover
        var parentA = population[0];
        var parentB = population[1];
        var couple = Couple<int>.Pair(parentA, parentB);

        // Configure K-Point crossover strategy via configuration
        var config = new CrossoverStrategyConfiguration<int>();
        var strategy = config.KPointCrossover(3);

        // Perform crossover
        var random = new Random(42);
        var offspring = strategy.Crossover(couple, random).ToList();

        // Verify results
        Assert.Equal(2, offspring.Count);
        Assert.Equal(8, offspring[0].Genes.Count);
        Assert.Equal(8, offspring[1].Genes.Count);

        // Verify that offspring contain genes from both parents
        var offspringA = offspring[0];
        var offspringB = offspring[1];
        
        Assert.True(offspringA.Genes.Intersect(parentA.Genes).Any(), "Offspring A should contain genes from parent A");
        Assert.True(offspringA.Genes.Intersect(parentB.Genes).Any(), "Offspring A should contain genes from parent B");
        Assert.True(offspringB.Genes.Intersect(parentA.Genes).Any(), "Offspring B should contain genes from parent A");
        Assert.True(offspringB.Genes.Intersect(parentB.Genes).Any(), "Offspring B should contain genes from parent B");

        // Verify offspring ages are reset
        Assert.Equal(0, offspringA.Age);
        Assert.Equal(0, offspringB.Age);

        // Verify original parents are unchanged
        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8], parentA.Genes);
        Assert.Equal([9, 10, 11, 12, 13, 14, 15, 16], parentB.Genes);

        // Verify that offspring can be used for fitness calculation
        var fitnessA = offspringA.CalculateFitness();
        var fitnessB = offspringB.CalculateFitness();
        
        Assert.True(fitnessA > 0, "Offspring A should have positive fitness");
        Assert.True(fitnessB > 0, "Offspring B should have positive fitness");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void KPointCrossover_WithDifferentPointCounts_ShouldProduceValidOffspring(int numberOfPoints)
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        var parentB = new DummyChromosome([11, 12, 13, 14, 15, 16, 17, 18, 19, 20]);
        var couple = Couple<int>.Pair(parentA, parentB);

        var config = new CrossoverStrategyConfiguration<int>();
        var strategy = config.KPointCrossover(numberOfPoints);

        var random = new Random(42);
        var offspring = strategy.Crossover(couple, random).ToList();

        Assert.Equal(2, offspring.Count);
        
        foreach (var child in offspring)
        {
            Assert.Equal(10, child.Genes.Count);
            Assert.True(child.Genes.All(gene => gene >= 1 && gene <= 20), 
                "All genes should be from the original parent gene pools");
        }
    }

    [Fact]
    public void KPointCrossover_MultipleGenerations_ShouldMaintainGeneticDiversity()
    {
        var config = new CrossoverStrategyConfiguration<int>();
        var strategy = config.KPointCrossover(2);
        var random = new Random(42);

        // Initial population
        var generation0 = new List<DummyChromosome>
        {
            new([1, 2, 3, 4, 5]),
            new([6, 7, 8, 9, 10]),
            new([11, 12, 13, 14, 15]),
            new([16, 17, 18, 19, 20])
        };

        // Simulate multiple generations
        var currentGeneration = generation0;
        var allUniqueGeneCombinations = new HashSet<string>();

        for (int generation = 0; generation < 5; generation++)
        {
            var nextGeneration = new List<DummyChromosome>();

            // Create offspring from random pairings
            for (int i = 0; i < currentGeneration.Count; i += 2)
            {
                var parentA = currentGeneration[i];
                var parentB = currentGeneration[(i + 1) % currentGeneration.Count];
                var couple = Couple<int>.Pair(parentA, parentB);

                var offspring = strategy.Crossover(couple, random).ToList();
                nextGeneration.AddRange(offspring.Cast<DummyChromosome>());
            }

            // Keep track of unique gene combinations
            foreach (var individual in nextGeneration)
            {
                var geneString = string.Join(",", individual.Genes);
                allUniqueGeneCombinations.Add(geneString);
            }

            currentGeneration = nextGeneration.Take(4).ToList(); // Keep population size constant
        }

        // Should have created multiple unique combinations
        Assert.True(allUniqueGeneCombinations.Count > 5, 
            $"Should have multiple unique combinations, found: {allUniqueGeneCombinations.Count}");
    }

    [Fact]
    public void KPointCrossover_ConfigurationReuse_ShouldWorkCorrectly()
    {
        var config = new CrossoverStrategyConfiguration<int>();
        
        // Configure for 2-point crossover
        var strategy2Point = config.KPointCrossover(2);
        Assert.IsType<KPointCrossoverStrategy<int>>(strategy2Point);
        
        // Reconfigure for 4-point crossover
        var strategy4Point = config.KPointCrossover(4);
        Assert.IsType<KPointCrossoverStrategy<int>>(strategy4Point);

        // Verify the configuration was updated
        Assert.Equal(strategy2Point, config.CrossoverStrategies.First());
        Assert.Equal(strategy4Point, config.CrossoverStrategies.Last());
        Assert.NotEqual(strategy2Point, strategy4Point);
        
        // Test both strategies work
        var parentA = new DummyChromosome([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
        var parentB = new DummyChromosome([11, 12, 13, 14, 15, 16, 17, 18, 19, 20]);
        var couple = Couple<int>.Pair(parentA, parentB);
        var random = new Random(42);
        
        var offspring2Point = strategy2Point.Crossover(couple, random).ToList();
        var offspring4Point = strategy4Point.Crossover(couple, random).ToList();
        
        Assert.Equal(2, offspring2Point.Count);
        Assert.Equal(2, offspring4Point.Count);
        
        // Results should be different due to different number of crossover points
        var genes2Point = string.Join(",", offspring2Point[0].Genes);
        var genes4Point = string.Join(",", offspring4Point[0].Genes);
        Assert.NotEqual(genes2Point, genes4Point);
    }
}
