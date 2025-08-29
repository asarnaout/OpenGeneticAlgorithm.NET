using OpenGA.Net.ParentSelectorStrategies;

namespace OpenGA.Net.Tests.ParentSelectorStrategies;

public class TournamentParentSelectorStrategyTests
{
    [Fact]
    public void WillFailIfThereThereIsLessThanTwoIndividuals()
    {
    var selector = new TournamentParentSelectorStrategy<int>(false);

        var random = new Random();

        var population = GenerateRandomPopulation(1, random);

        var result = selector.SelectMatingPairs(population, random, 100).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void WillProduceUniformCouplesIfOnlyTwoMembersExistInThePopulation()
    {
    var selector = new TournamentParentSelectorStrategy<int>(false);

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

    [Fact]
    public void WillRunIfTheTournamentSizeIsLargerThanThePopulationSize()
    {
    var selector = new TournamentParentSelectorStrategy<int>(true);

        var random = new Random();

        var population = GenerateRandomPopulation(3, random);

        var minimumNumberOfCouples = 100;

        var result = selector.SelectMatingPairs(population, random, minimumNumberOfCouples).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);
    }

    [Fact]
    public void WillRunOnNonStochasticTournaments()
    {
    var selector = new TournamentParentSelectorStrategy<int>(false);

        var random = new Random();

        var population = GenerateRandomPopulation(3, random);

        var minimumNumberOfCouples = 100;

        var result = selector.SelectMatingPairs(population, random, minimumNumberOfCouples).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);

        var populationOrderedByFitness = population.OrderByDescending(x => x.CalculateFitness()).ToList();

        foreach(var item in result)
        {
            Assert.True(populationOrderedByFitness[0] == item.IndividualA || populationOrderedByFitness[1] == item.IndividualA);
            Assert.True(populationOrderedByFitness[0] == item.IndividualB || populationOrderedByFitness[1] == item.IndividualB);
        }
    }

    [Fact]
    public void WillRunWithStochasticTournaments()
    {
    var selector = new TournamentParentSelectorStrategy<int>(true);

        var random = new Random();

        var population = GenerateRandomPopulation(3, random);

        var minimumNumberOfCouples = 100;

        var result = selector.SelectMatingPairs(population, random, minimumNumberOfCouples).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);
    }

    private static DummyChromosome[] GenerateRandomPopulation(int size, Random random) =>
        Enumerable.Range(0, size)
            .Select(x => new DummyChromosome(Enumerable.Range(0, 10).Select(y => random.Next()).ToList()))
            .ToArray();
}