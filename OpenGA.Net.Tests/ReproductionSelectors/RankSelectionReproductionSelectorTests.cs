using OpenGA.Net.ReproductionSelectors;

namespace OpenGA.Net.Tests.ReproductionSelectors;

public class RankSelectionReproductionSelectorTests
{
    [Fact]
    public void WillFailIfThereThereIsLessThanTwoIndividuals()
    {
        var selector = new RankSelectionReproductionSelector<int>();

        var random = new Random();

        var population = GenerateRandomPopulation(1, random);

        var result = selector.SelectMatingPairs(population, random, 100).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void WillProduceUniformCouplesIfOnlyTwoMembersExistInThePopulation()
    {
        var selector = new RankSelectionReproductionSelector<int>();

        var random = new Random();

        var population = GenerateRandomPopulation(2, random);

        var minimumNumberOfCouples = 100;

        var result = selector.SelectMatingPairs(population, random, minimumNumberOfCouples).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);

        foreach(var item in result)
        {
            Assert.Equal(population[0], item.IndividualA);
            Assert.Equal(population[1], item.IndividualB);
        }
    }

    /// <summary>
    /// A note on this test: Since Rank Selection significantly blunts any disproportionate advantage
    /// in fitness, therefore, the test needs to output a much larger number of couples and operate on
    /// a much smaller population compared to the same test run using the FitnessWeightedRouletteWheel.
    /// 
    /// This is intentional and by design. Rank Selection guarantees that disporportionately fit chromosomes
    /// will dominate over a long run, BUT will have a harder time getting there compared to the unbridled 
    /// FitnessWeightedRouletteWheel.
    /// </summary>
    [Fact]
    public void RankSelectionWillPreferTheMostFitChromosomesOverALargeNumberOfRuns()
    {
        var selector = new RankSelectionReproductionSelector<int>();

        var random = new Random();

        var population = Enumerable.Range(0, 50)
            .Select(x => new DummyChromosome(Enumerable.Range(0, 10).Select(y => random.Next(2, 11)).ToList()))
            .ToArray();

        var highestFitnessChromosome = new DummyChromosome(Enumerable.Range(0, 10).Select(x => random.Next(101, 201)).ToList());
        var secondHighestFitnessChromosome = new DummyChromosome(Enumerable.Range(0, 10).Select(x => random.Next(51, 101)).ToList());
        var thirdHighestFitnessChromosome = new DummyChromosome(Enumerable.Range(0, 10).Select(x => random.Next(26, 51)).ToList());
        var fourthHighestFitnessChromosome = new DummyChromosome(Enumerable.Range(0, 10).Select(x => random.Next(11, 26)).ToList());
        var leastFitChromosome = new DummyChromosome(Enumerable.Range(0, 10).Select(x => 1).ToList());

        population = [.. population, highestFitnessChromosome, secondHighestFitnessChromosome, thirdHighestFitnessChromosome, fourthHighestFitnessChromosome, leastFitChromosome];

        population = [.. population.OrderBy(x => random.Next())];

        var numberOfCouples = 1000000;

        var result = selector.SelectMatingPairs(population, random, numberOfCouples).ToList();

        Assert.Equal(numberOfCouples, result.Count);

        //Assert all couples have distinct parents
        foreach(var couple in result)
        {
            Assert.NotEqual(couple.IndividualA.InternalIdentifier, couple.IndividualB.InternalIdentifier);
        }

        var matingCounter = population.ToDictionary(x => x.InternalIdentifier, x => 0);

        foreach(var couple in result)
        {
            matingCounter[couple.IndividualA.InternalIdentifier]++;
            matingCounter[couple.IndividualB.InternalIdentifier]++;
        }

        var sortedPopulationByMatingCount = matingCounter.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

        Assert.Equal(highestFitnessChromosome.InternalIdentifier, sortedPopulationByMatingCount[0]);
        Assert.Equal(secondHighestFitnessChromosome.InternalIdentifier, sortedPopulationByMatingCount[1]);
        Assert.Equal(thirdHighestFitnessChromosome.InternalIdentifier, sortedPopulationByMatingCount[2]);
        Assert.Equal(leastFitChromosome.InternalIdentifier, sortedPopulationByMatingCount[^1]);
    }

    [Fact]
    public void RankSelectionShouldAssignCorrectRanksBasedOnFitness()
    {
        var selector = new RankSelectionReproductionSelector<int>();
        var random = new Random(42); // Fixed seed for reproducibility

        // Create chromosomes with known, distinct fitness values
        var chromosome1 = new DummyChromosome([1, 1, 1]); // Fitness: 1.0 (worst)
        var chromosome2 = new DummyChromosome([2, 2, 2]); // Fitness: 2.0
        var chromosome3 = new DummyChromosome([3, 3, 3]); // Fitness: 3.0
        var chromosome4 = new DummyChromosome([4, 4, 4]); // Fitness: 4.0 (best)

        var population = new[] { chromosome3, chromosome1, chromosome4, chromosome2 }; // Shuffled order

        // Get enough couples to verify ranking behavior
        var couples = selector.SelectMatingPairs(population, random, 10000).ToList();

        var matingCounter = population.ToDictionary(x => x.InternalIdentifier, x => 0);

        foreach(var couple in couples)
        {
            matingCounter[couple.IndividualA.InternalIdentifier]++;
            matingCounter[couple.IndividualB.InternalIdentifier]++;
        }

        // In rank selection, higher fitness should lead to more matings
        // But the difference should be more moderate than fitness-weighted selection
        Assert.True(matingCounter[chromosome4.InternalIdentifier] > matingCounter[chromosome3.InternalIdentifier]);
        Assert.True(matingCounter[chromosome3.InternalIdentifier] > matingCounter[chromosome2.InternalIdentifier]);
        Assert.True(matingCounter[chromosome2.InternalIdentifier] > matingCounter[chromosome1.InternalIdentifier]);
    }

    [Fact]
    public void RankSelectionShouldHandleIdenticalFitnessValues()
    {
        var selector = new RankSelectionReproductionSelector<int>();
        var random = new Random(42);

        // Create chromosomes with identical fitness values
        var chromosome1 = new DummyChromosome([5, 5, 5]); // Fitness: 5.0
        var chromosome2 = new DummyChromosome([5, 5, 5]); // Fitness: 5.0
        var chromosome3 = new DummyChromosome([5, 5, 5]); // Fitness: 5.0

        var population = new[] { chromosome1, chromosome2, chromosome3 };

        var couples = selector.SelectMatingPairs(population, random, 1000).ToList();

        Assert.Equal(1000, couples.Count);

        // With identical fitness, all chromosomes should have roughly equal selection probability
        var matingCounter = population.ToDictionary(x => x.InternalIdentifier, x => 0);

        foreach(var couple in couples)
        {
            matingCounter[couple.IndividualA.InternalIdentifier]++;
            matingCounter[couple.IndividualB.InternalIdentifier]++;
        }

        // When fitness is identical, OrderBy will still assign different ranks (1, 2, 3)
        // This means the selection won't be perfectly uniform, but should still be reasonably balanced
        // Let's verify that no chromosome is completely excluded and all get some selection
        foreach(var count in matingCounter.Values)
        {
            Assert.True(count > 0, "All chromosomes with identical fitness should get some selection");
        }

        // Verify that the difference in selection isn't too extreme
        var minSelections = matingCounter.Values.Min();
        var maxSelections = matingCounter.Values.Max();
        var ratio = (double)maxSelections / minSelections;
        
        // With ranks 1, 2, 3 the ratio should be 3:1 at most, but randomness should reduce this
        Assert.True(ratio < 5.0, $"Selection ratio between most and least selected should be reasonable, but was {ratio}");
    }

    [Fact]
    public void RankSelectionShouldReduceSelectionPressureComparedToFitnessWeighted()
    {
        var rankSelector = new RankSelectionReproductionSelector<int>();
        var fitnessSelector = new FitnessWeightedRouletteWheelReproductionSelector<int>();
        var random = new Random(42);

        // Create population with one very fit chromosome and several average ones
        var superFitChromosome = new DummyChromosome([100, 100, 100]); // Fitness: 100.0
        var averageChromosomes = Enumerable.Range(0, 5)
            .Select(_ => new DummyChromosome([10, 10, 10])) // Fitness: 10.0 each
            .ToArray();

        var population = new Chromosome<int>[] { superFitChromosome }.Concat(averageChromosomes).ToArray();

        var rankCouples = rankSelector.SelectMatingPairs(population, random, 10000).ToList();
        var fitnessCouples = fitnessSelector.SelectMatingPairs(population, new Random(42), 10000).ToList();

        // Count selections for the super fit chromosome in both strategies
        var rankSelections = rankCouples.Count(c => 
            c.IndividualA.InternalIdentifier == superFitChromosome.InternalIdentifier ||
            c.IndividualB.InternalIdentifier == superFitChromosome.InternalIdentifier);

        var fitnessSelections = fitnessCouples.Count(c => 
            c.IndividualA.InternalIdentifier == superFitChromosome.InternalIdentifier ||
            c.IndividualB.InternalIdentifier == superFitChromosome.InternalIdentifier);

        // Rank selection should give the super fit chromosome less dominance than fitness-weighted selection
        Assert.True(rankSelections < fitnessSelections, 
            $"Rank selection should reduce selection pressure. Rank: {rankSelections}, Fitness: {fitnessSelections}");

        // Both should still favor the super fit chromosome over average ones
        var averageSelections = rankCouples.Count(c => 
            averageChromosomes.Any(avg => avg.InternalIdentifier == c.IndividualA.InternalIdentifier ||
                                         avg.InternalIdentifier == c.IndividualB.InternalIdentifier));

        Assert.True(rankSelections > averageSelections / averageChromosomes.Length, 
            "Rank selection should still favor fitter chromosomes");
    }

    [Fact]
    public void RankSelectionShouldProduceValidCouplesWithMinimumRequirement()
    {
        var selector = new RankSelectionReproductionSelector<int>();
        var random = new Random();

        var population = GenerateRandomPopulation(10, random);

        var minimumCouples = 5;
        var couples = selector.SelectMatingPairs(population, random, minimumCouples).ToList();

        Assert.Equal(minimumCouples, couples.Count);

        // Verify all couples have different individuals
        foreach(var couple in couples)
        {
            Assert.NotEqual(couple.IndividualA.InternalIdentifier, couple.IndividualB.InternalIdentifier);
        }

        // Verify all individuals in couples are from the population
        var populationIds = new HashSet<Guid>(population.Select(x => x.InternalIdentifier));
        foreach(var couple in couples)
        {
            Assert.Contains(couple.IndividualA.InternalIdentifier, populationIds);
            Assert.Contains(couple.IndividualB.InternalIdentifier, populationIds);
        }
    }

    private static DummyChromosome[] GenerateRandomPopulation(int size, Random random) =>
        Enumerable.Range(0, size)
            .Select(x => new DummyChromosome(Enumerable.Range(0, 10).Select(y => random.Next()).ToList()))
            .ToArray();
}
