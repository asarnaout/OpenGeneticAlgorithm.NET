using OpenGA.Net.Exceptions;
using OpenGA.Net.ReproductionSelectors;

namespace OpenGA.Net.Tests;

public class ReproductionSelectorConfigurationTests
{
    [Fact]
    public void SingleSelectorShouldHaveWeightOne()
    {
        var config = new ReproductionSelectorConfiguration<float>(){
            ChainOfSelectors = [ new RandomReproductionSelector<float>().Weight(0.9f) ]
        };

        config.NormalizeWeightsToUnity();

        Assert.Single(config.ChainOfSelectors);
        Assert.Equal(1.0f, config.ChainOfSelectors[0].SelectorWeight);
    }

    [Fact]
    public void MultipleSelectorsWithValidWeightsShouldNormalize()
    {
        var config = new ReproductionSelectorConfiguration<float>(){
            ChainOfSelectors = [ 
                new RandomReproductionSelector<float>().Weight(0.2f),
                new RandomReproductionSelector<float>().Weight(0.3f),
                new RandomReproductionSelector<float>().Weight(0.9f)
            ]
        };

        config.NormalizeWeightsToUnity();

        var normalizedWeights = config.ChainOfSelectors.Select(s => s.SelectorWeight).ToList();

        Assert.Equal([0.2f / 1.4f, 0.3f / 1.4f, 0.9f / 1.4f], normalizedWeights);
        Assert.Equal(1.0f, normalizedWeights.Sum());
    }

    [Fact]
    public void AllWeightsZeroShouldDistributeEqually()
    {
        var config = new ReproductionSelectorConfiguration<float>(){
            ChainOfSelectors = [ 
                new RandomReproductionSelector<float>(),
                new RandomReproductionSelector<float>(),
                new RandomReproductionSelector<float>()
            ]
        };

        config.NormalizeWeightsToUnity();

        var normalizedWeights = config.ChainOfSelectors.Select(s => s.SelectorWeight).ToList();

        Assert.All(normalizedWeights, weight => Assert.Equal(1.0f / 3, weight));
        Assert.Equal(1.0f, normalizedWeights.Sum());
    }

    [Fact]
    public void ZeroWeightAmongNonZeroWeightsShouldThrowException()
    {
        var config = new ReproductionSelectorConfiguration<float>(){
            ChainOfSelectors = [ 
                new RandomReproductionSelector<float>().Weight(0.3f),
                new RandomReproductionSelector<float>(),
                new RandomReproductionSelector<float>().Weight(0.4f)
            ]
        };

        Assert.Throws<NullifyingRelativeWeightException>(config.NormalizeWeightsToUnity);
    }

    [Fact]
    public void NoSelectorsShouldDoNothing()
    {
        var config = new ReproductionSelectorConfiguration<float>(){
            ChainOfSelectors = []
        };

        config.NormalizeWeightsToUnity();

        Assert.Empty(config.ChainOfSelectors);
    }

    [Fact]
    public void AlreadyNormalizedShouldRemainUnchanged()
    {
        var config = new ReproductionSelectorConfiguration<float>(){
            ChainOfSelectors = [ 
                new RandomReproductionSelector<float>().Weight(0.5f),
                new RandomReproductionSelector<float>().Weight(0.5f)
            ]
        };

        config.NormalizeWeightsToUnity();

        var normalizedWeights = config.ChainOfSelectors.Select(s => s.SelectorWeight).ToList();
        Assert.Equal([0.5f, 0.5f], normalizedWeights);
        Assert.Equal(1.0f, normalizedWeights.Sum());
    }
}
