using OpenGA.Net.ReproductionSelectors;

namespace OpenGA.Net.Tests;

public class ElitistReproductionSelectorTests
{
    [Theory]
    [InlineData(100, 0.2, 20, 100, 100)]
    [InlineData(100, 0.2, 20, 30, 30)]
    [InlineData(100, 0.2, 20, 20, 20)]
    [InlineData(100, 0.2, 20, 10, 10)] //Case of interest
    [InlineData(100, 0.2, 20, 0, 10)] //Case of interest: Elites > minimum
    [InlineData(101, 0.2, 21, 100, 100)]
    [InlineData(101, 0.2, 21, 30, 30)]
    [InlineData(101, 0.2, 21, 21, 21)]
    [InlineData(101, 0.2, 21, 10, 11)] //Case of interest
    [InlineData(101, 0.2, 21, 0, 11)] //Case of interest
    [InlineData(100, 0.33, 33, 33, 33)]
    [InlineData(100, 0.33, 33, 2, 17)] //Case of interest
    [InlineData(100, 0.02, 2, 1, 1)] //Case of interest
    [InlineData(100, 0.01, 1, 1, 0)] //Case of interest
    [InlineData(100, 0.000001, 1, 0, 0)] //Case of interest
    [InlineData(3, 0.1, 1, 0, 0)]
    [InlineData(3, 0.4, 2, 2, 2)] //Case of interest
    [InlineData(3, 0.4, 2, 3, 3)]
    [InlineData(3, 0.9, 3, 3, 3)]
    [InlineData(1, 0.9, 1, 0, 0)] //Case of interest
    [InlineData(1, 0.9, 1, 100, 0)] //Case of interest
    public void OnlyElitesAllowedToMate(int populationSize, double proportionOfElites, int exactNumberOfElites, int minimumNumberOfCouples, int expectedMinimumNumberOfCouples)
    {
        var selector = new ElitistReproductionSelector<int>();

        var random = new Random();

        var population = GenerateRandomPopulation(populationSize, random);

        var config = new ReproductionSelectorConfiguration
        {
            ProportionOfNonElitesAllowedToMate = 0.0d,
            ProportionOfElitesInPopulation = proportionOfElites,
        };

        var result = selector.SelectMatingPairs(population, config, random, minimumNumberOfCouples).ToList();

        Assert.True(result.Count >= expectedMinimumNumberOfCouples);

        var elites = population.OrderByDescending(x => x.CalculateFitness()).Take(exactNumberOfElites).ToList();

        //Asserting all parents are elites
        Assert.True(result.All(x => elites.Any(y => y == x.IndividualA)));
        Assert.True(result.All(x => elites.Any(y => y == x.IndividualB)));

        //Asserting all elites have taken part in the mating process if there is at least 1 elite.
        var allParents = result.Select(y => y.IndividualA).Concat(result.Select(y => y.IndividualB));

        if(expectedMinimumNumberOfCouples > 1)
        {
            Assert.True(elites.All(x => allParents.Any(z => z == x)));
        }
    }

    [Theory]
    [InlineData(100, 0.2, 20, 0.1, 100, 100)]
    [InlineData(100, 0.2, 20, 0.6, 30, 30)]
    [InlineData(100, 0.2, 20, 0.9, 20, 20)]
    [InlineData(100, 0.2, 20, 0.5, 10, 10)]
    [InlineData(100, 0.2, 20, 0.5, 0, 10)] 
    [InlineData(101, 0.2, 21, 0.1, 100, 100)]
    [InlineData(101, 0.2, 21, 0.3, 30, 30)]
    [InlineData(101, 0.2, 21, 0.1, 21, 21)]
    [InlineData(101, 0.2, 21, 0.1, 10, 11)]
    [InlineData(101, 0.2, 21, 0.1, 0, 11)]
    [InlineData(100, 0.33, 33, 0.9, 33, 33)]
    [InlineData(100, 0.33, 33, 0.8, 2, 17)]
    [InlineData(100, 0.02, 2, 0.3, 1, 1)]
    [InlineData(100, 0.01, 1, 0.1, 1, 1)] //Case of interest
    [InlineData(100, 0.000001, 1, 0.000001, 0, 1)] //Case of interest
    [InlineData(100, 0.000001, 1, 0.000001, 1, 1)] //Case of interest
    [InlineData(100, 0.000001, 1, 0.000001, 3, 3)] //Case of interest
    [InlineData(3, 0.1, 1, 0.1, 0, 1)]
    [InlineData(3, 0.4, 2, 0.1, 2, 2)]
    [InlineData(3, 0.4, 2, 0.1, 3, 3)]
    [InlineData(3, 0.9, 3, 0.01, 3, 3)] //Case of interest
    [InlineData(1, 0.9, 1, 0.1, 0, 0)] //Case of interest
    public void NonElitesAllowedToMateWithElites(int populationSize, double proportionOfElites, int exactNumberOfElites, double proportionOfNonElitesAllowedToMate, int minimumNumberOfCouples, int expectedMinimumNumberOfCouples)
    {
        var selector = new ElitistReproductionSelector<int>();

        var random = new Random();

        var population = GenerateRandomPopulation(populationSize, random);

        var config = new ReproductionSelectorConfiguration
        {
            ProportionOfNonElitesAllowedToMate = proportionOfNonElitesAllowedToMate,
            ProportionOfElitesInPopulation = proportionOfElites,
            AllowMatingElitesWithNonElites = true
        };

        var result = selector.SelectMatingPairs(population, config, random, minimumNumberOfCouples).ToList();

        Assert.True(result.Count >= expectedMinimumNumberOfCouples);

        var elites = population.OrderByDescending(x => x.CalculateFitness()).Take(exactNumberOfElites).ToList();

        //Asserting all elites have taken part in the mating process if there is at least one eligible member for the elite to mate with.
        var allParents = result.Select(y => y.IndividualA).Concat(result.Select(y => y.IndividualB));

        if (expectedMinimumNumberOfCouples > 0)
        {
            Assert.True(elites.All(x => allParents.Any(z => z == x)));
        }
    }

    [Theory]
    [InlineData(100, 0.2, 20, 0.1, 100, 100)]
    [InlineData(100, 0.2, 20, 0.6, 30, 30)]
    [InlineData(100, 0.2, 20, 0.9, 20, 20)]
    [InlineData(100, 0.2, 20, 0.5, 10, 10)]
    [InlineData(100, 0.2, 20, 0.5, 0, 10)] 
    [InlineData(101, 0.2, 21, 0.1, 100, 100)]
    [InlineData(101, 0.2, 21, 0.3, 30, 30)]
    [InlineData(101, 0.2, 21, 0.1, 21, 21)]
    [InlineData(101, 0.2, 21, 0.1, 10, 11)]
    [InlineData(101, 0.2, 21, 0.1, 0, 11)]
    [InlineData(100, 0.33, 33, 0.9, 33, 33)]
    [InlineData(100, 0.33, 33, 0.8, 2, 17)]
    [InlineData(100, 0.02, 2, 0.3, 1, 1)]
    [InlineData(100, 0.01, 1, 0.1, 1, 1)]
    [InlineData(100, 0.000001, 1, 0.000001, 0, 0)] //Case of interest, 1 elite, 1 non-elite, both cant mate together
    [InlineData(100, 0.000001, 1, 0.000001, 10, 0)] //Case of interest, 1 elite, 1 non-elite, both cant mate together
    [InlineData(3, 0.1, 1, 0.1, 0, 0)] //Case of interest
    [InlineData(3, 0.4, 2, 0.1, 2, 2)]
    [InlineData(3, 0.4, 2, 0.1, 3, 3)]
    [InlineData(3, 0.9, 3, 0.01, 3, 3)]
    [InlineData(1, 0.9, 1, 0.1, 0, 0)]
    public void NonElitesRestrictedFromMatingWithElites(int populationSize, double proportionOfElites, int exactNumberOfElites, double proportionOfNonElitesAllowedToMate, int minimumNumberOfCouples, int expectedMinimumNumberOfCouples)
    {
        var selector = new ElitistReproductionSelector<int>();

        var random = new Random();

        var population = GenerateRandomPopulation(populationSize, random);

        var config = new ReproductionSelectorConfiguration
        {
            ProportionOfNonElitesAllowedToMate = proportionOfNonElitesAllowedToMate,
            ProportionOfElitesInPopulation = proportionOfElites,
            AllowMatingElitesWithNonElites = false
        };

        var result = selector.SelectMatingPairs(population, config, random, minimumNumberOfCouples).ToList();

        Assert.True(result.Count >= expectedMinimumNumberOfCouples);

        var elites = population.OrderByDescending(x => x.CalculateFitness()).Take(exactNumberOfElites).ToList();

        //Asserting all elites have taken part in the mating process if there is at least one eligible member for the elite to mate with.
        var allParents = result.Select(y => y.IndividualA).Concat(result.Select(y => y.IndividualB));

        if (elites.Count > 1)
        {
            Assert.True(elites.All(x => allParents.Any(z => z == x)));
        }

        //Assert that all couples are made up of either elite or non elite members
        foreach(var couple in result)
        {
            if (elites.Any(x => x.InternalIdentifier == couple.IndividualA.InternalIdentifier))
            {
                Assert.Contains(elites, x => x.InternalIdentifier == couple.IndividualB.InternalIdentifier);
            }
            else
            {
                Assert.DoesNotContain(elites, x => x.InternalIdentifier == couple.IndividualB.InternalIdentifier);
            }
        }
    }

    [Fact]
    public void WillProduceUniformCouplesIfOnlyTwoMembersExistInThePopulation()
    {
        var selector = new ElitistReproductionSelector<int>();

        var random = new Random();

        var population = GenerateRandomPopulation(2, random);

        var config = new ReproductionSelectorConfiguration
        {
            ProportionOfElitesInPopulation = 0.3
        };

        var minimumNumberOfCouples = 100;

        var result = selector.SelectMatingPairs(population, config, random, minimumNumberOfCouples).ToList();

        Assert.Equal(minimumNumberOfCouples, result.Count);

        foreach(var item in result)
        {
            Assert.Equal(population[0], item.IndividualA);
            Assert.Equal(population[1], item.IndividualB);
        }
    }

    private static DummyChromosome[] GenerateRandomPopulation(int size, Random random) =>
        Enumerable.Range(0, size)
            .Select(x => new DummyChromosome(Enumerable.Range(0, 10).Select(y => random.Next()).ToArray()))
            .ToArray();
}
