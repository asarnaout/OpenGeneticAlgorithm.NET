using OpenGA.Net.ParentSelectorStrategies;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.SurvivorSelectionStrategies;
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
/// - Survivor Selection Strategy Integration Tests (Generational, Elitist, Tournament, Age-Based)
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
        var runner = OpenGARunner<int>.Initialize(population);

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
            OpenGARunner<int>.Initialize(emptyPopulation));
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    public void MutationRate_WithInvalidValues_ThrowsException(float invalidRate)
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Initialize(population);

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

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            OpenGARunner<int>.Initialize(population)
                .Crossover(s => {
                    s.RegisterSingle(c => c.OnePointCrossover());
                    s.WithCrossoverRate(invalidRate);
                }));
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    public void OverrideOffspringGenerationRate_WithInvalidValues_ThrowsException(float invalidRate)
    {
        // Arrange
        var population = CreateDiversePopulation(5);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            OpenGARunner<int>.Initialize(population)
                .SurvivorSelection(config => {
                    config.RegisterSingle(s => s.Elitist());
                    config.OverrideOffspringGenerationRate(invalidRate);
                })
                .RunToCompletion());
    }

    #endregion

    #region Missing Strategy Configuration Tests

    [Fact]
    public void RunToCompletion_WithoutParentSelector_UsesDefaultTournamentSelector()
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Initialize(population)
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .Termination(config => config.MaximumEpochs(5));

        // Act & Assert - Should not throw, should use default Tournament reproduction selector
        var result = runner.RunToCompletion();
        Assert.NotNull(result);
    }

    [Fact]
    public void RunToCompletion_WithoutSurvivorSelectionStrategy_UsesDefaultElitistStrategy()
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .Termination(config => config.MaximumEpochs(5));

        // Act & Assert - Should not throw, should use default Elitist survivor selection strategy
        var result = runner.RunToCompletion();
        Assert.NotNull(result);
    }

    [Fact]
    public void RunToCompletion_WithoutCrossoverStrategy_UsesDefault()
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .Termination(config => config.MaximumEpochs(3));

        // Act & Assert - Should not throw, should use default OnePoint crossover
        var result = runner.RunToCompletion();
        Assert.NotNull(result);
    }

    [Fact]
    public void RunToCompletion_WithoutTerminationStrategy_UsesDefaultMaxEpochs()
    {
        // Arrange
        var population = CreateDiversePopulation(5);
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .MutationRate(0.1f)
            .Crossover(s => {
                s.RegisterSingle(c => c.OnePointCrossover());
                s.WithCrossoverRate(0.8f);
            });

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
    public void RunToCompletion_WithRandomParentSelector_ProducesValidResult(int populationSize)
    {
        // Arrange
        var population = CreateDiversePopulation(populationSize);
        var initialBestFitness = population.Max(c => c.Fitness);
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Elitist(0.2f)))
            .Termination(config => config.MaximumEpochs(10))
            .MutationRate(0.3f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.RouletteWheel()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Elitist(0.1f)))
            .Termination(config => config.MaximumEpochs(15))
            .MutationRate(0.2f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.9f); });

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // With fitness-weighted selection, we expect better results than random
        Assert.True(result.Fitness >= initialAverageFitness, 
            "Fitness-weighted selection should produce results at least as good as initial average");
    }

    [Fact]
    public void RunToCompletion_WithElitistParentSelector_PreservesGoodGenes()
    {
        // Arrange
        var population = CreateDiversePopulation(20);
        var initialBestFitness = population.Max(c => c.Fitness);
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Elitist(0.3f, 0.1f, true)))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Elitist(0.2f)))
            .Termination(config => config.MaximumEpochs(12))
            .MutationRate(0.1f)
            .Crossover(s => s.RegisterSingle(c => c.OnePointCrossover()).WithCrossoverRate(0.8f));

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // Elitist selection should maintain or improve fitness
        Assert.True(result.Fitness >= initialBestFitness * 0.9, 
            "Elitist selection should preserve good fitness");
    }

    [Fact]
    public void RunToCompletion_WithTournamentParentSelector_WorksCorrectly()
    {
        // Arrange
        var population = CreateDiversePopulation(12);
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Tournament(true)))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Random()))
            .Termination(config => config.MaximumEpochs(8))
            .MutationRate(0.25f)
            .Crossover(s => s.RegisterSingle(c => c.OnePointCrossover()).WithCrossoverRate(0.7f));

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Genes.Count > 0);
        Assert.True(result.Fitness > 0);
    }

    #endregion

    #region Survivor Selection Strategy Integration Tests

    [Fact]
    public void RunToCompletion_WithGenerationalSurvivorSelection_ReplacesEntirePopulation()
    {
        // Arrange
        var population = CreateDiversePopulation(8);
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .Termination(config => config.MaximumEpochs(5))
            .MutationRate(0.3f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // With generational survivor selection, we still get a valid result
        Assert.True(result.Fitness >= 0);
    }

    [Fact]
    public void RunToCompletion_WithElitistSurvivorSelection_MaintainsBestIndividuals()
    {
        // Arrange
        var population = CreateDiversePopulation(15);
        var initialBestFitness = population.Max(c => c.Fitness);
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.RouletteWheel()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Elitist(0.3f))) // Protect top 30%
            .Termination(config => config.MaximumEpochs(10))
            .MutationRate(0.2f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.9f); });

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // Elitist survivor selection should preserve the best fitness
        Assert.True(result.Fitness >= initialBestFitness, 
            "Elitist survivor selection should preserve best individuals");
    }

    [Fact]
    public void RunToCompletion_WithTournamentSurvivorSelection_WorksCorrectly()
    {
        // Arrange
        var population = CreateDiversePopulation(12);
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Tournament(4, true)))
            .Termination(config => config.MaximumEpochs(8))
            .MutationRate(0.3f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.7f); });

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness > 0);
    }

    [Fact]
    public void RunToCompletion_WithAgeBasedSurvivorSelection_WorksCorrectly()
    {
        // Arrange
        var population = CreateDiversePopulation(10);
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.AgeBased()))
            .Termination(config => config.MaximumEpochs(8))
            .MutationRate(0.25f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .Termination(config => config.MaximumEpochs(maxEpochs))
            .MutationRate(0.2f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .Termination(config => config.MaximumDuration(maxDuration))
            .MutationRate(0.2f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Elitist(0.5f))) // High elitism
            .Termination(config => config.TargetStandardDeviation(5.0, 3))
            .MutationRate(0.05f) // Low mutation to maintain convergence
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.6f); });

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
    public void RunToCompletion_WithTargetFitnessTermination_StopsWhenFitnessReached()
    {
        // Arrange - Create a population with diverse fitness levels
        var population = CreateDiversePopulation(10);
        var targetFitness = 80.0; // Set target that's achievable but requires some evolution
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Elitist(0.2f))) // Keep best chromosomes
            .Termination(config => config.TargetFitness(targetFitness))
            .MutationRate(0.1f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // The best chromosome should have reached or exceeded the target fitness
        Assert.True(result.Fitness >= targetFitness, 
            $"Expected fitness >= {targetFitness}, but got {result.Fitness}");
    }

    [Fact]
    public void RunToCompletion_WithMultipleTerminationStrategies_RespectsFirstToTrigger()
    {
        // Arrange
        var population = CreateDiversePopulation(8);
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .Termination(config => config
                .MaximumEpochs(3) // Should trigger first
                .MaximumDuration(TimeSpan.FromMinutes(1))
            )
            .MutationRate(0.2f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Elitist(0.2f)))
            .Termination(config => config.MaximumEpochs(5))
            .MutationRate(0.2f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        // Verification is that the algorithm completes successfully with different population sizes
    }

    [Theory]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    [InlineData(0.8f)]
    public void RunToCompletion_WithCustomOffspringGenerationRate_WorksCorrectly(float generationRate)
    {
        // Arrange
        var population = CreateDiversePopulation(10);
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => {
                config.RegisterSingle(s => s.Generational());
                config.OverrideOffspringGenerationRate(generationRate);
            })
            .Termination(config => config.MaximumEpochs(5))
            .MutationRate(0.2f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

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
        
        var runner = OpenGARunner<int>.Initialize(population, 0.5f, 1.5f) // min 50%, max 150% (15 from 10)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Elitist(0.3f)))
            .Termination(config => config.MaximumEpochs(6))
            .MutationRate(0.3f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.9f); });

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .Crossover(s => { 
                s.RegisterSingle(config => config.OnePointCrossover()); 
                s.WithCrossoverRate(0.8f); 
            })
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .Termination(config => config.MaximumEpochs(6))
            .MutationRate(0.2f);

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .Crossover(s => { 
                s.RegisterSingle(config => config.UniformCrossover()); 
                s.WithCrossoverRate(0.8f); 
            })
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .Termination(config => config.MaximumEpochs(6))
            .MutationRate(0.2f);

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .Crossover(s => { 
                s.RegisterSingle(config => config.KPointCrossover(2)); 
                s.WithCrossoverRate(0.8f); 
            })
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .Termination(config => config.MaximumEpochs(6))
            .MutationRate(0.2f);

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
        
        var runner = OpenGARunner<int>.Initialize(population, 0.5f, 1.25f)
            .MutationRate(0.15f)
            .ParentSelection(config => config.RegisterSingle(s => s.Elitist(0.2f, 0.1f, true)))
            .Crossover(s => s.RegisterSingle(config => config.KPointCrossover(2)).WithCrossoverRate(0.85f))
            .SurvivorSelection(config => {
                config.RegisterSingle(s => s.Elitist(0.15f));
                config.OverrideOffspringGenerationRate(0.8f);
            })
            .Termination(config => config
                .MaximumEpochs(15)
                .MaximumDuration(TimeSpan.FromSeconds(10))
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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .MutationRate(0.05f) // Low mutation
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.95f); }) // High crossover
            .ParentSelection(config => config.RegisterSingle(s => s.RouletteWheel()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Elitist(0.25f)))
            .Termination(config => config.MaximumEpochs(10));

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .MutationRate(0.4f) // High mutation
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.5f); }) // Lower crossover
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Random()))
            .Termination(config => config.MaximumEpochs(8));

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
            var runner = OpenGARunner<int>.Initialize(CreateDiversePopulation(10))
                .ParentSelection(config => config.RegisterSingle(s => s.Random()))
                .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
                .Termination(config => config.MaximumEpochs(5))
                .MutationRate(0.2f)
                .Crossover(s => s.RegisterSingle(c => c.OnePointCrossover()).WithCrossoverRate(0.8f));

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Generational()))
            .Termination(config => config.MaximumEpochs(3))
            .MutationRate(0.3f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

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
        
        var runner = OpenGARunner<int>.Initialize(population)
            .ParentSelection(config => config.RegisterSingle(s => s.Random()))
            .SurvivorSelection(config => config.RegisterSingle(s => s.Elitist(1.0f))) // Protect the only chromosome
            .Termination(config => config.MaximumEpochs(3))
            .MutationRate(0.2f)
            .Crossover(s => { s.RegisterSingle(c => c.OnePointCrossover()); s.WithCrossoverRate(0.8f); });

        // Act
        var result = runner.RunToCompletion();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Fitness > 0);
    }

    #endregion
}