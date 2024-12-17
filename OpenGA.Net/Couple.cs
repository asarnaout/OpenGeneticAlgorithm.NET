namespace OpenGA.Net;

public readonly struct Couple<T>
{
    public Chromosome<T> IndividualA { get; }

    public Chromosome<T> IndividualB { get; }

    private Couple(Chromosome<T> individualA, Chromosome<T> individualB)
    {
        IndividualA = individualA;
        IndividualB = individualB;
    }

    public static Couple<T> Pair(Chromosome<T> individualA, Chromosome<T> individualB)
    {
        return new Couple<T>(individualA, individualB);
    }

    public Chromosome<T> Crossover(double crossoverRate)
    {
        return default!;
    }
}
