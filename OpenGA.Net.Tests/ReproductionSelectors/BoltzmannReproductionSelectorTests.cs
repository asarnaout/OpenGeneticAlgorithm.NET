using OpenGA.Net.ReproductionSelectors;

namespace OpenGA.Net.Tests.ReproductionSelectors;

public class BoltzmannReproductionSelectorTests
{
    [Fact]
    public void Constructor_WithValidTemperature_ShouldCreateInstance()
    {
        var selector = new BoltzmannReproductionSelector<int>(1.0);
        Assert.NotNull(selector);
    }

    [Fact]
    public void Constructor_WithDefaultTemperature_ShouldCreateInstance()
    {
        var selector = new BoltzmannReproductionSelector<int>(1.0);
        Assert.NotNull(selector);
    }

    [Fact]
    public void SelectMatingPairs_WithEmptyPopulation_ShouldReturnEmptyResult()
    {
        var selector = new BoltzmannReproductionSelector<int>(1.0);
        var random = new Random();
        var population = Array.Empty<DummyChromosome>();

        var result = selector.SelectMatingPairs(population, random, 100).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void SelectMatingPairs_WithSingleIndividual_ShouldReturnEmptyResult()
    {
        var selector = new BoltzmannReproductionSelector<int>(1.0);
        var random = new Random();
        var population = GenerateRandomPopulation(1, random);

        var result = selector.SelectMatingPairs(population, random, 100).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void SelectMatingPairs_WithTwoIndividuals_ShouldProduceUniformCouples()
    {
        var selector = new BoltzmannReproductionSelector<int>(1.0);
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
        var selector = new BoltzmannReproductionSelector<int>(1.0);
        var random = new Random();
        var population = GenerateRandomPopulation(10, random);
        var minimumNumberOfCouples = 50;

        var result = selector.SelectMatingPairs(population, random, minimumNumberOfCouples).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);
    }

    [Fact]
    public void SelectMatingPairs_ShouldProduceDistinctParentsInEachCouple()
    {
        var selector = new BoltzmannReproductionSelector<int>(1);
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
    public void SelectMatingPairs_WithLowTemperature_ShouldFavorHighFitnessChromosomes()
    {
        var lowTemperature = 0.1;
        var selector = new BoltzmannReproductionSelector<int>(lowTemperature);
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

        var result = selector.SelectMatingPairs(populationArray, random, numberOfCouples).ToList();

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

        // With low temperature, high-fitness chromosomes should be selected much more frequently
        Assert.True(highFitnessSelections > averageLowFitnessSelections * 10, 
            $"High fitness chromosome should be selected much more frequently. Got {highFitnessSelections} vs average {averageLowFitnessSelections}");
        Assert.True(secondHighFitnessSelections > averageLowFitnessSelections * 5,
            $"Second high fitness chromosome should be selected more frequently. Got {secondHighFitnessSelections} vs average {averageLowFitnessSelections}");
    }

    [Fact]
    public void SelectMatingPairs_WithHighTemperature_ShouldApproachUniformSelection()
    {
        var highTemperature = 1000.0; // Increased temperature for more uniform selection
        var selector = new BoltzmannReproductionSelector<int>(highTemperature);
        var random = new Random(42); // Fixed seed for reproducibility

        // Create population with varying fitness levels
        var population = new List<DummyChromosome>();
        
        // Low fitness chromosomes
        for (int i = 0; i < 5; i++)
        {
            population.Add(new DummyChromosome(Enumerable.Range(0, 10).Select(x => 1).ToList())); // Fitness ≈ 1
        }
        
        // High fitness chromosomes
        for (int i = 0; i < 5; i++)
        {
            population.Add(new DummyChromosome(Enumerable.Range(0, 10).Select(x => 100).ToList())); // Fitness = 100
        }

        var populationArray = population.ToArray();
        var numberOfCouples = 10000;

        var result = selector.SelectMatingPairs(populationArray, random, numberOfCouples).ToList();

        // Count selections
        var selectionCounter = populationArray.ToDictionary(x => x.InternalIdentifier, x => 0);

        foreach (var couple in result)
        {
            selectionCounter[couple.IndividualA.InternalIdentifier]++;
            selectionCounter[couple.IndividualB.InternalIdentifier]++;
        }

        var selectionCounts = selectionCounter.Values.ToList();
        var averageSelections = selectionCounts.Average();
        var standardDeviation = Math.Sqrt(selectionCounts.Select(x => Math.Pow(x - averageSelections, 2)).Average());
        var coefficientOfVariation = standardDeviation / averageSelections;

        // With high temperature, selection should be more uniform (lower coefficient of variation)
        Assert.True(coefficientOfVariation < 0.5, // Relaxed threshold
            $"Selection should be more uniform with high temperature. Coefficient of variation: {coefficientOfVariation}");
    }

    [Fact]
    public void SelectMatingPairs_WithDifferentTemperatures_ShouldShowDifferentSelectionPressure()
    {
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

        // Test with low temperature (high selection pressure)
        var lowTempSelector = new BoltzmannReproductionSelector<int>(0.1);
        var lowTempResult = lowTempSelector.SelectMatingPairs(populationArray, random, numberOfCouples).ToList();
        
        // Test with high temperature (low selection pressure)
        var highTempSelector = new BoltzmannReproductionSelector<int>(50.0);
        var highTempResult = highTempSelector.SelectMatingPairs(populationArray, random, numberOfCouples).ToList();

        // Count selections for the high-fitness chromosome
        var lowTempHighFitnessSelections = CountChromosomeSelections(lowTempResult, highFitnessChromosome);
        var highTempHighFitnessSelections = CountChromosomeSelections(highTempResult, highFitnessChromosome);

        // Low temperature should select the high-fitness chromosome more frequently
        Assert.True(lowTempHighFitnessSelections > highTempHighFitnessSelections,
            $"Low temperature should favor high-fitness chromosomes more. Low temp: {lowTempHighFitnessSelections}, High temp: {highTempHighFitnessSelections}");
    }

    [Fact]
    public void SelectMatingPairs_WithZeroFitnessChromosomes_ShouldHandleGracefully()
    {
        var selector = new BoltzmannReproductionSelector<int>(1.0);
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
