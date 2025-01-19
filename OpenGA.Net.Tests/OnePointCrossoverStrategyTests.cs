using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.Exceptions;

namespace OpenGA.Net.Tests;

public class OnePointCrossoverStrategyTests
{
    [Fact]
    public void CrossoverThrowsOnInvalidChromosome()
    {
        var parentA = new DummyChromosome([]);
        var parentB = new DummyChromosome([6, 7, 8, 9, 10] );
        var couple = Couple<int>.Pair(parentA, parentB);

        var strategy = new TestCrossoverStrategy(1);
        var random = new Random();

        Assert.Throws<InvalidChromosomeException>(() => strategy.Crossover(couple, random).ToList());
    }

    [Fact]
    public void CrossoverEqualLengthGenesSwapsGenesCorrectly()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5] );
        var parentB = new DummyChromosome([6, 7, 8, 9, 10] );

        var couple = Couple<int>.Pair(parentA, parentB);
        
        var strategy = new TestCrossoverStrategy(2);
        var random = new Random();

        var offspring = strategy.Crossover(couple, random).ToList();

        Assert.Equal(2, offspring.Count);

        var offspringA = offspring[0];
        var offspringB = offspring[1];

        Assert.Equal([ 1, 2, 8, 9, 10 ], offspringA.Genes);
        Assert.Equal([6, 7, 3, 4, 5 ], offspringB.Genes);
    }

    [Fact]
    public void CrossoverVariableLengthGenesSwapsGenesCorrectly()
    {
        var parentA = new DummyChromosome([1, 2, 3, 4, 5] );
        var parentB = new DummyChromosome([6, 7] );

        var couple = Couple<int>.Pair(parentA, parentB);
        
        var strategy = new TestCrossoverStrategy(2);
        var random = new Random();

        var offspring = strategy.Crossover(couple, random).ToList();

        Assert.Equal(2, offspring.Count);

        var offspringA = offspring[0];
        var offspringB = offspring[1];

        Assert.Equal([ 1, 2 ], offspringA.Genes);
        Assert.Equal([6, 7, 3, 4, 5 ], offspringB.Genes);
    }
    
    private class TestCrossoverStrategy(int fixedCrossoverPoint) : OnePointCrossoverStrategy<int>
    {
        private readonly int _fixedCrossoverPoint = fixedCrossoverPoint;

        protected internal override int GetCrossoverPoint(Couple<int> couple, Random random)
        {
            return _fixedCrossoverPoint;
        }
    }
}
