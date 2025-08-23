using OpenGA.Net.ReproductionSelectors;

namespace OpenGA.Net.Tests.ReproductionSelectors;

public class RandomReproductionSelectorTests
{
    [Fact]
    public void WillFailIfThereThereIsLessThanTwoIndividuals()
    {
        var selector = new RandomReproductionSelector<int>();

        var random = new Random();

        var population = GenerateRandomPopulation(1, random);

        var result = selector.SelectMatingPairs(population, random, 100).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void WillProduceUniformCouplesIfOnlyTwoMembersExistInThePopulation()
    {
        var selector = new RandomReproductionSelector<int>();

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
    public void WillSucceedOtherwise()
    {
        var selector = new RandomReproductionSelector<int>();

        var random = new Random();

        var population = GenerateRandomPopulation(30, random);

        var numberOfCouples = 100;

        var result = selector.SelectMatingPairs(population, random, numberOfCouples).ToList();

        Assert.Equal(numberOfCouples, result.Count);

        //Assert all couples have distinct parents
        foreach(var couple in result)
        {
            Assert.NotEqual(couple.IndividualA.InternalIdentifier, couple.IndividualB.InternalIdentifier);
        }
    }

    private static DummyChromosome[] GenerateRandomPopulation(int size, Random random) =>
        Enumerable.Range(0, size)
            .Select(x => new DummyChromosome(Enumerable.Range(0, 10).Select(y => random.Next()).ToList()))
            .ToArray();
}
