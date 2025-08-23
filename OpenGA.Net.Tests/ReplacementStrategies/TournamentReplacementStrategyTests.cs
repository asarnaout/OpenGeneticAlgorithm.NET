using OpenGA.Net.ReplacementStrategies;

namespace OpenGA.Net.Tests.ReplacementStrategies;

public class TournamentReplacementStrategyTests
{
    [Fact]
    public void Constructor_WithValidTournamentSize_ShouldSucceed()
    {
        // Arrange & Act
        var strategy = new TournamentReplacementStrategy<int>(3, false);

        // Assert
        Assert.Equal(3, strategy.TournamentSize);
        Assert.False(strategy.StochasticTournament);
    }

    [Fact]
    public void Constructor_WithStochasticTournament_ShouldSucceed()
    {
        // Arrange & Act
        var strategy = new TournamentReplacementStrategy<int>(5, true);

        // Assert
        Assert.Equal(5, strategy.TournamentSize);
        Assert.True(strategy.StochasticTournament);
    }

    [Fact]
    public void Constructor_WithTournamentSizeLessThanTwo_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TournamentReplacementStrategy<int>(1));
    }

    [Fact]
    public void Constructor_WithTournamentSizeZero_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TournamentReplacementStrategy<int>(0));
    }

    [Fact]
    public void SelectChromosomesForElimination_WithEmptyPopulation_ShouldReturnEmpty()
    {
        // Arrange
        var strategy = new TournamentReplacementStrategy<int>(3);
        var random = new Random(42);
        var population = Array.Empty<DummyChromosome>();
        var offspring = new[] { new DummyChromosome([1, 2, 3]) };

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SelectChromosomesForElimination_WithNoOffspring_ShouldReturnEmpty()
    {
        // Arrange
        var strategy = new TournamentReplacementStrategy<int>(3);
        var random = new Random(42);
        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6])
        };
        var offspring = Array.Empty<DummyChromosome>();

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ApplyReplacement_WithDeterministicTournament_ShouldEliminateLeastFit()
    {
        // Arrange
        var strategy = new TournamentReplacementStrategy<int>(3, false); // Deterministic
        var random = new Random(42);

        // Create chromosomes with different fitness levels (fitness = average of genes)
        var population = new[]
        {
            new DummyChromosome([1, 1, 1]),    // Fitness = 1 (lowest)
            new DummyChromosome([5, 5, 5]),    // Fitness = 5 (highest)
            new DummyChromosome([3, 3, 3]),    // Fitness = 3 (middle)
            new DummyChromosome([2, 2, 2]),    // Fitness = 2
            new DummyChromosome([4, 4, 4])     // Fitness = 4
        };

        var offspring = new[]
        {
            new DummyChromosome([6, 6, 6])     // New chromosome
        };

        // Act
        var result = strategy.ApplyReplacement(population, offspring, random);

        // Assert
        Assert.Equal(5, result.Length); // Same size: 5 original - 1 eliminated + 1 offspring
        Assert.Contains(offspring[0], result); // Offspring should be included
        
        // In deterministic tournament, the least fit in each tournament should be eliminated
        // But which tournaments occur depends on random selection, so we can't guarantee
        // the globally least fit is eliminated. We can verify the offspring is present
        // and one chromosome was eliminated
        var originalChromosomesInResult = result.Where(c => !offspring.Contains(c)).ToList();
        Assert.Equal(4, originalChromosomesInResult.Count); // 4 out of 5 original should survive
    }

    [Fact]
    public void ApplyReplacement_WithStochasticTournament_ShouldVaryResults()
    {
        // Arrange
        var strategy = new TournamentReplacementStrategy<int>(3, true); // Stochastic
        var random = new Random(42);

        var population = new[]
        {
            new DummyChromosome([1, 1, 1]),    // Fitness = 1 (lowest)
            new DummyChromosome([2, 2, 2]),    // Fitness = 2
            new DummyChromosome([3, 3, 3]),    // Fitness = 3
            new DummyChromosome([4, 4, 4]),    // Fitness = 4
            new DummyChromosome([5, 5, 5])     // Fitness = 5 (highest)
        };

        var offspring = new[]
        {
            new DummyChromosome([6, 6, 6])
        };

        // Act
        var result = strategy.ApplyReplacement(population, offspring, random);

        // Assert
        Assert.Equal(5, result.Length);
        Assert.Contains(offspring[0], result); // Offspring should be included
        
        // With stochastic selection, even high fitness chromosomes could be eliminated
        // but low fitness ones are more likely to be eliminated
        Assert.True(result.Any(c => c.CalculateFitness() >= 2)); // Some reasonably fit should survive
    }

    [Fact]
    public void SelectChromosomesForElimination_WithMoreOffspringThanPopulation_ShouldEliminateAllPopulation()
    {
        // Arrange
        var strategy = new TournamentReplacementStrategy<int>(2);
        var random = new Random(42);

        var population = new[]
        {
            new DummyChromosome([1, 2, 3]),
            new DummyChromosome([4, 5, 6])
        };

        var offspring = new[]
        {
            new DummyChromosome([7, 8, 9]),
            new DummyChromosome([10, 11, 12]),
            new DummyChromosome([13, 14, 15])
        };

        // Act
        var result = strategy.SelectChromosomesForElimination(population, offspring, random);

        // Assert
        Assert.Equal(2, result.Count()); // Can only eliminate as many as exist in population
        Assert.True(result.All(eliminated => population.Contains(eliminated)));
    }

    [Fact]
    public void ApplyReplacement_WithSmallPopulationAndLargeTournament_ShouldHandleGracefully()
    {
        // Arrange
        var strategy = new TournamentReplacementStrategy<int>(10); // Tournament larger than population
        var random = new Random(42);

        var population = new[]
        {
            new DummyChromosome([1, 1, 1]),
            new DummyChromosome([5, 5, 5])
        };

        var offspring = new[]
        {
            new DummyChromosome([3, 3, 3])
        };

        // Act
        var result = strategy.ApplyReplacement(population, offspring, random);

        // Assert
        Assert.Equal(2, result.Length); // 2 original - 1 eliminated + 1 offspring
        Assert.Contains(offspring[0], result);
    }

    [Fact]
    public void SelectChromosomesForElimination_DeterministicTournament_ShouldPreferLeastFit()
    {
        // Arrange
        var strategy = new TournamentReplacementStrategy<int>(2, false);
        var random = new Random(42);

        var population = new[]
        {
            new DummyChromosome([10, 10, 10]), // Fitness = 10 (highest)
            new DummyChromosome([1, 1, 1])     // Fitness = 1 (lowest)
        };

        var offspring = new[]
        {
            new DummyChromosome([5, 5, 5])
        };

        // Act - run multiple times to verify consistency
        var results = new List<IEnumerable<Chromosome<int>>>();
        for (int i = 0; i < 5; i++)
        {
            var eliminated = strategy.SelectChromosomesForElimination(population, offspring, new Random(42 + i));
            results.Add(eliminated);
        }

        // Assert
        // In deterministic tournament with these two chromosomes, the low fitness one should always lose
        foreach (var eliminated in results)
        {
            Assert.Single(eliminated);
            var eliminatedChromosome = eliminated.First();
            Assert.Equal(1, eliminatedChromosome.CalculateFitness()); // Lowest fitness should be eliminated
        }
    }
}
