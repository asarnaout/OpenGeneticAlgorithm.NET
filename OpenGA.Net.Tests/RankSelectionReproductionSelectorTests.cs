using OpenGA.Net.ReproductionSelectors;

namespace OpenGA.Net.Tests;

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

    private static DummyChromosome[] GenerateRandomPopulation(int size, Random random) =>
        Enumerable.Range(0, size)
            .Select(x => new DummyChromosome(Enumerable.Range(0, 10).Select(y => random.Next()).ToList()))
            .ToArray();
}
