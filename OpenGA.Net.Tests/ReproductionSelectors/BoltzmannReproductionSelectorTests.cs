using OpenGA.Net.ParentSelectors;

namespace OpenGA.Net.Tests.ParentSelectors;

public class BoltzmannParentSelectorTests
{
    [Fact]
    public void Constructor_WithValidDecayRate_ShouldCreateInstance()
    {
    var selector = new BoltzmannParentSelector<int>(0.01);
        Assert.NotNull(selector);
    }

    [Fact]
    public void Constructor_WithZeroDecayRate_ShouldCreateInstance()
    {
    var selector = new BoltzmannParentSelector<int>(0.0);
        Assert.NotNull(selector);
    }

    [Fact]
    public void Constructor_WithCustomInitialTemperature_ShouldCreateInstance()
    {
    var selector = new BoltzmannParentSelector<int>(0.01, 2.0);
        Assert.NotNull(selector);
    }

    [Fact]
    public void Constructor_WithLinearDecay_ShouldCreateInstance()
    {
    var selector = new BoltzmannParentSelector<int>(0.01, 1.0, useExponentialDecay: false);
        Assert.NotNull(selector);
    }

    [Fact]
    public void Constructor_WithExponentialDecay_ShouldCreateInstance()
    {
    var selector = new BoltzmannParentSelector<int>(0.01, 1.0, useExponentialDecay: true);
        Assert.NotNull(selector);
    }

    [Fact]
    public void SelectMatingPairs_WithEmptyPopulation_ShouldReturnEmptyResult()
    {
    var selector = new BoltzmannParentSelector<int>(0.01);
        var random = new Random();
        var population = Array.Empty<DummyChromosome>();

        var result = selector.SelectMatingPairs(population, random, 100).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void SelectMatingPairs_WithSingleIndividual_ShouldReturnEmptyResult()
    {
    var selector = new BoltzmannParentSelector<int>(0.01);
        var random = new Random();
        var population = GenerateRandomPopulation(1, random);

        var result = selector.SelectMatingPairs(population, random, 100).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void SelectMatingPairs_WithTwoIndividuals_ShouldProduceUniformCouples()
    {
    var selector = new BoltzmannParentSelector<int>(0.01);
        var random = new Random();
        var population = GenerateRandomPopulation(2, random);
        var minimumNumberOfCouples = 100;

        var result = selector.SelectMatingPairs(population, random, minimumNumberOfCouples).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);

        foreach (var couple in result)
        {
            Assert.Equal(population[0], couple.IndividualA);
            Assert.Equal(population[1], couple.IndividualB);
        }
    }

    [Fact]
    public void SelectMatingPairs_ShouldReturnRequestedNumberOfCouples()
    {
    var selector = new BoltzmannParentSelector<int>(0.01);
        var random = new Random();
        var population = GenerateRandomPopulation(10, random);
        var minimumNumberOfCouples = 50;

        var result = selector.SelectMatingPairs(population, random, minimumNumberOfCouples).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);
    }

    [Fact]
    public void SelectMatingPairs_ShouldProduceDistinctParentsInEachCouple()
    {
    var selector = new BoltzmannParentSelector<int>(0.01);
        var random = new Random();
        var population = GenerateRandomPopulation(10, random);
        var minimumNumberOfCouples = 100;

        var result = selector.SelectMatingPairs(population, random, minimumNumberOfCouples).ToList();

        foreach (var couple in result)
        {
            Assert.NotEqual(couple.IndividualA.InternalIdentifier, couple.IndividualB.InternalIdentifier);
        }
    }

    [Fact]
    public void SelectMatingPairs_AtEpochZero_ShouldUseInitialTemperature()
    {
    var selector = new BoltzmannParentSelector<int>(0.1); // High decay rate
        var random = new Random(42); // Fixed seed for reproducibility

        // Create population with clear fitness hierarchy
        var population = new List<DummyChromosome>();
        
        // Add many low-fitness chromosomes
        for (int i = 0; i < 100; i++)
        {
            population.Add(new DummyChromosome(Enumerable.Range(0, 10).Select(x => 1).ToList())); // Fitness ≈ 1
        }
        
        // Add few high-fitness chromosomes
        var highFitnessChromosome = new DummyChromosome(Enumerable.Range(0, 10).Select(x => 100).ToList()); // Fitness = 100
        var secondHighFitnessChromosome = new DummyChromosome(Enumerable.Range(0, 10).Select(x => 90).ToList()); // Fitness = 90
        
        population.Add(highFitnessChromosome);
        population.Add(secondHighFitnessChromosome);

        var populationArray = population.ToArray();
        var numberOfCouples = 10000;

        // At epoch 0, temperature should be 1.0 (initial), allowing for exploration
        var result = selector.SelectMatingPairs(populationArray, random, numberOfCouples, 0).ToList();

        // Count how many times each chromosome was selected
        var selectionCounter = populationArray.ToDictionary(x => x.InternalIdentifier, x => 0);

        foreach (var couple in result)
        {
            selectionCounter[couple.IndividualA.InternalIdentifier]++;
            selectionCounter[couple.IndividualB.InternalIdentifier]++;
        }

        var highFitnessSelections = selectionCounter[highFitnessChromosome.InternalIdentifier];
        var secondHighFitnessSelections = selectionCounter[secondHighFitnessChromosome.InternalIdentifier];
        var averageLowFitnessSelections = selectionCounter
            .Where(kv => kv.Key != highFitnessChromosome.InternalIdentifier && kv.Key != secondHighFitnessChromosome.InternalIdentifier)
            .Average(kv => kv.Value);

        // At epoch 0 with temperature 1.0, high-fitness chromosomes should be favored but not dominantly
        Assert.True(highFitnessSelections > averageLowFitnessSelections, 
            $"High fitness chromosome should be selected more frequently. Got {highFitnessSelections} vs average {averageLowFitnessSelections}");
        Assert.True(secondHighFitnessSelections > averageLowFitnessSelections,
            $"Second high fitness chromosome should be selected more frequently. Got {secondHighFitnessSelections} vs average {averageLowFitnessSelections}");
    }

    [Fact]
    public void SelectMatingPairs_AtLaterEpochs_ShouldShowIncreasedSelectionPressure()
    {
    var selector = new BoltzmannParentSelector<int>(0.1); // Decay rate of 0.1 per epoch
        var random = new Random(42); // Fixed seed for reproducibility

        // Create population with clear fitness hierarchy
        var population = new List<DummyChromosome>();
        
        // Add low-fitness chromosomes
        for (int i = 0; i < 50; i++)
        {
            population.Add(new DummyChromosome(Enumerable.Range(0, 10).Select(x => 1).ToList())); // Fitness ≈ 1
        }
        
        // Add one high-fitness chromosome
        var highFitnessChromosome = new DummyChromosome(Enumerable.Range(0, 10).Select(x => 100).ToList()); // Fitness = 100
        population.Add(highFitnessChromosome);

        var populationArray = population.ToArray();
        var numberOfCouples = 5000;

        // Test at epoch 0 (temperature = 1.0)
        var epoch0Result = selector.SelectMatingPairs(populationArray, random, numberOfCouples, 0).ToList();
        var epoch0HighFitnessSelections = CountChromosomeSelections(epoch0Result, highFitnessChromosome);

        // Test at epoch 5 (temperature = 1.0 - 0.1*5 = 0.5)
        var epoch5Result = selector.SelectMatingPairs(populationArray, random, numberOfCouples, 5).ToList();
        var epoch5HighFitnessSelections = CountChromosomeSelections(epoch5Result, highFitnessChromosome);

        // Test at epoch 10 (temperature = 1.0 - 0.1*10 = 0, which becomes epsilon)
        var epoch10Result = selector.SelectMatingPairs(populationArray, random, numberOfCouples, 10).ToList();
        var epoch10HighFitnessSelections = CountChromosomeSelections(epoch10Result, highFitnessChromosome);

        // As epochs progress and temperature decreases, selection pressure should increase
        Assert.True(epoch5HighFitnessSelections >= epoch0HighFitnessSelections,
            $"Epoch 5 should show equal or higher selection pressure than epoch 0. Epoch 0: {epoch0HighFitnessSelections}, Epoch 5: {epoch5HighFitnessSelections}");
        
        Assert.True(epoch10HighFitnessSelections >= epoch5HighFitnessSelections,
            $"Epoch 10 should show equal or higher selection pressure than epoch 5. Epoch 5: {epoch5HighFitnessSelections}, Epoch 10: {epoch10HighFitnessSelections}");
    }

    [Fact]
    public void SelectMatingPairs_WithZeroDecayRate_ShouldMaintainConstantTemperature()
    {
    var selector = new BoltzmannParentSelector<int>(0.0); // No decay
        var random = new Random(42);
        
        // Create population with clear fitness hierarchy
        var population = new List<DummyChromosome>();
        
        // Add low-fitness chromosomes
        for (int i = 0; i < 50; i++)
        {
            population.Add(new DummyChromosome(Enumerable.Range(0, 10).Select(x => 1).ToList())); // Fitness ≈ 1
        }
        
        // Add one high-fitness chromosome
        var highFitnessChromosome = new DummyChromosome(Enumerable.Range(0, 10).Select(x => 100).ToList()); // Fitness = 100
        population.Add(highFitnessChromosome);

        var populationArray = population.ToArray();
        var numberOfCouples = 5000;

        // Test at different epochs - should show similar behavior
        var epoch0Result = selector.SelectMatingPairs(populationArray, random, numberOfCouples, 0).ToList();
        var epoch0HighFitnessSelections = CountChromosomeSelections(epoch0Result, highFitnessChromosome);

        var epoch10Result = selector.SelectMatingPairs(populationArray, random, numberOfCouples, 10).ToList();
        var epoch10HighFitnessSelections = CountChromosomeSelections(epoch10Result, highFitnessChromosome);

        // With no decay, behavior should be similar across epochs (allowing for some variance due to randomness)
        var difference = Math.Abs(epoch10HighFitnessSelections - epoch0HighFitnessSelections);
        var tolerance = numberOfCouples * 0.1; // Allow 10% variance
        
        Assert.True(difference <= tolerance,
            $"With no decay, selection behavior should be similar across epochs. Epoch 0: {epoch0HighFitnessSelections}, Epoch 10: {epoch10HighFitnessSelections}, Difference: {difference}, Tolerance: {tolerance}");
    }

    [Fact]
    public void SelectMatingPairs_ExponentialDecayVsLinearDecay_ShouldShowDifferentBehavior()
    {
    var exponentialSelector = new BoltzmannParentSelector<int>(0.1, 1.0, useExponentialDecay: true);
    var linearSelector = new BoltzmannParentSelector<int>(0.1, 1.0, useExponentialDecay: false);
        var random = new Random(42);
        
        // Create population with clear fitness hierarchy
        var population = new List<DummyChromosome>();
        for (int i = 0; i < 50; i++)
        {
            population.Add(new DummyChromosome(Enumerable.Range(0, 10).Select(x => 1).ToList())); // Fitness ≈ 1
        }
        
        var highFitnessChromosome = new DummyChromosome(Enumerable.Range(0, 10).Select(x => 100).ToList()); // Fitness = 100
        population.Add(highFitnessChromosome);

        var populationArray = population.ToArray();
        var numberOfCouples = 5000;

        // Test at epoch 10 where difference should be more pronounced
        var exponentialResult = exponentialSelector.SelectMatingPairs(populationArray, random, numberOfCouples, 10).ToList();
        var exponentialHighFitnessSelections = CountChromosomeSelections(exponentialResult, highFitnessChromosome);

        var linearResult = linearSelector.SelectMatingPairs(populationArray, random, numberOfCouples, 10).ToList();
        var linearHighFitnessSelections = CountChromosomeSelections(linearResult, highFitnessChromosome);

        // Exponential decay should maintain higher temperature at epoch 10 (1.0 * e^(-0.1*10) ≈ 0.368)
        // Linear decay would be at 0 temperature (1.0 - 0.1*10 = 0, clamped to epsilon)
        // So linear should show stronger selection pressure (more high-fitness selections)
        Assert.True(linearHighFitnessSelections >= exponentialHighFitnessSelections,
            $"Linear decay should show stronger selection pressure than exponential at epoch 10. Linear: {linearHighFitnessSelections}, Exponential: {exponentialHighFitnessSelections}");
    }

    [Fact]
    public void SelectMatingPairs_WithZeroFitnessChromosomes_ShouldHandleGracefully()
    {
    var selector = new BoltzmannParentSelector<int>(0.01);
        var random = new Random();
        
        // Create chromosomes with zero fitness
        var population = new DummyChromosome[]
        {
            new DummyChromosome(Enumerable.Range(0, 10).Select(x => 0).ToList()), // Fitness = 0
            new DummyChromosome(Enumerable.Range(0, 10).Select(x => 0).ToList()), // Fitness = 0
            new DummyChromosome(Enumerable.Range(0, 10).Select(x => 1).ToList())  // Fitness = 1
        };

        var result = selector.SelectMatingPairs(population, random, 10).ToList();

        Assert.Equal(10, result.Count);
        
        // Should not throw any exceptions and should produce valid couples
        foreach (var couple in result)
        {
            Assert.NotNull(couple.IndividualA);
            Assert.NotNull(couple.IndividualB);
            Assert.NotEqual(couple.IndividualA.InternalIdentifier, couple.IndividualB.InternalIdentifier);
        }
    }

    [Fact]
    public void SelectMatingPairs_WithoutEpochParameter_ShouldDefaultToEpochZero()
    {
    var selector = new BoltzmannParentSelector<int>(0.1);
        var random = new Random(42);
        
        // Create simple population
        var population = new List<DummyChromosome>();
        for (int i = 0; i < 10; i++)
        {
            population.Add(new DummyChromosome(Enumerable.Range(0, 10).Select(x => i + 1).ToList()));
        }

        var populationArray = population.ToArray();
        var numberOfCouples = 100;

        // Call without epoch parameter (should default to epoch 0)
        var resultWithoutEpoch = selector.SelectMatingPairs(populationArray, random, numberOfCouples).ToList();
        
        // Call with explicit epoch 0
        var resultWithEpoch0 = selector.SelectMatingPairs(populationArray, random, numberOfCouples, 0).ToList();

        // Both should have the same number of couples
        Assert.Equal(numberOfCouples, resultWithoutEpoch.Count);
        Assert.Equal(numberOfCouples, resultWithEpoch0.Count);
    }

    [Fact]
    public void Configuration_WithInvalidInitialTemperature_ShouldThrowException()
    {
    var config = new ParentSelectorConfiguration<int>();
        
    Assert.Throws<ArgumentException>(() => config.Boltzmann(0.01, -1.0));
    Assert.Throws<ArgumentException>(() => config.Boltzmann(0.01, 0.0));
    Assert.Throws<ArgumentException>(() => config.BoltzmannWithLinearDecay(0.01, -1.0));
    Assert.Throws<ArgumentException>(() => config.BoltzmannWithLinearDecay(0.01, 0.0));
    }

    [Fact]
    public void Configuration_WithValidParameters_ShouldCreateSelector()
    {
        var config = new ParentSelectorConfiguration<int>();
        
        config.Boltzmann(); // Exponential defaults
        Assert.NotNull(config.ParentSelector);
        
        config.BoltzmannWithLinearDecay(); // Linear defaults
        Assert.NotNull(config.ParentSelector);
        
        config.Boltzmann(0.1, 2.0); // Custom exponential
        Assert.NotNull(config.ParentSelector);
        
        config.BoltzmannWithLinearDecay(0.05, 3.0); // Custom linear
        Assert.NotNull(config.ParentSelector);
    }

    private static int CountChromosomeSelections(List<Couple<int>> couples, DummyChromosome chromosome)
    {
        return couples.Count(couple => 
            couple.IndividualA.InternalIdentifier == chromosome.InternalIdentifier ||
            couple.IndividualB.InternalIdentifier == chromosome.InternalIdentifier);
    }

    private static DummyChromosome[] GenerateRandomPopulation(int size, Random random) =>
        Enumerable.Range(0, size)
            .Select(x => new DummyChromosome(Enumerable.Range(0, 10).Select(y => random.Next(1, 11)).ToList()))
            .ToArray();
}
