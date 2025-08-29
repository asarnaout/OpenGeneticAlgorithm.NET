using OpenGA.Net.SurvivorSelectionStrategies;

namespace OpenGA.Net.Tests.SurvivorSelectionStrategies;

public class ElitistSurvivorSelectionStrategyTests
{
    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    [InlineData(2.0f)]
    public void Elitist_WithInvalidElitePercentage_ShouldThrowException(float invalidPercentage)
    {
        // Arrange
        var configuration = new SurvivorSelectionStrategyConfiguration<int>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => 
            configuration.Elitist(invalidPercentage));
        
        Assert.Contains("Elite percentage must be between 0.0 and 1.0", exception.Message);
    }

    [Fact]
    public async Task SelectChromosomesForElimination_WithEmptyPopulation_ShouldReturnEmpty()
    {
        // Arrange
        var strategy = new ElitistSurvivorSelectionStrategy<int>(0.1f);
        var random = new Random(42);
        var population = Array.Empty<Chromosome<int>>();
        var offspring = new[] { new DummyChromosome([1, 2, 3]) };

        // Act
        var result = await strategy.SelectChromosomesForEliminationAsync(population, offspring, random);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SelectChromosomesForElimination_WithEmptyOffspring_ShouldReturnEmpty()
    {
        // Arrange
        var strategy = new ElitistSurvivorSelectionStrategy<int>(0.1f);
        var random = new Random(42);
        var population = new[] { new DummyChromosome([1, 2, 3]) };
        var offspring = Array.Empty<Chromosome<int>>();

        // Act
        var result = await strategy.SelectChromosomesForEliminationAsync(population, offspring, random);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SelectChromosomesForElimination_ShouldProtectEliteChromosomes()
    {
        // Arrange
        var strategy = new ElitistSurvivorSelectionStrategy<int>(0.2f); // Protect top 20%
        var random = new Random(42);
        
        // Create population with known fitness values (based on average of genes)
        var population = new[]
        {
            new DummyChromosome([1, 1, 1]), // Fitness: 1.0 (lowest)
            new DummyChromosome([2, 2, 2]), // Fitness: 2.0
            new DummyChromosome([3, 3, 3]), // Fitness: 3.0
            new DummyChromosome([4, 4, 4]), // Fitness: 4.0
            new DummyChromosome([5, 5, 5])  // Fitness: 5.0 (highest - elite)
        };
        
        var offspring = new[]
        {
            new DummyChromosome([6, 6, 6]),
            new DummyChromosome([7, 7, 7])
        };

        // Act
        var eliminated = (await strategy.SelectChromosomesForEliminationAsync(population, offspring, random)).ToArray();

        // Assert
        // Should eliminate 2 chromosomes (same as offspring count)
        Assert.Equal(2, eliminated.Length);
        
        // The elite chromosome (highest fitness) should NOT be eliminated
        var eliteChromosome = population[4]; // [5, 5, 5] with fitness 5.0
        Assert.DoesNotContain(eliteChromosome, eliminated);
        
        // All eliminated chromosomes should be from the non-elite pool
        foreach (var eliminatedChromosome in eliminated)
        {
            Assert.Contains(eliminatedChromosome, population);
            Assert.NotEqual(eliteChromosome, eliminatedChromosome);
        }
    }

    [Fact]
    public async Task SelectChromosomesForElimination_WithHighElitePercentage_ShouldLimitEliminations()
    {
        // Arrange
        var strategy = new ElitistSurvivorSelectionStrategy<int>(0.8f); // Protect 80%
        var random = new Random(42);
        
        var population = new[]
        {
            new DummyChromosome([1, 1, 1]),
            new DummyChromosome([2, 2, 2]),
            new DummyChromosome([3, 3, 3]),
            new DummyChromosome([4, 4, 4]),
            new DummyChromosome([5, 5, 5])
        };
        
        var offspring = new[]
        {
            new DummyChromosome([6, 6, 6]),
            new DummyChromosome([7, 7, 7]),
            new DummyChromosome([8, 8, 8])
        };

        // Act
        var eliminated = (await strategy.SelectChromosomesForEliminationAsync(population, offspring, random)).ToArray();

        // Assert
        // With 80% elite protection, only 1 chromosome (20% of 5) can be eliminated
        // Even though we have 3 offspring, we can only eliminate 1 non-elite
        Assert.Single(eliminated);
        
        // The eliminated chromosome should be the lowest fitness one
        var lowestFitnessChromosome = population[0]; // [1, 1, 1] with fitness 1.0
        Assert.Contains(lowestFitnessChromosome, eliminated);
    }

    [Fact]
    public async Task ApplySurvivorSelection_ShouldMaintainElites()
    {
        // Arrange
        var strategy = new ElitistSurvivorSelectionStrategy<int>(0.4f); // Protect top 40%
        var random = new Random(42);
        
        var population = new[]
        {
            new DummyChromosome([1, 1, 1]), // Fitness: 1.0 (will be eliminated)
            new DummyChromosome([2, 2, 2]), // Fitness: 2.0 (will be eliminated)
            new DummyChromosome([3, 3, 3]), // Fitness: 3.0 (elite - protected)
            new DummyChromosome([4, 4, 4]), // Fitness: 4.0 (elite - protected)
            new DummyChromosome([5, 5, 5])  // Fitness: 5.0 (elite - protected)
        };
        
        var offspring = new[]
        {
            new DummyChromosome([6, 6, 6]),
            new DummyChromosome([7, 7, 7])
        };

        // Act
        var newPopulation = await strategy.ApplySurvivorSelectionAsync(population, offspring, random);

        // Assert
        // New population should contain elites + offspring
        Assert.Equal(5, newPopulation.Length); // 3 elites + 2 offspring
        
        // All elites should be preserved
        var elites = population.Skip(2).ToArray(); // Top 3 chromosomes
        foreach (var elite in elites)
        {
            Assert.Contains(elite, newPopulation);
        }
        
        // All offspring should be in new population
        foreach (var child in offspring)
        {
            Assert.Contains(child, newPopulation);
        }
        
        // Low fitness chromosomes should be eliminated
        Assert.DoesNotContain(population[0], newPopulation); // [1, 1, 1]
        Assert.DoesNotContain(population[1], newPopulation); // [2, 2, 2]
    }

    [Fact]
    public async Task SelectChromosomesForElimination_WithZeroElitePercentage_ShouldBehaveLikeRandomElimination()
    {
        // Arrange
        var strategy = new ElitistSurvivorSelectionStrategy<int>(0.0f); // No elites protected
        var random = new Random(42);
        
        var population = new[]
        {
            new DummyChromosome([1, 1, 1]),
            new DummyChromosome([5, 5, 5]) // High fitness, but not protected
        };
        
        var offspring = new[]
        {
            new DummyChromosome([3, 3, 3])
        };

        // Act
        var eliminated = (await strategy.SelectChromosomesForEliminationAsync(population, offspring, random)).ToArray();

        // Assert
        Assert.Single(eliminated);
        Assert.Contains(eliminated[0], population);
        // With 0% elite protection, any chromosome can be eliminated
    }

    [Fact]
    public async Task SelectChromosomesForElimination_WithOneHundredPercentElite_ShouldEliminateNothing()
    {
        // Arrange
        var strategy = new ElitistSurvivorSelectionStrategy<int>(1.0f); // Protect everyone
        var random = new Random(42);
        
        var population = new[]
        {
            new DummyChromosome([1, 1, 1]),
            new DummyChromosome([2, 2, 2])
        };
        
        var offspring = new[]
        {
            new DummyChromosome([3, 3, 3])
        };

        // Act
        var eliminated = (await strategy.SelectChromosomesForEliminationAsync(population, offspring, random)).ToArray();

        // Assert
        Assert.Empty(eliminated); // No one can be eliminated if everyone is elite
    }

    [Fact]
    public async Task SelectChromosomesForElimination_ShouldRespectFitnessOrdering()
    {
        // Arrange
        var strategy = new ElitistSurvivorSelectionStrategy<int>(0.5f); // Protect top 50%
        var random = new Random(42);
        
        var population = new[]
        {
            new DummyChromosome([10, 10, 10]), // Fitness: 10.0 (highest - elite)
            new DummyChromosome([5, 5, 5]),    // Fitness: 5.0 (elite)
            new DummyChromosome([3, 3, 3]),    // Fitness: 3.0 (non-elite)
            new DummyChromosome([1, 1, 1])     // Fitness: 1.0 (lowest - non-elite)
        };
        
        var offspring = new[]
        {
            new DummyChromosome([7, 7, 7]),
            new DummyChromosome([8, 8, 8])
        };

        // Act
        var eliminated = (await strategy.SelectChromosomesForEliminationAsync(population, offspring, random)).ToArray();

        // Assert
        Assert.Equal(2, eliminated.Length);
        
        // Elites (top 50% = top 2) should not be eliminated
        Assert.DoesNotContain(population[0], eliminated); // 10.0 fitness
        Assert.DoesNotContain(population[1], eliminated); // 5.0 fitness
        
        // Non-elites should be the ones eliminated
        Assert.Contains(population[2], eliminated); // 3.0 fitness
        Assert.Contains(population[3], eliminated); // 1.0 fitness
    }
}
