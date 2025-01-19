using System.Linq.Expressions;

namespace OpenGA.Net;

/// <summary>
/// A Chromosome represents a potential solution to the given optimization problem. Chromosomes are made of 
///  <see cref="Genes">Genes</see> which represent components of the solution. For example, in the Travelling
///  Salesman Problem, a Gene could be an integer representing the city while the chromosome would hold an array
///  of the latter genes representing the sequence of cities to be traversed in order.
/// </summary>
public abstract class Chromosome<T>(IList<T> genes) : IEquatable<Chromosome<T>>
{
    public IList<T> Genes { get; internal set; } = genes;

    internal Guid InternalIdentifier { get; } = Guid.NewGuid();

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

    /// <summary>
    /// The fitness function evaluates the favorability of the chromosome as a potential solution to the optimization
    /// problem. Override this method to return a value reflecting this favorability.
    /// </summary>
    public abstract double CalculateFitness();

    /// <summary>
    /// Override this method to provide a custom Mutation implementation to the Chromosome. An example of a mutation
    /// is to randomly delete a member of the <see cref="Genes">Genes</see> array.
    /// </summary>
    public abstract void Mutate();

    /// <summary>
    /// Since Chromosomes partake in crossover operations, we will need to ensure that we can create deep copies of
    /// the mating chromosome(s). Override the DeepCopy method to provide such an implementation.
    /// </summary>
    public abstract Chromosome<T> DeepCopy();

    /// <summary>
    /// This method is a No-Op by default and will be called on each newly generated chromosome after a crossover operation.
    /// Override to introduce a custom operation to run right on the new chromosome right after its inception. An example of
    /// such an operation is to eliminate any duplicates (if necessary) from the <see cref="Genes">Genes</see> array after
    /// crossover is completed.
    /// </summary>
    public virtual void PostCrossover() => Expression.Empty();

    public override bool Equals(object? obj)
    {
        return Equals(obj as Chromosome<T>);
    }

    public override int GetHashCode()
    {
        return InternalIdentifier.GetHashCode();
    }
}