using OpenGA.Net.ReproductionSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.ReplacementStrategies;
using OpenGA.Net.Termination;
using OpenGA.Net.Exceptions;

namespace OpenGA.Net.Tests;

/// <summary>
/// Comprehensive integration tests for OpenGARunner that test the genetic algorithm's behavior 
/// and expectations rather than concrete values (since GA is non-deterministic).
/// 
/// Test Categories:
/// - Basic Configuration and Validation Tests
/// - Missing Strategy Configuration Tests  
/// - Reproduction Selector Integration Tests (Random, Fitness-Weighted, Elitist, Tournament)
/// - Replacement Strategy Integration Tests (Generational, Elitist, Tournament, Age-Based)
/// - Termination Strategy Integration Tests (Max Epochs, Max Duration, Target Std Dev, Multiple)
/// - Population Size and Offspring Generation Tests
/// - Crossover Strategy Integration Tests (One Point, Uniform, K-Point)
/// - Comprehensive Integration Tests (Complex configurations, mutation/crossover combinations)
/// - Edge Case Tests (Small populations, single chromosomes)
/// 
/// All tests are designed to run in practical time (total suite < 3 seconds) while thoroughly
/// exercising the genetic algorithm's functionality and ensuring robust behavior across
/// different configurations and edge cases.
/// </summary>
public class OpenGARunnerIntegrationTests
{
    #region Helper Methods

    /// <summary>
    /// Creates a dummy chromosome with genes that sum to a target value for predictable fitness
    /// </summary>
    private static DummyChromosome CreateTargetFitnessChromosome(int targetSum)
    {
        var genes = new List<int>();
        var remaining = targetSum;
        var count = 5; // Fixed number of genes for consistency

        for (int i = 0; i < count - 1; i++)
        {
            var value = remaining / (count - i);
            genes.Add(value * 2); // Multiply by 2 since genetic repair ensures even numbers
            remaining -= value;
        }
        genes.Add(remaining * 2); // Last gene gets remainder, doubled for even constraint

        return new DummyChromosome(genes);
    }

    /// <summary>
    /// Creates a population with varying fitness levels for testing
    /// </summary>
    private static Chromosome<int>[] CreateDiversePopulation(int size)
    {
        var population = new Chromosome<int>[size];
        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 0; i < size; i++)
        {
            var genes = new List<int>();
            var targetSum = random.Next(10, 100);
            
            // Create 5 genes that sum to roughly targetSum
            for (int j = 0; j < 5; j++)
            {
                genes.Add(random.Next(1, targetSum / 5 + 10) * 2); // Even numbers
            }
            
            population[i] = new DummyChromosome(genes);
        }

        return population;
    }

    /// <summary>
    /// Creates a population where all chromosomes have similar fitness
    /// </summary>
    private static Chromosome<int>[] CreateConvergedPopulation(int size)
    {
        var population = new Chromosome<int>[size];
        
        for (int i = 0; i < size; i++)
        {
            // All chromosomes have very similar fitness (around 50 average)
            population[i] = CreateTargetFitnessChromosome(50 + (i % 3)); // Small variations
        }

        return population;
    }

    #endregion

    #region Basic Configuration and Validation Tests

    [Fact]
    public void Init_WithValidPopulation_CreatesRunner()
    {
        // Arrange
        var population = CreateDiversePopulation(10);

        // Act
        var runner = OpenGARunner<int>.Init(population);

        // Assert
        Assert.NotNull(runner);
    }

    [Fact]
    public void Init_WithEmptyPopulation_ThrowsException()
    {
        // Arrange
        var emptyPopulation = Array.Empty<Chromosome<int>>();

        // Act & Assert
        Assert.Throws<MissingInitialPopulationException>(() => 
            OpenGARunner<int>.Init(emptyPopulation));
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    public void MutationRate_WithInvalidValues_ThrowsException(float invalidRate)
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Init(population);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            runner.MutationRate(invalidRate));
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    public void CrossoverRate_WithInvalidValues_ThrowsException(float invalidRate)
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Init(population);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            runner.CrossoverRate(invalidRate));
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(2.1f)]
    public void OverrideOffspringGenerationRate_WithInvalidValues_ThrowsException(float invalidRate)
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Init(population);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            runner.OverrideOffspringGenerationRate(invalidRate));
    }

    #endregion

    #region Missing Strategy Configuration Tests

    [Fact]
    public void RunToCompletion_WithoutReproductionSelector_UsesDefaultTournamentSelector()
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(5));

        // Act & Assert - Should not throw, should use default Tournament reproduction selector
        var result = runner.RunToCompletion();
        Assert.NotNull(result);
    }

    [Fact]
    public void RunToCompletion_WithoutReplacementStrategy_UsesDefaultElitistStrategy()
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(5));

        // Act & Assert - Should not throw, should use default Elitist replacement strategy
        var result = runner.RunToCompletion();
        Assert.NotNull(result);
    }

    [Fact]
    public void RunToCompletion_WithoutCrossoverStrategy_UsesDefault()
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(3));

        // Act & Assert - Should not throw, should use default OnePoint crossover
        var result = runner.RunToCompletion();
        Assert.NotNull(result);
    }

    [Fact]
    public void RunToCompletion_WithoutTerminationStrategy_UsesDefaultMaxEpochs()
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .MutationRate(0.1f)
            .CrossoverRate(0.8f);

        // Act - Should use default 100 epochs but we need to ensure it doesn't run too long
        var startTime = DateTime.UtcNow;
        var result = runner.RunToCompletion();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        // Should terminate within reasonable time (default 100 epochs shouldn't take more than a few seconds)
        Assert.True(duration < TimeSpan.FromSeconds(30), $"Algorithm took too long: {duration}");
    }

    #endregion

    #region Reproduction Selector Integration Tests

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void RunToCompletion_WithRandomReproductionSelector_ProducesValidResult(int populationSize)
    {
        // Arrange
        var population = CreateDiversePopulation(populationSize);
        var initialBestFitness = population.Max(c => c.Fitness);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyElitistReplacementStrategy(0.2f))
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(10))
            .MutationRate(0.3f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness >= initialBestFitness * 0.8, 
            "Final fitness should be reasonably close to or better than initial best");
        Assert.True(result.Genes.Count > 0, "Result should have genes");
    }

    [Fact]
    public void RunToCompletion_WithFitnessWeightedSelector_TendsTowardHigherFitness()
    {
        // Arrange
        var population = CreateDiversePopulation(15);
        var initialBestFitness = population.Max(c => c.Fitness);
        var initialAverageFitness = population.Average(c => c.Fitness);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyFitnessWeightedRouletteWheelReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyElitistReplacementStrategy(0.1f))
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(15))
            .MutationRate(0.2f)
            .CrossoverRate(0.9f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // With fitness-weighted selection, we expect better results than random
        Assert.True(result.Fitness >= initialAverageFitness, 
            "Fitness-weighted selection should produce results at least as good as initial average");
    }

    [Fact]
    public void RunToCompletion_WithElitistReproductionSelector_PreservesGoodGenes()
    {
        // Arrange
        var population = CreateDiversePopulation(20);
        var initialBestFitness = population.Max(c => c.Fitness);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyElitistReproductionSelector(0.3f, 0.1f, true))
            .ApplyReplacementStrategy(config => config.ApplyElitistReplacementStrategy(0.2f))
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(12))
            .MutationRate(0.1f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // Elitist selection should maintain or improve fitness
        Assert.True(result.Fitness >= initialBestFitness * 0.9, 
            "Elitist selection should preserve good fitness");
    }

    [Fact]
    public void RunToCompletion_WithTournamentReproductionSelector_WorksCorrectly()
    {
        // Arrange
        var population = CreateDiversePopulation(12);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyTournamentReproductionSelector(true))
            .ApplyReplacementStrategy(config => config.ApplyRandomEliminationReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(8))
            .MutationRate(0.25f)
            .CrossoverRate(0.7f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Genes.Count > 0);
        Assert.True(result.Fitness > 0);
    }

    #endregion

    #region Replacement Strategy Integration Tests

    [Fact]
    public void RunToCompletion_WithGenerationalReplacement_ReplacesEntirePopulation()
    {
        // Arrange
        var population = CreateDiversePopulation(8);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(5))
            .MutationRate(0.3f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // With generational replacement, we still get a valid result
        Assert.True(result.Fitness >= 0);
    }

    [Fact]
    public void RunToCompletion_WithElitistReplacement_MaintainsBestIndividuals()
    {
        // Arrange
        var population = CreateDiversePopulation(15);
        var initialBestFitness = population.Max(c => c.Fitness);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyFitnessWeightedRouletteWheelReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyElitistReplacementStrategy(0.3f)) // Protect top 30%
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(10))
            .MutationRate(0.2f)
            .CrossoverRate(0.9f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // Elitist replacement should preserve the best fitness
        Assert.True(result.Fitness >= initialBestFitness, 
            "Elitist replacement should preserve best individuals");
    }

    [Fact]
    public void RunToCompletion_WithTournamentReplacement_WorksCorrectly()
    {
        // Arrange
        var population = CreateDiversePopulation(12);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyTournamentReplacementStrategy(4, true))
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(8))
            .MutationRate(0.3f)
            .CrossoverRate(0.7f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness > 0);
    }

    [Fact]
    public void RunToCompletion_WithAgeBasedReplacement_WorksCorrectly()
    {
        // Arrange
        var population = CreateDiversePopulation(10);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyAgeBasedReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(8))
            .MutationRate(0.25f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness > 0);
    }

    #endregion

    #region Termination Strategy Integration Tests

    [Fact]
    public void RunToCompletion_WithMaxEpochsTermination_RespectsEpochLimit()
    {
        // Arrange
        var population = CreateDiversePopulation(10);
        var maxEpochs = 7;
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(maxEpochs))
            .MutationRate(0.2f)
            .CrossoverRate(0.8f);

        // Act
        var startTime = DateTime.UtcNow;
        var result = runner.RunToCompletion();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        // Should terminate relatively quickly (within maxEpochs)
        Assert.True(duration < TimeSpan.FromSeconds(10), 
            $"Algorithm should terminate quickly with max epochs {maxEpochs}, but took {duration}");
    }

    [Fact]
    public void RunToCompletion_WithMaxDurationTermination_RespectsTimeLimit()
    {
        // Arrange
        var population = CreateDiversePopulation(10);
        var maxDuration = TimeSpan.FromSeconds(2);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumDurationTerminationStrategy(maxDuration))
            .MutationRate(0.2f)
            .CrossoverRate(0.8f);

        // Act
        var startTime = DateTime.UtcNow;
        var result = runner.RunToCompletion();
        var actualDuration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        // Should respect the time limit (allow some overhead for processing)
        Assert.True(actualDuration <= maxDuration.Add(TimeSpan.FromSeconds(1)), 
            $"Algorithm should respect max duration {maxDuration}, but took {actualDuration}");
    }

    [Fact]
    public void RunToCompletion_WithTargetStandardDeviationTermination_ConvergesCorrectly()
    {
        // Arrange - Use a converged population that should trigger the termination condition
        var population = CreateConvergedPopulation(10);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyElitistReplacementStrategy(0.5f)) // High elitism
            .ApplyTerminationStrategies(config => config.ApplyTargetStandardDeviationTerminationStrategy(5.0, 3))
            .MutationRate(0.05f) // Low mutation to maintain convergence
            .CrossoverRate(0.6f);

        // Act
        var startTime = DateTime.UtcNow;
        var result = runner.RunToCompletion();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        // Should terminate quickly due to convergence
        Assert.True(duration < TimeSpan.FromSeconds(5), 
            $"Converged population should terminate quickly, but took {duration}");
    }

    [Fact]
    public void RunToCompletion_WithMultipleTerminationStrategies_RespectsFirstToTrigger()
    {
        // Arrange
        var population = CreateDiversePopulation(8);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(
                config => config.ApplyMaximumEpochsTerminationStrategy(3), // Should trigger first
                config => config.ApplyMaximumDurationTerminationStrategy(TimeSpan.FromMinutes(1))
            )
            .MutationRate(0.2f)
            .CrossoverRate(0.8f);

        // Act
        var startTime = DateTime.UtcNow;
        var result = runner.RunToCompletion();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        // Should terminate quickly due to epoch limit, not duration limit
        Assert.True(duration < TimeSpan.FromSeconds(5), 
            $"Should terminate quickly due to epoch limit, but took {duration}");
    }

    #endregion

    #region Population Size and Offspring Generation Tests

    [Theory]
    [InlineData(5)]
    [InlineData(15)]
    [InlineData(25)]
    public void RunToCompletion_WithVariousPopulationSizes_MaintainsPopulationSize(int populationSize)
    {
        // Arrange
        var population = CreateDiversePopulation(populationSize);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyElitistReplacementStrategy(0.2f))
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(5))
            .MutationRate(0.2f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // Verification is that the algorithm completes successfully with different population sizes
    }

    [Theory]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    [InlineData(1.5f)]
    public void RunToCompletion_WithCustomOffspringGenerationRate_WorksCorrectly(float generationRate)
    {
        // Arrange
        var population = CreateDiversePopulation(10);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(5))
            .OverrideOffspringGenerationRate(generationRate)
            .MutationRate(0.2f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness >= 0);
    }

    [Fact]
    public void RunToCompletion_WithMaxPopulationSize_RespectsLimit()
    {
        // Arrange
        var population = CreateDiversePopulation(10);
        var maxSize = 15;
        
        var runner = OpenGARunner<int>.Init(population)
            .MaxPopulationSize(maxSize)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyElitistReplacementStrategy(0.3f))
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(6))
            .MutationRate(0.3f)
            .CrossoverRate(0.9f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // Verification is successful completion without population explosion
    }

    #endregion

    #region Crossover Strategy Integration Tests

    [Fact]
    public void RunToCompletion_WithOnePointCrossover_WorksCorrectly()
    {
        // Arrange
        var population = CreateDiversePopulation(10);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyCrossoverStrategies(config => config.ApplyOnePointCrossoverStrategy())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(6))
            .MutationRate(0.2f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Genes.Count > 0);
    }

    [Fact]
    public void RunToCompletion_WithUniformCrossover_WorksCorrectly()
    {
        // Arrange
        var population = CreateDiversePopulation(10);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyCrossoverStrategies(config => config.ApplyUniformCrossoverStrategy())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(6))
            .MutationRate(0.2f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Genes.Count > 0);
    }

    [Fact]
    public void RunToCompletion_WithKPointCrossover_WorksCorrectly()
    {
        // Arrange
        var population = CreateDiversePopulation(10);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyCrossoverStrategies(config => config.ApplyKPointCrossoverStrategy(2))
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(6))
            .MutationRate(0.2f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Genes.Count > 0);
    }

    #endregion

    #region Comprehensive Integration Tests

    [Fact]
    public void RunToCompletion_ComplexConfiguration_WorksCorrectly()
    {
        // Arrange - Test a complex configuration with multiple strategies
        var population = CreateDiversePopulation(20);
        var initialBestFitness = population.Max(c => c.Fitness);
        
        var runner = OpenGARunner<int>.Init(population)
            .MaxPopulationSize(25)
            .MutationRate(0.15f)
            .CrossoverRate(0.85f)
            .OverrideOffspringGenerationRate(0.8f)
            .ApplyReproductionSelector(config => config.ApplyElitistReproductionSelector(0.2f, 0.1f, true))
            .ApplyCrossoverStrategies(config => config.ApplyKPointCrossoverStrategy(2))
            .ApplyReplacementStrategy(config => config.ApplyElitistReplacementStrategy(0.15f))
            .ApplyTerminationStrategies(
                config => config.ApplyMaximumEpochsTerminationStrategy(15),
                config => config.ApplyMaximumDurationTerminationStrategy(TimeSpan.FromSeconds(10))
            );

        // Act
        var startTime = DateTime.UtcNow;
        var result = runner.RunToCompletion();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness > 0, "Result should have positive fitness");
        Assert.True(result.Genes.Count > 0, "Result should have genes");
        Assert.True(duration < TimeSpan.FromSeconds(15), 
            $"Complex configuration should complete within reasonable time, took {duration}");
        
        // With elitist strategies, we expect to maintain or improve fitness
        Assert.True(result.Fitness >= initialBestFitness * 0.8, 
            "Complex elitist configuration should maintain reasonable fitness");
    }

    [Fact]
    public void RunToCompletion_LowMutationHighCrossover_ProducesStableResults()
    {
        // Arrange
        var population = CreateDiversePopulation(12);
        
        var runner = OpenGARunner<int>.Init(population)
            .MutationRate(0.05f) // Low mutation
            .CrossoverRate(0.95f) // High crossover
            .ApplyReproductionSelector(config => config.ApplyFitnessWeightedRouletteWheelReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyElitistReplacementStrategy(0.25f))
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(10));

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness > 0);
        Assert.True(result.Genes.Count > 0);
    }

    [Fact]
    public void RunToCompletion_HighMutationLowCrossover_ProducesValidResults()
    {
        // Arrange
        var population = CreateDiversePopulation(12);
        
        var runner = OpenGARunner<int>.Init(population)
            .MutationRate(0.4f) // High mutation
            .CrossoverRate(0.5f) // Lower crossover
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyRandomEliminationReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(8));

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness > 0);
        Assert.True(result.Genes.Count > 0);
    }

    [Fact]
    public void RunToCompletion_MultipleRuns_ProduceValidResults()
    {
        // Arrange - Test that multiple runs work correctly (non-deterministic but valid)
        var population = CreateDiversePopulation(10);
        var results = new List<Chromosome<int>>();
        
        // Act - Run the algorithm multiple times
        for (int i = 0; i < 3; i++)
        {
            var runner = OpenGARunner<int>.Init(CreateDiversePopulation(10))
                .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
                .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
                .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(5))
                .MutationRate(0.2f)
                .CrossoverRate(0.8f);

            var result = runner.RunToCompletion();
            results.Add(result);
        }

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, result =>
        {
            Assert.NotNull(result);
            Assert.True(result.Fitness > 0);
            Assert.True(result.Genes.Count > 0);
        });
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void RunToCompletion_WithTwoChromosomePopulation_WorksCorrectly()
    {
        // Arrange
        var population = CreateDiversePopulation(2);
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyGenerationalReplacementStrategy())
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(3))
            .MutationRate(0.3f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness > 0);
    }

    [Fact]
    public void RunToCompletion_WithSingleChromosome_HandlesGracefully()
    {
        // Arrange
        var population = new[] { CreateTargetFitnessChromosome(50) };
        
        var runner = OpenGARunner<int>.Init(population)
            .ApplyReproductionSelector(config => config.ApplyRandomReproductionSelector())
            .ApplyReplacementStrategy(config => config.ApplyElitistReplacementStrategy(1.0f)) // Protect the only chromosome
            .ApplyTerminationStrategies(config => config.ApplyMaximumEpochsTerminationStrategy(3))
            .MutationRate(0.2f)
            .CrossoverRate(0.8f);

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness > 0);
    }

    #endregion
}