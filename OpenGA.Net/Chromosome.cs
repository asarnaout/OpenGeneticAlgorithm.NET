namespace OpenGA.Net;

public abstract class Chromosome<T>(T[] genes) : IEquatable<Chromosome<T>>
{
    public T[] Genes { get; internal set; } = genes;

    public Guid InternalIdentifier { get; } = Guid.NewGuid();

    public abstract double CalculateFitness();

    public bool Equals(Chromosome<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other) || InternalIdentifier == other.InternalIdentifier)
        {
            return true;
        }

        return false;
    }

    public static bool operator ==(Chromosome<T> obj1, Chromosome<T> obj2) => obj1.Equals(obj2);

    public static bool operator !=(Chromosome<T> obj1, Chromosome<T> obj2) => !obj1.Equals(obj2);

    public abstract Task MutateAsync();

    public abstract Task PostCrossoverAsync();

    public override bool Equals(object? obj)
    {
        return Equals(obj as Chromosome<T>);
    }

    public override int GetHashCode()
    {
        return InternalIdentifier.GetHashCode();
    }
}