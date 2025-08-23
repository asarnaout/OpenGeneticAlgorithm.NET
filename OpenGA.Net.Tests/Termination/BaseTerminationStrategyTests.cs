using OpenGA.Net.Termination;

namespace OpenGA.Net.Tests.Termination;

public class BaseTerminationStrategyTests
{
    // Test implementation of the abstract base class
    private class TestTerminationStrategy<T> : BaseTerminationStrategy<T>
    {
        private readonly bool _shouldTerminate;

        public TestTerminationStrategy(bool shouldTerminate)
        {
            _shouldTerminate = shouldTerminate;
        }

        public override bool Terminate(GeneticAlgorithmState state)
        {
            return _shouldTerminate;
        }
    }

    [Fact]
    public void BaseTerminationStrategy_CanBeInherited()
    {
        // Arrange & Act
        var strategy = new TestTerminationStrategy<int>(true);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<TestTerminationStrategy<int>>(strategy);
    }

    [Fact]
    public void BaseTerminationStrategy_AbstractMethodCanBeOverridden()
    {
        // Arrange
        var trueStrategy = new TestTerminationStrategy<int>(true);
        var falseStrategy = new TestTerminationStrategy<int>(false);
        
        var population = new[]
        {
            new DummyChromosome([1, 2, 3])
        };

        var runner = OpenGARunner<int>.Init(population);
        var state = new GeneticAlgorithmState(0, 100, TimeSpan.Zero, 1.0);

        // Act & Assert
        Assert.True(trueStrategy.Terminate(state));
        Assert.False(falseStrategy.Terminate(state));
    }

    [Fact]
    public void BaseTerminationStrategy_SupportsPolymorphism()
    {
        // Arrange
        BaseTerminationStrategy<int>[] strategies = 
        [
            new TestTerminationStrategy<int>(true),
            new TestTerminationStrategy<int>(false),
            new MaximumEpochsTerminationStrategy<int>(),
            new TargetStandardDeviationTerminationStrategy<int>(0.1),
            new MaximumDurationTerminationStrategy<int>(TimeSpan.FromMinutes(1))
        ];

        var population = new[]
        {
            new DummyChromosome([1, 2, 3])
        };

        var runner = OpenGARunner<int>.Init(population);
        var state = new GeneticAlgorithmState(0, 100, TimeSpan.Zero, 1.0);

        // Act & Assert
        foreach (var strategy in strategies)
        {
            // Should not throw and should return a boolean
            var result = strategy.Terminate(state);
            Assert.IsType<bool>(result);
        }
    }

    [Fact]
    public void BaseTerminationStrategy_GenericTypeParameter_WorksWithDifferentTypes()
    {
        // Arrange
        var intStrategy = new TestTerminationStrategy<int>(true);
        var stringStrategy = new TestTerminationStrategy<string>(false);

        // Assert
        Assert.IsAssignableFrom<BaseTerminationStrategy<int>>(intStrategy);
        Assert.IsAssignableFrom<BaseTerminationStrategy<string>>(stringStrategy);
        
        // Should be different types
        Assert.NotEqual(intStrategy.GetType().GetGenericArguments()[0], 
                       stringStrategy.GetType().GetGenericArguments()[0]);
    }
}
