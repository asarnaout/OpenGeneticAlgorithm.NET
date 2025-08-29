using OpenGA.Net.ParentSelectorStrategies;

namespace OpenGA.Net.Tests.ParentSelectorStrategies;

public class TournamentParentSelectorStrategyTests
{
    [Fact]
    public async Task WillFailIfThereThereIsLessThanTwoIndividuals()
    {
        var selector = new TournamentParentSelectorStrategy<int>(false);

        var random = new Random();

        var population = GenerateRandomPopulation(1, random);

        var result = (await selector.SelectMatingPairsAsync(population, random, 100)).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public async Task WillProduceUniformCouplesIfOnlyTwoMembersExistInThePopulation()
    {
        var selector = new TournamentParentSelectorStrategy<int>(false);

        var random = new Random();

        var population = GenerateRandomPopulation(2, random);

        var minimumNumberOfCouples = 100;

        var result = (await selector.SelectMatingPairsAsync(population, random, minimumNumberOfCouples)).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);

        foreach(var item in result)
        {
            Assert.Equal(population[0], item.IndividualA);
            Assert.Equal(population[1], item.IndividualB);
        }
    }

    [Fact]
    public async Task WillRunIfTheTournamentSizeIsLargerThanThePopulationSize()
    {
        var selector = new TournamentParentSelectorStrategy<int>(true);

        var random = new Random();

        var population = GenerateRandomPopulation(3, random);

        var minimumNumberOfCouples = 100;

        var result = (await selector.SelectMatingPairsAsync(population, random, minimumNumberOfCouples)).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);
    }

    [Fact]
    public async Task WillRunOnNonStochasticTournaments()
    {
        var selector = new TournamentParentSelectorStrategy<int>(false);

        var random = new Random();

        var population = GenerateRandomPopulation(3, random);

        var minimumNumberOfCouples = 100;

        var result = (await selector.SelectMatingPairsAsync(population, random, minimumNumberOfCouples)).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);

        var populationWithFitness = new List<(DummyChromosome chromosome, double fitness)>();
        foreach (var chromosome in population)
        {
            var fitness = await chromosome.CalculateFitnessAsync();
            populationWithFitness.Add((chromosome, fitness));
        }
        var populationOrderedByFitness = populationWithFitness.OrderByDescending(x => x.fitness).Select(x => x.chromosome).ToList();

        foreach(var item in result)
        {
            Assert.True(populationOrderedByFitness[0] == item.IndividualA || populationOrderedByFitness[1] == item.IndividualA);
            Assert.True(populationOrderedByFitness[0] == item.IndividualB || populationOrderedByFitness[1] == item.IndividualB);
        }
    }

    [Fact]
    public async Task WillRunWithStochasticTournaments()
    {
        var selector = new TournamentParentSelectorStrategy<int>(true);

        var random = new Random();

        var population = GenerateRandomPopulation(3, random);

        var minimumNumberOfCouples = 100;

        var result = (await selector.SelectMatingPairsAsync(population, random, minimumNumberOfCouples)).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);
    }

    private static DummyChromosome[] GenerateRandomPopulation(int size, Random random) =>
        [.. Enumerable.Range(0, size).Select(x => new DummyChromosome(Enumerable.Range(0, 10).Select(y => random.Next()).ToList()))];
}