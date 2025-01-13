using OpenGA.Net.CrossoverStrategies;

namespace OpenGA.Net;

public readonly struct Couple<T>
{
    internal Chromosome<T> IndividualA { get; }

    internal Chromosome<T> IndividualB { get; }

    private Couple(Chromosome<T> individualA, Chromosome<T> individualB)
    {
        IndividualA = individualA;
        IndividualB = individualB;
    }

    public static Couple<T> Pair(Chromosome<T> individualA, Chromosome<T> individualB)
    {
        return new Couple<T>(individualA, individualB);
    }
}
